using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using HaemophilusWeb.Domain;
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


        protected override IDbSet<MeningoSending> SendingDbSet()
        {
            return db.MeningoSendings;
        }

        protected override IDbSet<MeningoPatient> PatientDbSet()
        {
            return db.MeningoPatients;
        }

        protected override void CreateAndEditPreparations(PatientSendingViewModel<MeningoPatient, MeningoSending> patientSending)

        {
            AssignClinicalInformationFromCheckboxValues(patientSending);

            // TODO Validation
            //ValidateModel(patientSending.Sending, new SendingValidator());
            //ValidateModel(patientSending.Patient, new PatientValidator());
        }

        protected override string IsolateControllerName => "MeningoIsolate";

        private void AssignClinicalInformationFromCheckboxValues(PatientSendingViewModel<MeningoPatient, MeningoSending> patientSending)
        {
            patientSending.Patient.ClinicalInformation =
                EnumUtils.ParseCommaSeperatedListOfNamesAsFlagsEnum<MeningoClinicalInformation>(
                    Request.Form["ClinicalInformation"]);
        }

        protected override IEnumerable<MeningoSending> SendingsMatchingExportQuery(ExportQuery query, List<SamplingLocation> samplingLocations)
        {
            return NotDeletedSendings()
                .Include(s => s.Patient)
                //TODO fix sampling location
                //.Where
                //(s => samplingLocations.Contains(s.SamplingLocation)
                //      && ((s.SamplingDate == null && s.ReceivingDate >= query.From && s.ReceivingDate <= query.To)
                //          || (s.SamplingDate >= query.From && s.SamplingDate <= query.To))
                //)
                //.OrderBy(s => s.Isolate.StemNumber)
                .ToList();
        }

        protected override ExportDefinition<MeningoSending> CreateRkiExportDefinition()
        {
            var export = new ExportDefinition<MeningoSending>();
            var counties = db.Counties.OrderBy(c => c.ValidSince).ToList();

            var emptyCounty = new County { CountyNumber = "" };
            Func<MeningoSending, County> findCounty =
                s => counties.FirstOrDefault(c => c.IsEqualTo(s.Patient.County)) ?? emptyCounty;

            //export.AddField(s => s.Isolate.StemNumber, "klhi_nr");
            export.AddField(s => s.ReceivingDate.ToReportFormat(), "eing");
            export.AddField(s => s.SamplingDate.ToReportFormat(), "ent");
            //export.AddField(s => ExportSamplingLocation(s.SamplingLocation, s), "mat");
            export.AddField(s => s.Patient.BirthDate.HasValue ? s.Patient.BirthDate.Value.Month : 0, "geb_monat");
            export.AddField(s => s.Patient.BirthDate.HasValue ? s.Patient.BirthDate.Value.Year : 0, "geb_jahr");
            export.AddField(s => ExportGender(s.Patient.Gender), "geschlecht");
            //export.AddField(s => ExportToString(s.Patient.HibVaccination), "hib_impf");
            //export.AddField(s => ExportToString(s.Isolate.Evaluation), "styp");
            //export.AddField(s => ExportToString(s.Isolate.BetaLactamase), "b_lac");
            export.AddField(s => findCounty(s).CountyNumber, "kreis_nr");
            export.AddField(s => new string(findCounty(s).CountyNumber.Take(2).ToArray()), "bundesland");
            export.AddField(s => s.SenderId, "einsender");
            export.AddField(s => ExportToString(s.Patient.County), "landkreis");
            export.AddField(s => ExportToString(s.Patient.State), "bundeslandName");
            //AddEpsilometerTestFields(export, Antibiotic.Ampicillin, "ampicillinMHK", "ampicillinBewertung");
            //AddEpsilometerTestFields(export, Antibiotic.AmoxicillinClavulanate, "amoxicillinClavulansaeureMHK", "bewertungAmoxicillinClavulansaeure");

            return export;
        }

        protected override ExportDefinition<MeningoSending> CreateLaboratoryExportDefinition()
        {
            var export = new ExportDefinition<MeningoSending>();

            //export.AddField(s => s.Isolate.LaboratoryNumber);
            //export.AddField(s => s.Isolate.StemNumber);
            export.AddField(s => s.SenderId);
            export.AddField(s => s.ReceivingDate.ToReportFormat());
            export.AddField(s => s.SamplingDate.ToReportFormat());
            export.AddField(s => s.SenderLaboratoryNumber);
            //export.AddField(s => ExportSamplingLocation(s.SamplingLocation, s));
            export.AddField(s => ExportToString(s.Material));
            export.AddField(s => ExportToString(s.Invasive));
            export.AddField(s => s.SerogroupSender);

            export.AddField(s => s.Patient.Initials);
            export.AddField(s => s.Patient.BirthDate.ToReportFormat());
            //export.AddField(s => s.Isolate.PatientAge(), "Patientenalter bei Entnahme");
            export.AddField(s => ExportToString(s.Patient.Gender));
            export.AddField(s => s.Patient.PostalCode);
            export.AddField(s => s.Patient.City);
            export.AddField(s => ExportToString(s.Patient.County));
            export.AddField(s => ExportToString(s.Patient.State));
            //export.AddField(s => ExportClinicalInformation(s.Patient.ClinicalInformation, s));
            //export.AddField(s => ExportToString(s.Patient.HibVaccination));
            //export.AddField(s => s.Patient.HibVaccinationDate.ToReportFormat());
            //export.AddField(s => s.Remark, "Bemerkung (Einsendung)");

            //export.AddField(s => ExportToString(s.Isolate.Growth));
            //export.AddField(s => ExportToString(s.Isolate.TypeOfGrowth));
            //export.AddField(s => ExportToString(s.Isolate.Oxidase));
            //export.AddField(s => ExportToString(s.Isolate.BetaLactamase));
            //export.AddField(s => ExportToString(s.Isolate.Agglutination));
            //export.AddField(s => ExportToString(s.Isolate.FactorTest));
            //AddEpsilometerTestFields(export, Antibiotic.Ampicillin);
            //AddEpsilometerTestFields(export, Antibiotic.AmoxicillinClavulanate);
            //AddEpsilometerTestFields(export, Antibiotic.Meropenem);
            //AddEpsilometerTestFields(export, Antibiotic.Cefotaxime);
            //AddEpsilometerTestFields(export, Antibiotic.Imipenem);
            //AddEpsilometerTestFields(export, Antibiotic.Ciprofloxacin);
            //export.AddField(s => ExportToString(s.Isolate.OuterMembraneProteinP2));
            //export.AddField(s => ExportToString(s.Isolate.BexA));
            //export.AddField(s => ExportToString(s.Isolate.SerogroupPcr));
            //export.AddField(s => ExportToString(s.Isolate.FuculoKinase));
            //export.AddField(s => ExportToString(s.Isolate.OuterMembraneProteinP6));
            //export.AddField(s => ExportToString(s.Isolate.RibosomalRna16S));
            //export.AddField(s => ExportToString(s.Isolate.RibosomalRna16SBestMatch));
            //export.AddField(s => ExportToString(s.Isolate.RibosomalRna16SMatchInPercent));
            //export.AddField(s => ExportToString(s.Isolate.ApiNh));
            //export.AddField(s => s.Isolate.ApiNhBestMatch);
            //export.AddField(s => s.Isolate.ApiNhMatchInPercent);
            //export.AddField(s => ExportToString(s.Isolate.MaldiTof));
            //export.AddField(s => s.Isolate.MaldiTofBestMatch);
            //export.AddField(s => s.Isolate.MaldiTofMatchConfidence);
            //export.AddField(s => ExportToString(s.Isolate.Ftsi));
            //export.AddField(s => s.Isolate.FtsiEvaluation1);
            //export.AddField(s => s.Isolate.FtsiEvaluation2);
            //export.AddField(s => s.Isolate.FtsiEvaluation3);
            //export.AddField(s => ExportToString(s.Isolate.Mlst));
            //export.AddField(s => s.Isolate.MlstSequenceType);
            //export.AddField(s => ExportToString(s.Isolate.Evaluation));
            //export.AddField(s => s.Isolate.ReportDate);
            //export.AddField(s => s.Isolate.Remark, "Bemerkung (Isolat)");
            //export.AddField(s => ExportRkiMatchRecord(s.RkiMatchRecord, rkiMatchRecord => rkiMatchRecord.RkiReferenceId.ToString()), "RKI InterneRef");
            //export.AddField(s => ExportRkiMatchRecord(s.RkiMatchRecord, rkiMatchRecord => rkiMatchRecord.RkiReferenceNumber), "RKI Aktenzeichen");
            //export.AddField(s => ExportRkiMatchRecord(s.RkiMatchRecord, rkiMatchRecord => rkiMatchRecord.RkiStatus, ExportToString(RkiStatus.None)), "RKI Status");

            return export;
        }

        protected override List<QueryRecord> QueryRecords()
        {
            var query = NotDeletedSendings().Select(x => new
            {
                x.MeningoSendingId,
                x.Isolate.MeningoIsolateId,
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
                var laboratoryNumber = ReportFormatter.ToLaboratoryNumber(x.YearlySequentialIsolateNumber, x.Year);
                return new QueryRecord
                {
                    SendingId = x.MeningoSendingId,
                    IsolateId = x.MeningoIsolateId,
                    Initials = x.Initials,
                    BirthDate = x.BirthDate,
                    StemNumber = x.StemNumber,
                    ReceivingDate = x.ReceivingDate,
                    SamplingLocation = samplingLocation,
                    Invasive = EnumEditor.GetEnumDescription(invasive),
                    LaboratoryNumber = x.Year * 10000 + x.YearlySequentialIsolateNumber,
                    LaboratoryNumberString = laboratoryNumber,
                    ReportGenerated = x.ReportDate.HasValue,
                    PatientPostalCode = x.PatientPostalCode,
                    SenderPostalCode = x.SenderPostalCode,
                    SenderLaboratoryNumber = x.SenderLaboratoryNumber,
                    FullTextSearch = string.Join(" ",
                            x.Initials, x.BirthDate.ToReportFormat(),
                            x.StemNumber,
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