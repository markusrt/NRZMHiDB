using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using HaemophilusWeb.Models;
using HaemophilusWeb.Tools;
using HaemophilusWeb.Utils;
using HaemophilusWeb.Validators;
using HaemophilusWeb.ViewModels;
using HaemophilusWeb.Views.Utils;

namespace HaemophilusWeb.Controllers
{
    public enum ExportType
    {
        Laboratory,
        Rki,
        Iris
    }

    public class PatientSendingController : PatientSendingControllerBase<PatientSendingViewModel<Patient, Sending>, Patient, Sending>
    {


        public PatientSendingController()
            : this(
                new ApplicationDbContextWrapper(new ApplicationDbContext()), new PatientController(),
                new SendingController())
        {
        }

        public PatientSendingController(IApplicationDbContext applicationDbContext, PatientController patientController,
            SendingController sendingController) : base(applicationDbContext, patientController, sendingController)
        {
        }

        [Authorize(Roles = DefaultRoles.User)]
        public ActionResult PubMlstExport(FromToQueryWithAdjustment queryWithAdjustment)
        {
            if (queryWithAdjustment.From == DateTime.MinValue)
            {
                var lastYear = DateTime.Now.Year - 1;
                var exportQuery = new FromToQueryWithAdjustment
                {
                    From = new DateTime(lastYear, 1, 1),
                    To = new DateTime(lastYear, 12, 31)
                };
                return View(exportQuery);
            }

            //TODO clarify and merge method with Iris-Export should actually be the same
            var sendings = SendingsMatchingExportQuery(queryWithAdjustment, ExportType.Iris).ToList();
            var duplicateResolver = new DuplicatePatientResolver(new PubMlstColumns());
            Action<DataTable> cleanDuplicates = duplicateResolver.RemovePatientData;

            if (queryWithAdjustment.Unadjusted == YesNo.No)
            {
                sendings.RemoveAll(s => s.SamplingLocation == SamplingLocation.OtherNonInvasive);
                cleanDuplicates = duplicateResolver.CleanOrMarkDuplicates;
            }

            return ExportToExcel(queryWithAdjustment, sendings, new HaemophilusPubMlstExport(), "PubMLST", cleanDuplicates);
        }

        protected override IDbSet<Sending> SendingDbSet()
        {
            return db.Sendings;
        }

        protected override IDbSet<Patient> PatientDbSet()
        {
            return db.Patients;
        }

        protected override void CreateAndEditPreparationsExtensions(PatientSendingViewModel<Patient, Sending> patientSending)

        {
            ValidateModel(patientSending.Sending, new SendingValidator());
            ValidateModel(patientSending.Patient, new PatientValidator());
        }

        protected override string IsolateControllerName => "Isolate";

        protected override IEnumerable<Sending> SendingsByPatient(int patientId)
        {
            var queryResult = NotDeletedSendings()
                .Include(s => s.Patient)
                .Include(s => s.Isolate)
                .Where(s => s.PatientId == patientId);
            return queryResult;
        }

        protected override void MergePatients(int patientIdToKeep, int patientIdToDelete)
        {
            var sendingsToMigrate = SendingsByPatient(patientIdToDelete);
            foreach (var sending in sendingsToMigrate)
            {
                sending.PatientId = patientIdToKeep;
            }

            var patientToDelete = PatientDbSet().Find(patientIdToDelete);
            PatientDbSet().Remove(patientToDelete);
            db.SaveChanges();
        }

        protected override IEnumerable<Sending> SendingsMatchingExportQuery(FromToQuery query, ExportType exportType)
        {
            var samplingLocations = exportType == ExportType.Rki || exportType == ExportType.Iris
                ? new List<SamplingLocation> { SamplingLocation.Blood, SamplingLocation.Liquor, SamplingLocation.OtherInvasive }
                : EnumUtils.AllEnumValues<SamplingLocation>().ToList();

            var filteredSendings = NotDeletedSendings()
                .Include(s => s.Patient)
                .Where
                (s => samplingLocations.Contains(s.SamplingLocation)
                      && ((s.SamplingDate == null && s.ReceivingDate >= query.From && s.ReceivingDate <= query.To)
                          || (s.SamplingDate >= query.From && s.SamplingDate <= query.To))
                );
            if (exportType == ExportType.Iris)
            {
                var overseas = EnumEditor.GetEnumDescription(State.Overseas);
                filteredSendings = filteredSendings.Where(s => !s.Patient.County.Equals(overseas));

                var nonHaemophilus = new List<Evaluation>
                {
                    Evaluation.HaemophilusParainfluenzae,
                    Evaluation.NoGrowth,
                    Evaluation.NoHaemophilusSpecies,
                    Evaluation.HaemophilusSpeciesNoHaemophilusInfluenzae,
                    Evaluation.NoHaemophilusInfluenzae
                };
                filteredSendings = filteredSendings.Where(s => !nonHaemophilus.Contains(s.Isolate.Evaluation));
            }
            return filteredSendings.OrderBy(s => s.Isolate.StemNumber).ToList();
        }

        protected override ExportDefinition<Sending> CreateRkiExportDefinition()
        {
            var counties = db.Counties.OrderBy(c => c.ValidSince).ToList();
            return new HaemophilusSendingRkiExport(counties);
        }

        protected override ExportDefinition<Sending> CreateLaboratoryExportDefinition()
        {
            return new HaemophilusSendingLaboratoryExport();
        }

        protected override List<QueryRecord> QueryRecords()
        {
            var query = NotDeletedSendings().Select(x => new
            {
                x.SendingId,
                x.Isolate.IsolateId,
                x.Isolate.ReportStatus,
                x.Isolate.ReportDate,
                x.Patient.Initials,
                x.Patient.BirthDate,
                x.Isolate.StemNumber,
                x.ReceivingDate,
                x.SamplingLocation,
                x.OtherSamplingLocation,
                x.Isolate.YearlySequentialIsolateNumber,
                x.Isolate.Year,
                x.SenderLaboratoryNumber,
                x.PatientId,
                PatientPostalCode = x.Patient.PostalCode,
                SenderPostalCode = db.Senders.FirstOrDefault(s => s.SenderId == x.SenderId).PostalCode,
            });
            var list = query.ToList();
            var queryRecords = list.Select(x =>
            {
                var samplingLocation = x.SamplingLocation.IsOther()
                    ? Server.HtmlEncode(x.OtherSamplingLocation)
                    : EnumEditor.GetEnumDescription(x.SamplingLocation);
                var laboratoryNumber = ReportFormatter.ToLaboratoryNumber(x.YearlySequentialIsolateNumber, x.Year, DatabaseType.Haemophilus);
                var isInvasive = Sending.IsInvasive(x.SamplingLocation) ? YesNo.Yes : YesNo.No;
                var invasive = EnumEditor.GetEnumDescription(isInvasive);
                return new QueryRecord
                {
                    SendingId = x.SendingId,
                    IsolateId = x.IsolateId,
                    PatientId = x.PatientId,
                    Initials = x.Initials,
                    BirthDate = x.BirthDate,
                    StemNumber = x.StemNumber.ToStemNumberWithPrefix(DatabaseType.Haemophilus),
                    ReceivingDate = x.ReceivingDate,
                    SamplingLocation = samplingLocation,
                    Invasive = invasive,
                    LaboratoryNumber = x.Year * 10000 + x.YearlySequentialIsolateNumber,
                    LaboratoryNumberString = laboratoryNumber,
                    ReportStatus = x.ReportStatus,
                    PatientPostalCode = x.PatientPostalCode,
                    SenderPostalCode = x.SenderPostalCode,
                    SenderLaboratoryNumber = x.SenderLaboratoryNumber,
                    FullTextSearch = String.Join(" ",
                            x.Initials, x.BirthDate.ToReportFormat(),
                            x.StemNumber.ToStemNumberWithPrefix(DatabaseType.Haemophilus), x.ReceivingDate.ToReportFormat(),
                            invasive, samplingLocation, laboratoryNumber,
                            x.PatientPostalCode, x.SenderPostalCode, x.SenderLaboratoryNumber)
                        .ToLower()
                };
            }).ToList();
            return queryRecords;
        }

        protected override ActionResult RkiExportWithPotentialAdjustments(FromToQueryWithAdjustment query)
        {
            var duplicateResolver = new DuplicatePatientResolver(new RkiExportColumns());
            Action<DataTable> cleanDuplicates = duplicateResolver.RemovePatientData;

            if (query.Unadjusted == YesNo.No)
            {
                cleanDuplicates = duplicateResolver.CleanOrMarkDuplicates;
            }

            var sendings = SendingsMatchingExportQuery(query, ExportType.Rki).ToList();

            return ExportToExcel(query, sendings, CreateRkiExportDefinition(), "RKI", cleanDuplicates);
        }
    }
}