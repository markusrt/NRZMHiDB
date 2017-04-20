using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using DataTables.Mvc;
using FluentValidation;
using HaemophilusWeb.Domain;
using HaemophilusWeb.Models;
using HaemophilusWeb.Tools;
using HaemophilusWeb.Utils;
using HaemophilusWeb.Validators;
using HaemophilusWeb.ViewModels;
using HaemophilusWeb.Views.Utils;

namespace HaemophilusWeb.Controllers
{
    public class PatientSendingController : ControllerBase
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

        [Authorize(Roles = DefaultRoles.User)]
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
            sendingController.AddReferenceDataToViewBag(ViewBag, patientSending.Sending);
            patientController.AddReferenceDataToViewBag(ViewBag);
            return View(patientSending);
        }

        [Authorize(Roles = DefaultRoles.User)]
        public ActionResult Edit(int? id)
        {
            var sending = LoadSendingFromSendingController(id);
            return CreateEditView(CreatePatientSending(sending));
        }

        private Sending LoadSendingFromSendingController(int? id)
        {
            var sendingResult = sendingController.Edit(id) as ViewResult;
            var sending = (Sending) sendingResult.Model;
            return sending;
        }

        [HttpPost]
        [Authorize(Roles = DefaultRoles.User)]
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

        [Authorize(Roles = DefaultRoles.User)]
        public ActionResult Delete(int? id)
        {
            return View(LoadSendingFromSendingController(id));
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = DefaultRoles.User)]
        public ActionResult DeleteConfirmed(int id)
        {
            var sending = LoadSendingFromSendingController(id);
            sending.Deleted = true;
            return EditUnvalidated(sending);
        }

        [Authorize(Roles = DefaultRoles.User)]
        public ActionResult Deleted()
        {
            return View(db.Sendings.Include(s=>s.Patient).Where(s => s.Deleted).Select(CreatePatientSending).ToList());
        }

        [Authorize(Roles = DefaultRoles.User)]
        public ActionResult Undelete(int? id)
        {
            var sending = LoadSendingFromSendingController(id);
            sending.Deleted = false;
            return EditUnvalidated(sending);
        }


        private ActionResult EditUnvalidated(Sending sending)
        {
            ActionResult result = View();
            db.PerformWithoutSaveValidation(() => result = sendingController.Edit(sending));
            return result;
        }

        private void AssignClinicalInformationFromCheckboxValues(PatientSendingViewModel patientSending)
        {
            patientSending.Patient.ClinicalInformation =
                EnumUtils.ParseCommaSeperatedListOfNamesAsFlagsEnum<ClinicalInformation>(
                    Request.Form["ClinicalInformation"]);
        }

        [HttpPost]
        [Authorize(Roles = DefaultRoles.User)]
        public ActionResult Create(PatientSendingViewModel patientSending)
        {
            PerformValidations(patientSending);
            ValidatePatientDoesNotAlreadyExist(patientSending);

            if (ModelState.IsValid && !patientSending.DuplicatePatientDetected)
            {
                var patient = CreateOrUpdatePatient(patientSending);
                var sending = patientSending.Sending;
                sending.PatientId = patient.PatientId;
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

        private Patient CreateOrUpdatePatient(PatientSendingViewModel patientSending)
        {
            var patient = patientSending.Patient;
            if (patientSending.DuplicatePatientResolution == DuplicatePatientResolution.UseExistingPatient)
            {
                var duplicatePatients = FindDuplicatePatients(patient);
                var existingPatient = duplicatePatients.Single();
                patient.PatientId = existingPatient.PatientId;
                patientController.EditPatient(patient);
            }
            else
            {
                patientController.CreatePatient(patient);
            }
            return patient;
        }

        private void PerformValidations(PatientSendingViewModel patientSending)
        {
            ValidateModel(patientSending.Sending, new SendingValidator());
            ValidateModel(patientSending.Patient, new PatientValidator());
        }

        private void ValidatePatientDoesNotAlreadyExist(PatientSendingViewModel patientSending)
        {
            patientSending.DuplicatePatientDetected = false;
            if (patientSending.DuplicatePatientResolution != null)
            {
                return;
            }

            var duplicatePatients = FindDuplicatePatients(patientSending.Patient);
            if (duplicatePatients.Count() == 1)
            {
                patientSending.DuplicatePatientDetected = true;
            }
            else if (duplicatePatients.Count() > 1)
            {
                ModelState.AddModelError("",
                    "Es existieren bereits zwei Patienten mit den selben Initialen, Geburtsdatum und Postleitzahl. " +
                    "Ein dritter Patient mit den selben Eigenschaften kann leider nicht gespeichert werden.");
            }
        }

        private IQueryable<Patient> FindDuplicatePatients(Patient patient)
        {
            var existingPatients = db.Patients.Where(
                p => p.Initials == patient.Initials &&
                     ((p.BirthDate.HasValue && patient.BirthDate.HasValue &&
                       p.BirthDate.Value == patient.BirthDate.Value)
                      || (!p.BirthDate.HasValue && !patient.BirthDate.HasValue))
                     && p.PostalCode == patient.PostalCode);
            return existingPatients;
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
        [Authorize(Roles = DefaultRoles.User)]
        public ActionResult Index()
        {
            return View(NotDeletedSendings().Take(0).Select(CreatePatientSending).ToList());
        }

        private PatientSendingViewModel CreatePatientSending(Sending sending)
        {
            return new PatientSendingViewModel
            {
                Patient = sending.Patient,
                Sending = sending
            };
        }

        [Authorize(Roles = DefaultRoles.User + "," + DefaultRoles.PublicHealth)]
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

            var sendings = SendingsMatchingExportQuery(query,
                new List<SamplingLocation> {SamplingLocation.Blood, SamplingLocation.Liquor}).ToList();

            return ExportToExcel(query, sendings, CreateRkiExportDefinition(), "RKI");
        }

        [Authorize(Roles = DefaultRoles.User)]
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

            var sendings = SendingsMatchingExportQuery(query,
                new List<SamplingLocation>
                {
                    SamplingLocation.Blood,
                    SamplingLocation.Liquor,
                    SamplingLocation.Other
                }).ToList();
            return ExportToExcel(query, sendings, CreateLaboratoryExportDefinition(), "Labor");
        }

        private List<Sending> SendingsMatchingExportQuery(ExportQuery query, List<SamplingLocation> samplingLocations)
        {
            return NotDeletedSendings()
                .Include(s => s.Patient)
                .Where
                (s => samplingLocations.Contains(s.SamplingLocation)
                      && ((s.SamplingDate == null && s.ReceivingDate >= query.From && s.ReceivingDate <= query.To)
                          || (s.SamplingDate >= query.From && s.SamplingDate <= query.To))
                )
                .OrderBy(s => s.Isolate.StemNumber)
                .ToList();
        }

        private ExportDefinition<Sending> CreateLaboratoryExportDefinition()
        {
            var export = new ExportDefinition<Sending>();

            export.AddField(s => s.Isolate.LaboratoryNumber);
            export.AddField(s => s.Isolate.StemNumber);
            export.AddField(s => s.SenderId);
            export.AddField(s => s.ReceivingDate.ToReportFormat());
            export.AddField(s => s.SamplingDate.ToReportFormat());
            export.AddField(s => s.SenderLaboratoryNumber);
            export.AddField(s => ExportSamplingLocation(s.SamplingLocation, s));
            export.AddField(s => ExportToString(s.Material));
            export.AddField(s => ExportToString(s.Invasive));
            export.AddField(s => s.SenderConclusion);

            export.AddField(s => s.Patient.Initials);
            export.AddField(s => s.Patient.BirthDate.ToReportFormat());
            export.AddField(s => s.Isolate.PatientAge(), "Patientenalter bei Entnahme");
            export.AddField(s => ExportToString(s.Patient.Gender));
            export.AddField(s => s.Patient.PostalCode);
            export.AddField(s => s.Patient.City);
            export.AddField(s => ExportToString(s.Patient.County));
            export.AddField(s => ExportToString(s.Patient.State));
            export.AddField(s => ExportClinicalInformation(s.Patient.ClinicalInformation, s));
            export.AddField(s => ExportToString(s.Patient.HibVaccination));
            export.AddField(s => s.Patient.HibVaccinationDate.ToReportFormat());
            export.AddField(s => ExportToString(s.Patient.Therapy));
            export.AddField(s => s.Patient.TherapyDetails);
            export.AddField(s => s.Remark, "Bemerkung (Einsendung)");

            export.AddField(s => ExportToString(s.Isolate.Growth));
            export.AddField(s => ExportToString(s.Isolate.TypeOfGrowth));
            export.AddField(s => ExportToString(s.Isolate.Oxidase));
            export.AddField(s => ExportToString(s.Isolate.BetaLactamase));
            export.AddField(s => ExportToString(s.Isolate.Agglutination));
            export.AddField(s => ExportToString(s.Isolate.FactorTest));
            AddEpsilometerTestFields(export, Antibiotic.Ampicillin);
            AddEpsilometerTestFields(export, Antibiotic.AmoxicillinClavulanate);
            AddEpsilometerTestFields(export, Antibiotic.Meropenem);
            AddEpsilometerTestFields(export, Antibiotic.Cefotaxime);
            AddEpsilometerTestFields(export, Antibiotic.Imipenem);
            AddEpsilometerTestFields(export, Antibiotic.Ciprofloxacin);
            export.AddField(s => ExportToString(s.Isolate.OuterMembraneProteinP2));
            export.AddField(s => ExportToString(s.Isolate.BexA));
            export.AddField(s => ExportToString(s.Isolate.SerotypePcr));
            export.AddField(s => ExportToString(s.Isolate.FuculoKinase));
            export.AddField(s => ExportToString(s.Isolate.OuterMembraneProteinP6));
            export.AddField(s => ExportToString(s.Isolate.RibosomalRna16S));
            export.AddField(s => ExportToString(s.Isolate.RibosomalRna16SBestMatch));
            export.AddField(s => ExportToString(s.Isolate.RibosomalRna16SMatchInPercent));
            export.AddField(s => ExportToString(s.Isolate.ApiNh));
            export.AddField(s => s.Isolate.ApiNhBestMatch);
            export.AddField(s => s.Isolate.ApiNhMatchInPercent);
            export.AddField(s => ExportToString(s.Isolate.MaldiTof));
            export.AddField(s => s.Isolate.MaldiTofBestMatch);
            export.AddField(s => s.Isolate.MaldiTofMatchConfidence);
            export.AddField(s => ExportToString(s.Isolate.Ftsi));
            export.AddField(s => s.Isolate.FtsiEvaluation1);
            export.AddField(s => s.Isolate.FtsiEvaluation2);
            export.AddField(s => s.Isolate.FtsiEvaluation3);
            export.AddField(s => ExportToString(s.Isolate.Mlst));
            export.AddField(s => s.Isolate.MlstSequenceType);
            export.AddField(s => ExportToString(s.Isolate.Evaluation));
            export.AddField(s => s.Isolate.ReportDate);
            export.AddField(s => s.Isolate.Remark, "Bemerkung (Isolat)");

            return export;
        }

        private string ExportClinicalInformation(ClinicalInformation clinicalInformation, Sending sending)
        {
            var clinicalInformationAsString = EnumEditor.GetEnumDescription(clinicalInformation);
            if (sending.Patient.ClinicalInformation.HasFlag(ClinicalInformation.Other))
            {
                clinicalInformationAsString =
                    clinicalInformationAsString.Replace(EnumEditor.GetEnumDescription(ClinicalInformation.Other),
                        sending.Patient.OtherClinicalInformation);
            }
            return clinicalInformationAsString;
        }

        private ExportDefinition<Sending> CreateRkiExportDefinition()
        {
            var export = new ExportDefinition<Sending>();
            var counties = db.Counties.OrderBy(c => c.ValidSince).ToList();

            var emptyCounty = new County {CountyNumber = ""};
            Func<Sending, County> findCounty =
                s => counties.FirstOrDefault(c => c.IsEqualTo(s.Patient.County)) ?? emptyCounty;

            export.AddField(s => s.Isolate.StemNumber, "klhi_nr");
            export.AddField(s => s.ReceivingDate.ToReportFormat(), "eing");
            export.AddField(s => s.SamplingDate.ToReportFormat(), "ent");
            export.AddField(s => ExportSamplingLocation(s.SamplingLocation, s), "mat");
            export.AddField(s => s.Patient.BirthDate.HasValue ? s.Patient.BirthDate.Value.Month : 0, "geb_monat");
            export.AddField(s => s.Patient.BirthDate.HasValue ? s.Patient.BirthDate.Value.Year : 0, "geb_jahr");
            export.AddField(s => ExportGender(s.Patient.Gender), "geschlecht");
            export.AddField(s => ExportToString(s.Patient.HibVaccination), "hib_impf");
            export.AddField(s => ExportToString(s.Isolate.Evaluation), "styp");
            export.AddField(s => ExportToString(s.Isolate.BetaLactamase), "b_lac");
            export.AddField(s => findCounty(s).CountyNumber, "kreis_nr");
            export.AddField(s => new string(findCounty(s).CountyNumber.Take(2).ToArray()), "bundesland");
            export.AddField(s => s.SenderId, "einsender");
            export.AddField(s => ExportToString(s.Patient.County), "landkreis");
            export.AddField(s => ExportToString(s.Patient.State), "bundeslandName");
            AddEpsilometerTestFields(export, Antibiotic.Ampicillin, "ampicillinMHK", "ampicillinBewertung");
            AddEpsilometerTestFields(export, Antibiotic.AmoxicillinClavulanate, "amoxicillinClavulansaeureMHK", "bewertungAmoxicillinClavulansaeure");

            return export;
        }

        private static void AddEpsilometerTestFields(ExportDefinition<Sending> export, Antibiotic antibiotic, string measurementHeaderParameter = null, string evaluationHeaderParameter = null)
        {
            var antibioticName = ExportToString(antibiotic);
            var measurementHeader = measurementHeaderParameter ?? string.Format("{0} MHK", antibioticName);
            var evaluationHeader = evaluationHeaderParameter ?? string.Format("{0} Bewertung", antibioticName);

            export.AddField(s => FindEpsilometerTestMeasurement(s, antibiotic), measurementHeader);
            export.AddField(s => ExportToString(FindEpsilometerTestEvaluation(s, antibiotic)), evaluationHeader);
        }

        private static string ExportGender(Gender? gender)
        {
            return gender == null
                ? "?"
                : EnumEditor.GetEnumDescription(gender).Substring(0, 1);
        }

        private static string ExportSamplingLocation(SamplingLocation location, Sending sending)
        {
            if (location == SamplingLocation.Other)
            {
                return sending.OtherSamplingLocation;
            }
            return ExportToString(location);
        }

        private static string ExportToString<T>(T value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            var type = value.GetTypeOrNullableType();
            if (type.IsEnum)
            {
                return EnumEditor.GetEnumDescription(value);
            }
            return value.ToString();
        }

        private static EpsilometerTest FindEpsilometerTestResult(Sending sending, Antibiotic antibiotic)
        {
            var eTestResult = sending.Isolate.EpsilometerTests.SingleOrDefault(
                e => e.EucastClinicalBreakpoint.Antibiotic == antibiotic);
            return eTestResult;
        }

        private static double? FindEpsilometerTestMeasurement(Sending sending, Antibiotic antibiotic)
        {
            var eTestResult = FindEpsilometerTestResult(sending, antibiotic);
            if (eTestResult == null)
            {
                return null;
            }
            return Math.Round(eTestResult.Measurement, 3);
        }

        private static EpsilometerTestResult? FindEpsilometerTestEvaluation(Sending sending, Antibiotic antibiotic)
        {
            var eTestResult = FindEpsilometerTestResult(sending, antibiotic);
            if (eTestResult == null)
            {
                return null;
            }
            return eTestResult.Result;
        }

        [HttpPost]
        [Authorize(Roles = DefaultRoles.User)]
        public JsonResult DataTableAjax([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestParameters)
        {
            var query = NotDeletedSendings().Select(x => new
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
                x.SenderLaboratoryNumber,
                PatientPostalCode = x.Patient.PostalCode,
                SenderPostalCode = db.Senders.FirstOrDefault(s => s.SenderId == x.SenderId).PostalCode,
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
                    PatientPostalCode = x.PatientPostalCode,
                    SenderPostalCode = x.SenderPostalCode,
                    SenderLaboratoryNumber = x.SenderLaboratoryNumber,
                    FullTextSearch = string.Join(" ",
                        x.Initials, x.BirthDate.ToReportFormat(),
                        x.StemNumber, x.ReceivingDate.ToReportFormat(),
                        invasive, samplingLocation, laboratoryNumber,
                        x.PatientPostalCode, x.SenderPostalCode, x.SenderLaboratoryNumber)
                        .ToLower()
                };
            }).ToList();

            var totalCount = queryable.Count();

            var searchValue = requestParameters.Search.Value.ToLower();
            var columns = requestParameters.Columns;
            var filteredColumns = columns.GetFilteredColumns().ToList();
            if (!string.IsNullOrEmpty(searchValue))
            {
                queryable = queryable.Where(x => x.FullTextSearch.Contains(searchValue)).ToList();
            }
            else if (filteredColumns.Any())
            {
                foreach (var column in filteredColumns)
                {
                    Func<QueryRecord, object> keySelector = x => x.GetType().GetProperty(column.Data).GetValue(x);
                    if (column.Data == "BirthDate")
                    {
                        keySelector = x => x.BirthDate.ToReportFormat();
                    }
                    queryable = queryable.Where(x => keySelector(x) != null && keySelector(x).ToString().
                        ToLower().Contains(column.Search.Value.ToLower())).ToList();
                }
            }

            foreach (var column in columns.GetSortedColumns())
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
                    x.PatientPostalCode,
                    x.SenderPostalCode,
                    x.SenderLaboratoryNumber,
                    Link = CreateEditControls(x.SendingId, x.IsolateId)
                });

            var dataTablesResult = new DataTablesResponse(
                requestParameters.Draw,
                pagedData,
                queryable.Count(),
                totalCount);

            return Json(dataTablesResult);
        }

        private IQueryable<Sending> NotDeletedSendings()
        {
            return db.Sendings.Where(s => !s.Deleted);
        }

        private static string CreateReportGeneratedIcon(bool reportGenerated)
        {
            return string.Format(
                "<span style=\"color:{0}\" class=\"glyphicon {1}\" aria-hidden=\"true\"></span>",
                reportGenerated ? "#00CC00" : "#CC0000",
                reportGenerated ? "glyphicon-ok-sign" : "glyphicon-remove-sign");
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
                Url.Action("Isolate", "Report", new {id = isolateId}));
            builder.Append("</div>");
            return builder.ToString();
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
            public string PatientPostalCode { get; set; }
            public string SenderPostalCode { get; set; }
            public string SenderLaboratoryNumber { get; set; }
        }
    }
}