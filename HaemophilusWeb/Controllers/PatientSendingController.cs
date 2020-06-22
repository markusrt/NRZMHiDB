using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using DataTables.Mvc;
using FluentValidation;
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
        public ActionResult PubMlstExport(FromToQuery query)
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

            //TODO clarify and merge method with Iris-Export should actually be the same
            var sendings = SendingsMatchingExportQuery(query, ExportType.Iris).ToList();

            return ExportToExcel(query, sendings, new HaemophilusPubMlstExport(), "PubMLST");
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

        protected override IEnumerable<Sending> SendingsMatchingExportQuery(FromToQuery query, ExportType exportType)
        {
            var samplingLocations = exportType == ExportType.Rki || exportType == ExportType.Iris
                ? new List<SamplingLocation> { SamplingLocation.Blood, SamplingLocation.Liquor }
                : new List<SamplingLocation> { SamplingLocation.Blood, SamplingLocation.Liquor, SamplingLocation.Other };

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
                x.Invasive,
                x.Isolate.YearlySequentialIsolateNumber,
                x.Isolate.Year,
                x.SenderLaboratoryNumber,
                PatientPostalCode = x.Patient.PostalCode,
                SenderPostalCode = db.Senders.FirstOrDefault(s => s.SenderId == x.SenderId).PostalCode,
            });
            var list = query.ToList();
            var queryRecords = list.Select(x =>
            {
                var invasive = EnumEditor.GetEnumDescription(x.Invasive);
                var samplingLocation = x.SamplingLocation == SamplingLocation.Other
                    ? Server.HtmlEncode(x.OtherSamplingLocation)
                    : EnumEditor.GetEnumDescription(x.SamplingLocation);
                var laboratoryNumber = ReportFormatter.ToLaboratoryNumber(x.YearlySequentialIsolateNumber, x.Year, DatabaseType.Haemophilus);
                return new QueryRecord
                {
                    SendingId = x.SendingId,
                    IsolateId = x.IsolateId,
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
    }

    public abstract class PatientSendingControllerBase<TViewModel, TPatient, TSending> : ControllerBase
        where TViewModel : PatientSendingViewModel<TPatient, TSending>, new()
        where TPatient : PatientBase, new()
        where TSending : SendingBase<TPatient>, new()
    {
        protected readonly IApplicationDbContext db;
        private readonly PatientControllerBase<TPatient> patientController;
        private readonly SendingControllerBase<TSending, TPatient> sendingController;

        public PatientSendingControllerBase(IApplicationDbContext applicationDbContext, PatientControllerBase<TPatient> patientController,
            SendingControllerBase<TSending, TPatient> sendingController)
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

            return CreateEditView(new TViewModel
            {
                Patient = (TPatient) patientResult.Model,
                Sending = (TSending) sendingResult.Model
            });
        }

        [Authorize(Roles = DefaultRoles.User)]
        public ActionResult Delete(int? id)
        {
            return View(LoadSendingFromSendingController(id));
        }

        [Authorize(Roles = DefaultRoles.User)]
        public ActionResult Deleted()
        {
            return View(NotDeletedSendings().Include(s=>s.Patient).AsEnumerable().Select(CreatePatientSending).ToList());
        }

        protected abstract IDbSet<TSending> SendingDbSet();

        protected abstract IDbSet<TPatient> PatientDbSet();

        protected abstract void CreateAndEditPreparationsExtensions(TViewModel patientSending);

        protected abstract string IsolateControllerName { get; }

        private void CreateAndEditPreparations(TViewModel patientSending)
        {
            patientController.PopulateEnumFlagProperties(patientSending.Patient, Request);
            CreateAndEditPreparationsExtensions(patientSending);
        }

        private TViewModel CreatePatientSending(TSending sending)
        {
            return new TViewModel
            {
                Patient = sending.Patient,
                Sending = sending
            };
        }

        protected IQueryable<TSending> NotDeletedSendings()
        {
            return SendingDbSet().Where(s => !s.Deleted);
        }

        protected abstract IEnumerable<TSending> SendingsMatchingExportQuery(FromToQuery query, ExportType additionalFilters);

        protected abstract ExportDefinition<TSending> CreateRkiExportDefinition();

        protected abstract ExportDefinition<TSending> CreateLaboratoryExportDefinition();

        protected abstract List<QueryRecord> QueryRecords();

        private ViewResult CreateEditView(TViewModel patientSending)
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

        protected TSending LoadSendingFromSendingController(int? id)
        {
            var sendingResult = sendingController.Edit(id) as ViewResult;
            var sending = (TSending) sendingResult.Model;
            return sending;
        }

        [HttpPost]
        [Authorize(Roles = DefaultRoles.User)]
        public ActionResult Edit(TViewModel patientSending)
        {
            CreateAndEditPreparations(patientSending);

            if (ModelState.IsValid)
            {
                db.MarkAsModified(patientSending.Patient);
                db.MarkAsModified(patientSending.Sending);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return CreateEditView(patientSending);
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
        public ActionResult Undelete(int? id)
        {
            var sending = LoadSendingFromSendingController(id);
            sending.Deleted = false;
            return EditUnvalidated(sending);
        }


        private ActionResult EditUnvalidated(TSending sending)
        {
            ActionResult result = View();
            db.PerformWithoutSaveValidation(() => result = sendingController.Edit(sending));
            return result;
        }


        [HttpPost]
        [Authorize(Roles = DefaultRoles.User)]
        public ActionResult Create(TViewModel patientSending)
        {
            CreateAndEditPreparations(patientSending);
            ValidatePatientDoesNotAlreadyExist(patientSending);

            if (ModelState.IsValid && !patientSending.DuplicatePatientDetected)
            {
                var patient = CreateOrUpdatePatient(patientSending);
                var sending = patientSending.Sending;
                sending.SetPatientId(patient.PatientId);
                sendingController.CreateSendingAndAssignStemAndLaboratoryNumber(sending);
                if (sending.LaboratoryNumber != sending.IsolateLaboratoryNumber)
                {
                    TempData["WarningMessage"] =
                        "Beim Speichern der letzten Einsendung hat sich die Labornummer geändert. Neue Labornummer: " +
                        sending.IsolateLaboratoryNumber;
                }
                return RedirectToAction("Index");
            }
            return CreateEditView(patientSending);
        }

        private TPatient CreateOrUpdatePatient(TViewModel patientSending)
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


        private void ValidatePatientDoesNotAlreadyExist(TViewModel patientSending)
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

        private IQueryable<TPatient> FindDuplicatePatients(TPatient patient)
        {
            var existingPatients = PatientDbSet().Where(
                p => p.Initials == patient.Initials &&
                     ((p.BirthDate.HasValue && patient.BirthDate.HasValue &&
                       p.BirthDate.Value == patient.BirthDate.Value)
                      || (!p.BirthDate.HasValue && !patient.BirthDate.HasValue))
                     && p.PostalCode == patient.PostalCode);
            return existingPatients;
        }

        protected void ValidateModel<T>(T objectToValidate, IValidator<T> validator)
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


        [Authorize(Roles = DefaultRoles.User + "," + DefaultRoles.PublicHealth)]
        public ActionResult RkiExport(FromToQuery query)
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

            var sendings = SendingsMatchingExportQuery(query, ExportType.Rki).ToList();

            return ExportToExcel(query, sendings, CreateRkiExportDefinition(), "RKI");
        }

        [Authorize(Roles = DefaultRoles.User)]
        public ActionResult LaboratoryExport(FromToQuery query)
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

            var sendings = SendingsMatchingExportQuery(query, ExportType.Laboratory).ToList();
            return ExportToExcel(query, sendings, CreateLaboratoryExportDefinition(), "Labor");
        }

        [HttpPost]
        [Authorize(Roles = DefaultRoles.User)]
        public JsonResult DataTableAjax([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestParameters)
        {
            var queryable = QueryRecords();
            var totalCount = queryable.Count;

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
                    ReportGenerated = CreateReportGeneratedIcon(x.ReportStatus),
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

        private static string CreateReportGeneratedIcon(ReportStatus reportStatus)
        {
            var color = "#CC0000";
            var icon = "glyphicon-remove-sign";
            if (reportStatus == ReportStatus.Preliminary)
            {
                color = "#FFCC00";
                icon = "glyphicon-time";
            }
            else if (reportStatus == ReportStatus.Final)
            {
                color = "#00CC00";
                icon = "glyphicon-ok-sign";
            }
            return $"<span style=\"color:{color}\" class=\"glyphicon {icon}\" aria-hidden=\"true\" title=\"{EnumEditor.GetEnumDescription(reportStatus)}\"></span>";
        }

        private string CreateIsolateLink(int isolateId, string laboratoryNumber)
        {
            return
                $"<a class=\"btn-sm btn btn-default\" href=\"{Url.Action("Edit", IsolateControllerName, new {id = isolateId})}\" role=\"button\">{laboratoryNumber}</a>";
        }

        private string CreateEditControls(int sendingId, int isolateId)
        {
            var builder = new StringBuilder();
            builder.Append("<div class=\"btn-group btn-group-sm\">");

            builder.AppendFormat("<a class=\"btn btn-default\" href=\"{0}\" role=\"button\">Bearbeiten</a>",
                Url.Action("Edit", new { id = sendingId }));
            //TODO make this cleaner
            if (this is PatientSendingController)
            {
                builder.AppendFormat("<a class=\"btn btn-default\" href=\"{0}\" role=\"button\">Befund erstellen</a>",
                    Url.Action("Isolate", "Report", new { id = isolateId }));
            }
            else if (this is MeningoPatientSendingController)
            {
                builder.AppendFormat("<a class=\"btn btn-default\" href=\"{0}\" role=\"button\">Befund erstellen</a>",
                    Url.Action("Isolate", "MeningoReport", new { id = isolateId }));
            }
            builder.Append("</div>");
            return builder.ToString();
        }
    }
}