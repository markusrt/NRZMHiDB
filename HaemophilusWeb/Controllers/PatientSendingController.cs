using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using DataTables.Mvc;
using ExportToExcel;
using FluentValidation;
using HaemophilusWeb.Domain;
using HaemophilusWeb.Models;
using HaemophilusWeb.Utils;
using HaemophilusWeb.Validators;
using HaemophilusWeb.ViewModels;
using HaemophilusWeb.Views.Utils;

namespace HaemophilusWeb.Controllers
{
    public class PatientSendingController : Controller
    {
        private readonly IApplicationDbContext db;
        private readonly PatientController patientController;
        private readonly SendingController sendingController;

        public PatientSendingController()
            : this(
                new ApplicationDbContextWrapper(new ApplicationDbContext()), new PatientController(),
                new SendingController())
        {
        }

        public PatientSendingController(IApplicationDbContext applicationDbContext, PatientController patientController,
            SendingController sendingController)
        {
            db = applicationDbContext;
            this.patientController = patientController;
            this.sendingController = sendingController;
        }

        public ActionResult Create()
        {
            var patientResult = patientController.Create() as ViewResult;
            var sendingResult = sendingController.Create() as ViewResult;


            return CreateEditView(new PatientSendingViewModel
            {
                Patient = (Patient) patientResult.Model,
                Sending = (Sending) sendingResult.Model
            });
        }

        private ViewResult CreateEditView(PatientSendingViewModel patientSending)
        {
            sendingController.AddReferenceDataToViewBag(ViewBag);
            patientController.AddReferenceDataToViewBag(ViewBag);
            return View(patientSending);
        }

        public ActionResult Edit(int? id)
        {
            var sendingResult = sendingController.Edit(id) as ViewResult;

            return CreateEditView(CreatePatientSending((Sending) sendingResult.Model));
        }

        [HttpPost]
        public ActionResult Edit(PatientSendingViewModel patientSending)
        {
            AssignClinicalInformationFromCheckboxValues(patientSending);

            PerformValidations(patientSending);

            if (ModelState.IsValid)
            {
                db.MarkAsModified(patientSending.Patient);
                db.MarkAsModified(patientSending.Sending);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return CreateEditView(patientSending);
        }

        private void AssignClinicalInformationFromCheckboxValues(PatientSendingViewModel patientSending)
        {
            patientSending.Patient.ClinicalInformation =
                EnumUtils.ParseCommaSeperatedListOfNamesAsFlagsEnum<ClinicalInformation>(
                    Request.Form["ClinicalInformation"]);
        }

        [HttpPost]
        public ActionResult Create(PatientSendingViewModel patientSending)
        {
            PerformValidations(patientSending);
            ValidatePatientDoesNotAlreadyExist(patientSending.Patient);

            if (ModelState.IsValid)
            {
                patientController.CreatePatient(patientSending.Patient);
                var sending = patientSending.Sending;
                sending.PatientId = patientSending.Patient.PatientId;
                sendingController.CreateSendingAndAssignStemnumber(sending);
                if (sending.LaboratoryNumber != sending.Isolate.LaboratoryNumber)
                {
                    TempData["WarningMessage"] =
                        "Beim Speichern der letzten Einsendung hat sich die Labornummer geändert. Neue Labornummer: " +
                        sending.Isolate.LaboratoryNumber;
                }
                return RedirectToAction("Index");
            }
            return CreateEditView(patientSending);
        }

        private void PerformValidations(PatientSendingViewModel patientSending)
        {
            ValidateModel(patientSending.Sending, new SendingValidator());
            ValidateModel(patientSending.Patient, new PatientValidator());
        }

        private void ValidatePatientDoesNotAlreadyExist(Patient patient)
        {
            var existingPatient = db.Patients.SingleOrDefault(
                p => p.Initials == patient.Initials &&
                     ((p.BirthDate.HasValue && patient.BirthDate.HasValue &&
                       p.BirthDate.Value == patient.BirthDate.Value)
                      || (!p.BirthDate.HasValue && !patient.BirthDate.HasValue))
                     && p.PostalCode == patient.PostalCode);
            if (existingPatient != null)
            {
                ModelState.AddModelError("",
                    "Ein Patient mit den selben Initialen, Geburtsdatum und Postleitzahl existiert bereits");
            }
        }

        private void ValidateModel<T>(T objectToValidate, IValidator<T> validator)
        {
            var result = validator.Validate(objectToValidate);
            if (result.IsValid)
            {
                return;
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult Index()
        {
            return View(db.Sendings.Include(s => s.Patient).OrderBy(s => s.SendingId)
                .Take(0).Select(CreatePatientSending).ToList());
        }

        private PatientSendingViewModel CreatePatientSending(Sending sending)
        {
            return new PatientSendingViewModel
            {
                Patient = sending.Patient,
                Sending = sending
            };
        }

        public ActionResult RkiExport(ExportQuery query)
        {
            if (query.From == DateTime.MinValue)
            {
                var lastYear = DateTime.Now.Year - 1;
                var exportQuery = new ExportQuery
                {
                    From = new DateTime(lastYear, 1, 1),
                    To = new DateTime(lastYear, 12, 31)
                };
                return View(exportQuery);
            }

            var list = new List<RkiExportRecord>();
            foreach (
                var sending in
                    SendingsMatchingExportQuery(query,
                        new List<SamplingLocation> {SamplingLocation.Blood, SamplingLocation.Liquor}))
            {
                var rkiExportRecord = new RkiExportRecord();
                PopulateExportRecord(sending, rkiExportRecord);
                list.Add(rkiExportRecord);
            }
            return ExportToExcel(query, list, "RKI");
        }

        public ActionResult LaboratoryExport(ExportQuery query)
        {
            if (query.From == DateTime.MinValue)
            {
                var lastYear = DateTime.Now.Year - 1;
                var exportQuery = new ExportQuery
                {
                    From = new DateTime(lastYear, 1, 1),
                    To = new DateTime(lastYear, 12, 31)
                };
                return View(exportQuery);
            }

            var list = new List<LaboratoryExportRecord>();
            foreach (
                var sending in
                    SendingsMatchingExportQuery(query,
                        new List<SamplingLocation>
                        {
                            SamplingLocation.Blood,
                            SamplingLocation.Liquor,
                            SamplingLocation.Other
                        }))
            {
                var laboratoryExportRecord = new LaboratoryExportRecord();
                PopulateExportRecord(sending, laboratoryExportRecord);
                laboratoryExportRecord.PatientAgeAtSampling = sending.Isolate.PatientAge();
                list.Add(laboratoryExportRecord);
            }
            return ExportToExcel(query, list, "Labor");
        }

        private ActionResult ExportToExcel<T>(ExportQuery query, List<T> list, string prefix) where T : ExportRecord
        {
            var tempFile = System.IO.Path.GetTempFileName();
            CreateExcelFile.CreateExcelDocument(list, tempFile);
            return File(System.IO.File.ReadAllBytes(tempFile),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                String.Format("{0}-Export_{1:yyyyMMdd}-{2:yyyyMMdd}.xlsx", prefix, query.From, query.To));
        }

        private List<Sending> SendingsMatchingExportQuery(ExportQuery query, List<SamplingLocation> samplingLocations)
        {
            return db.Sendings.Include(s => s.Patient)
                .Include(s => s.Patient)
                .Where
                (s => samplingLocations.Contains(s.SamplingLocation)
                      && ((s.SamplingDate == null && s.ReceivingDate >= query.From && s.ReceivingDate <= query.To)
                          || (s.SamplingDate >= query.From && s.SamplingDate <= query.To))
                )
                .OrderBy(s => s.Isolate.StemNumber)
                .ToList();
        }

        private void PopulateExportRecord(Sending sending, ExportRecord exportRecord)
        {
            var patientCounty = sending.Patient.County;
            exportRecord.BetaLactamase = EnumEditor.GetEnumDescription(sending.Isolate.BetaLactamase);
            exportRecord.Evaluation = EnumEditor.GetEnumDescription(sending.Isolate.Evaluation);
            exportRecord.HibVaccination = EnumEditor.GetEnumDescription(sending.Patient.HibVaccination);
            exportRecord.SenderId = sending.SenderId;
            exportRecord.SamplingLocation = EnumEditor.GetEnumDescription(sending.SamplingLocation);
            if (sending.SamplingLocation == SamplingLocation.Other)
            {
                exportRecord.SamplingLocation = sending.OtherSamplingLocation;
            }
            exportRecord.State = EnumEditor.GetEnumDescription(sending.Patient.State);
            exportRecord.County = patientCounty;
            exportRecord.Gender = sending.Patient.Gender == null
                ? "?"
                : EnumEditor.GetEnumDescription(sending.Patient.Gender).Substring(0, 1);
            exportRecord.Birthyear = sending.Patient.BirthDate.HasValue ? sending.Patient.BirthDate.Value.Year : 0;
            exportRecord.Birthmonth = sending.Patient.BirthDate.HasValue ? sending.Patient.BirthDate.Value.Month : 0;
            exportRecord.ReceivingDate = sending.ReceivingDate.ToReportFormat();
            exportRecord.SamplingDate = sending.SamplingDate.ToReportFormat();
            exportRecord.StemNumber = sending.Isolate.StemNumber;

            var county =
                db.Counties.OrderBy(c => c.ValidSince).ToList().FirstOrDefault(c => c.IsEqualTo(patientCounty));
            if (county != null)
            {
                exportRecord.CountyNumber = county.CountyNumber;
                exportRecord.StateNumber = county.CountyNumber.Substring(0, 2);
            }

            PopulateEpsilometerTestResult(sending, Antibiotic.Ampicillin,
                result => exportRecord.AmpicillinEpsilometerTestResult = EnumEditor.GetEnumDescription(result),
                measurement => exportRecord.AmpicillinMeasurement = measurement);

            PopulateEpsilometerTestResult(sending, Antibiotic.AmoxicillinClavulanate,
                result =>
                    exportRecord.AmoxicillinClavulanateEpsilometerTestResult = EnumEditor.GetEnumDescription(result),
                measurement => exportRecord.AmoxicillinClavulanateMeasurement = measurement);
        }

        private static void PopulateEpsilometerTestResult(Sending sending, Antibiotic antibiotic,
            Action<EpsilometerTestResult> populateTestResult, Action<double> populateMeasurement)
        {
            var eTestResult = sending.Isolate.EpsilometerTests.SingleOrDefault(
                e => e.EucastClinicalBreakpoint.Antibiotic == antibiotic);
            if (eTestResult == null)
            {
                return;
            }
            populateTestResult(eTestResult.Result);
            populateMeasurement(Math.Round(eTestResult.Measurement, 3));
        }

        [HttpPost]
        public JsonResult DataTableAjax([ModelBinder(typeof (DataTablesBinder))] IDataTablesRequest requestParameters)
        {
            var query = db.Sendings.Select(x => new
            {
                x.SendingId,
                x.Isolate.IsolateId,
                x.Isolate.ReportDate,
                x.Patient.Initials,
                x.Patient.BirthDate,
                x.Isolate.StemNumber,
                x.ReceivingDate,
                x.SamplingLocation,
                x.OtherSamplingLocation,
                x.Invasive,
                x.Isolate.YearlySequentialIsolateNumber,
                x.Isolate.Year,
            });
            var queryable = query.ToList().Select(x =>
            {
                var invasive = EnumEditor.GetEnumDescription(x.Invasive);
                var samplingLocation = x.SamplingLocation == SamplingLocation.Other
                    ? Server.HtmlEncode(x.OtherSamplingLocation)
                    : EnumEditor.GetEnumDescription(x.SamplingLocation);
                var laboratoryNumber = ReportFormatter.ToLaboratoryNumber(x.YearlySequentialIsolateNumber, x.Year);
                return new QueryRecord
                {
                    SendingId = x.SendingId,
                    IsolateId = x.IsolateId,
                    Initials = x.Initials,
                    BirthDate = x.BirthDate,
                    StemNumber = x.StemNumber,
                    ReceivingDate = x.ReceivingDate,
                    SamplingLocation = samplingLocation,
                    Invasive = invasive,
                    LaboratoryNumber = x.Year*10000 + x.YearlySequentialIsolateNumber,
                    LaboratoryNumberString = laboratoryNumber,
                    ReportGenerated = x.ReportDate.HasValue,
                    FullTextSearch = string.Join(" ",
                        x.Initials, x.BirthDate.ToReportFormat(),
                        x.StemNumber, x.ReceivingDate.ToReportFormat(),
                        invasive, samplingLocation, laboratoryNumber).ToLower()
                };
            }).ToList();

            var totalCount = queryable.Count();

            var searchValue = requestParameters.Search.Value.ToLower();
            if (!string.IsNullOrEmpty(searchValue))
            {
                queryable = queryable.Where(x => x.FullTextSearch.Contains(searchValue)).ToList();
            }

            foreach (var column in requestParameters.Columns.GetSortedColumns())
            {
                Func<QueryRecord, object> keySelector = x => x.GetType().GetProperty(column.Data).GetValue(x);
                queryable = column.SortDirection == Column.OrderDirection.Descendant
                    ? queryable.OrderByDescending(keySelector).ToList()
                    : queryable.OrderBy(keySelector).ToList();
            }

            var pagedData = queryable.Skip(requestParameters.Start)
                .Take(requestParameters.Length)
                .Select(x => new
                {
                    x.Initials,
                    BirthDate = x.BirthDate.ToReportFormat(),
                    x.StemNumber,
                    ReceivingDate = x.ReceivingDate.ToReportFormat(),
                    x.SamplingLocation,
                    x.Invasive,
                    ReportGenerated = CreateReportGeneratedIcon(x.ReportGenerated),
                    LaboratoryNumber = CreateIsolateLink(x.IsolateId, x.LaboratoryNumberString),
                    Link = CreateEditControls(x.SendingId, x.IsolateId)
                });


            var dataTablesResult = new DataTablesResponse(
                requestParameters.Draw,
                pagedData,
                queryable.Count(),
                totalCount
                );

            return Json(dataTablesResult);
        }

        private static string CreateReportGeneratedIcon(bool reportGenerated)
        {
            return string.Format(
                "<span style=\"color:{0}\" class=\"glyphicon {1}\" aria-hidden=\"true\"></span>",
                reportGenerated ? "#00CC00" : "#CC0000",
                reportGenerated ? "glyphicon-ok-sign" : "glyphicon-remove-sign");
        }

        public class QueryRecord
        {
            public string Initials { get; set; }
            public DateTime? BirthDate { get; set; }
            public int? StemNumber { get; set; }
            public DateTime ReceivingDate { get; set; }
            public string SamplingLocation { get; set; }
            public string Invasive { get; set; }
            public int LaboratoryNumber { get; set; }
            public string LaboratoryNumberString { get; set; }
            public string FullTextSearch { get; set; }
            public int IsolateId { get; set; }
            public int SendingId { get; set; }
            public bool ReportGenerated { get; set; }
        }

        private string CreateIsolateLink(int isolateId, string laboratoryNumber)
        {
            return string.Format("<a class=\"btn-sm btn btn-default\" href=\"{0}\" role=\"button\">{1}</a>",
                Url.Action("Edit", "Isolate", new {id = isolateId}), laboratoryNumber);
        }

        private string CreateEditControls(int sendingId, int isolateId)
        {
            var builder = new StringBuilder();
            builder.Append("<div class=\"btn-group btn-group-sm\">");

            builder.AppendFormat("<a class=\"btn btn-default\" href=\"{0}\" role=\"button\">Bearbeiten</a>",
                Url.Action("Edit", new {id = sendingId}));
            builder.AppendFormat("<a class=\"btn btn-default\" href=\"{0}\" role=\"button\">Befund erstellen</a>",
                Url.Action("Report", "Isolate", new {id = isolateId}));
            builder.Append("</div>");
            return builder.ToString();
        }
    }
}