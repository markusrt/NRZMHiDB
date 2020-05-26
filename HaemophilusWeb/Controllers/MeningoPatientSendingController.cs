﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Tools;
using HaemophilusWeb.Utils;
using HaemophilusWeb.Validators;
using HaemophilusWeb.ViewModels;
using HaemophilusWeb.Views.Utils;

namespace HaemophilusWeb.Controllers
{
    public class MeningoPatientSendingController : PatientSendingControllerBase<PatientSendingViewModel<MeningoPatient, MeningoSending>,
        MeningoPatient, MeningoSending>
    {
        public MeningoPatientSendingController()
            : this(
                new ApplicationDbContextWrapper(new ApplicationDbContext()), new MeningoPatientController(),
                new MeningoSendingController())
        {
        }

        public MeningoPatientSendingController(IApplicationDbContext applicationDbContext, MeningoPatientController patientController,
            MeningoSendingController sendingController) : base(applicationDbContext, patientController, sendingController)
        {
        }


        [Authorize(Roles = DefaultRoles.User)]
        public ActionResult IrisExport(FromToQuery query)
        {
            if (query.From == DateTime.MinValue)
            {
                var lastYear = DateTime.Now.Year - 1;
                var exportQuery = new FromToQuery
                {
                    From = new DateTime(lastYear, 1, 1),
                    To = new DateTime(lastYear, 12, 31)
                };
                return View(exportQuery);
            }

            var sendings = SendingsMatchingExportQuery(query, ExportType.Iris).ToList();
            return ExportToExcel(query, sendings, new MeningoSendingIrisExport(), "Iris");
        }


        protected override IDbSet<MeningoSending> SendingDbSet()
        {
            return db.MeningoSendings;
        }

        protected override IDbSet<MeningoPatient> PatientDbSet()
        {
            return db.MeningoPatients;
        }

        protected override void CreateAndEditPreparationsExtensions(PatientSendingViewModel<MeningoPatient, MeningoSending> patientSending)
        {
            ValidateModel(patientSending.Sending, new MeningoSendingValidator());
            ValidateModel(patientSending.Patient, new MeningoPatientValidator());
            ValidatePatientBirthdateGreaterOrEqualReceivingDate(patientSending);
        }

        private void ValidatePatientBirthdateGreaterOrEqualReceivingDate(PatientSendingViewModel<MeningoPatient, MeningoSending> patientSending)
        {
            var samplingDateBeforeBirthDate = (patientSending.Sending.SamplingDate ?? DateTime.MaxValue).CompareTo(patientSending.Patient.BirthDate ?? DateTime.MinValue) < 0;
            if (samplingDateBeforeBirthDate)
            {
                ModelState.AddModelError("Sending.SamplingDate", "Das Entnahmedatum muss nach dem Geburtsdatum des Patienten liegen");
            }
        }

        protected override string IsolateControllerName => "MeningoIsolate";

        protected override IEnumerable<MeningoSending> SendingsMatchingExportQuery(FromToQuery query, ExportType exportType)
        {
            if (exportType == ExportType.Rki)
            {
                throw new NotImplementedException("RKI export is not finished for Meningococci");
            }
            var queryResult = NotDeletedSendings()
                .Include(s => s.Patient)
                .Include(s => s.Isolate)
                .Include(s => s.Isolate.NeisseriaPubMlstIsolate)
                .Where
                (s => (s.SamplingDate == null && s.ReceivingDate >= query.From && s.ReceivingDate <= query.To)
                      || (s.SamplingDate >= query.From && s.SamplingDate <= query.To)
                ).OrderBy(s => s.Isolate.StemNumber).ToList();

            if (exportType == ExportType.Iris)
            {
                queryResult = queryResult.Where(s => s.Invasive == YesNo.Yes).ToList();
            }

            return queryResult;
        }

        protected override ExportDefinition<MeningoSending> CreateRkiExportDefinition()
        {
            var export = new ExportDefinition<MeningoSending>();
            return export;
        }

        protected override ExportDefinition<MeningoSending> CreateLaboratoryExportDefinition()
        {
            return new MeningoSendingLaboratoryExport();
        }

        protected override List<QueryRecord> QueryRecords()
        {
            var query = NotDeletedSendings().Select(x => new
            {
                x.MeningoSendingId,
                x.Isolate.MeningoIsolateId,
                x.Isolate.ReportStatus,
                x.Isolate.ReportDate,
                x.Patient.Initials,
                x.Patient.BirthDate,
                x.Isolate.StemNumber,
                x.ReceivingDate,
                x.SamplingLocation,
                x.OtherInvasiveSamplingLocation,
                x.OtherNonInvasiveSamplingLocation,
                x.Isolate.YearlySequentialIsolateNumber,
                x.Isolate.Year,
                x.SenderLaboratoryNumber,
                PatientPostalCode = x.Patient.PostalCode,
                SenderPostalCode = db.Senders.FirstOrDefault(s => s.SenderId == x.SenderId).PostalCode,
            });
            var queryRecords = query.ToList().Select(x =>
            {
                var invasive = MeningoSending.IsInvasive(x.SamplingLocation) ? YesNo.Yes : YesNo.No;
                var samplingLocation = x.SamplingLocation == MeningoSamplingLocation.OtherInvasive
                    ? Server.HtmlEncode(x.OtherInvasiveSamplingLocation)
                    : x.SamplingLocation == MeningoSamplingLocation.OtherNonInvasive
                    ? Server.HtmlEncode(x.OtherNonInvasiveSamplingLocation)
                    : EnumEditor.GetEnumDescription(x.SamplingLocation);
                var laboratoryNumber = ReportFormatter.ToLaboratoryNumber(x.YearlySequentialIsolateNumber, x.Year, DatabaseType.Meningococci);
                return new QueryRecord
                {
                    SendingId = x.MeningoSendingId,
                    IsolateId = x.MeningoIsolateId,
                    Initials = x.Initials,
                    BirthDate = x.BirthDate,
                    StemNumber = x.StemNumber.ToStemNumberWithPrefix(DatabaseType.Meningococci),
                    ReceivingDate = x.ReceivingDate,
                    SamplingLocation = samplingLocation,
                    Invasive = EnumEditor.GetEnumDescription(invasive),
                    LaboratoryNumber = x.Year * 10000 + x.YearlySequentialIsolateNumber,
                    LaboratoryNumberString = laboratoryNumber,
                    ReportStatus = x.ReportStatus,
                    PatientPostalCode = x.PatientPostalCode,
                    SenderPostalCode = x.SenderPostalCode,
                    SenderLaboratoryNumber = x.SenderLaboratoryNumber,
                    FullTextSearch = string.Join(" ",
                            x.Initials, x.BirthDate.ToReportFormat(),
                            x.StemNumber.ToStemNumberWithPrefix(DatabaseType.Meningococci),
                            x.ReceivingDate.ToReportFormat(),
                            invasive, samplingLocation, laboratoryNumber,
                            x.PatientPostalCode, x.SenderPostalCode, x.SenderLaboratoryNumber)
                        .ToLower()
                };
            }).ToList();
            return queryRecords;
        }
    }
}