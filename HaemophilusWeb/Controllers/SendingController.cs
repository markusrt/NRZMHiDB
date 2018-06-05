using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using HaemophilusWeb.Models;
using HaemophilusWeb.Utils;
using HaemophilusWeb.Views.Utils;

namespace HaemophilusWeb.Controllers
{
    public class SendingController : SendingControllerBase<Sending, Patient>
    {
        public SendingController() : this(new ApplicationDbContextWrapper(new ApplicationDbContext()))
        {
        }

        public SendingController(IApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
        }

        protected override IDbSet<Isolate> IsolateDbSet()
        {
            return db.Isolates;
        }

        protected override IDbSet<Sending> SendingDbSet()
        {
            return db.Sendings;
        }

        protected override IDbSet<Patient> PatientDbSet()
        {
            return db.Patients;
        }
    }


    [Authorize(Roles = DefaultRoles.User)]
    public abstract class SendingControllerBase<TSending, TPatient> : Controller 
        where TPatient : PatientBase, new()
        where TSending : SendingBase<TPatient>, new()
    {
        protected readonly IApplicationDbContext db;

        protected SendingControllerBase(IApplicationDbContext applicationDbContext)
        {
            db = applicationDbContext;
        }

        private static int CurrentYear
        {
            get
            {
                var currentYear = DateTime.Now.Year;
                return currentYear;
            }
        }

        // GET: /Sending/
        public ActionResult Index()
        {
            return View(SendingDbSet().Where(s => !s.Deleted).ToList());
        }

        // GET: /Sending/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var sending = FindSendingById(id);
            if (sending == null)
            {
                return HttpNotFound();
            }
            return View(sending);
        }

        // GET: /Sending/Create
        public ActionResult Create()
        {
            return CreateEditView(new TSending());
        }

        // POST: /Sending/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TSending sending)
        {
            if (ModelState.IsValid)
            {
                CreateSendingAndAssignStemnumber(sending);
                return RedirectToAction("Index");
            }

            return CreateEditView(sending);
        }

        // GET: /Sending/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var sending = FindSendingById(id);
            if (sending == null)
            {
                return HttpNotFound();
            }
            return CreateEditView(sending);
        }

        // POST: /Sending/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(TSending sending)
        {
            if (ModelState.IsValid)
            {
                db.MarkAsModified(sending);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return CreateEditView(sending);
        }

        public Isolate AssignStemNumber(int? id)
        {
            if (id == null)
            {
                throw new ArgumentException("Sending id is required", "id");
            }
            var sending = FindSendingById(id);
            if (sending == null)
            {
                throw new ArgumentException("Sending id does not exist", "id");
            }
            if (sending.GetIsolate() == null)
            {
                db.WrapInTransaction(() => CreateIsolateWithNewStemAndLaboratoryNumber(sending));
            }
            return sending.GetIsolate();
        }

        public void CreateSendingAndAssignStemnumber(TSending sending)
        {
            SendingDbSet().Add(sending);
            db.SaveChanges();
            AssignStemNumber(sending.GetSendingId());
        }

        internal void AddReferenceDataToViewBag(dynamic viewBag, TSending sending)
        {
            viewBag.PossibleSenders = db.Senders.Where(s => !s.Deleted || s.SenderId == sending.SenderId);
            viewBag.PossiblePatients = PatientDbSet();
            viewBag.PossibleOtherSamplingLocations = SendingDbSet().Where(
                s => !string.IsNullOrEmpty(s.OtherSamplingLocation)).Select(s => s.OtherSamplingLocation).AsDataList();
        }

        protected abstract IDbSet<Isolate> IsolateDbSet();

        protected abstract IDbSet<TSending> SendingDbSet();

        protected abstract IDbSet<TPatient> PatientDbSet();

        private void CreateIsolateWithNewStemAndLaboratoryNumber(TSending sending)
        {
            var nextSequentialIsolateNumber = GetNextSequentialIsolateNumber();
            var nextSequentialStemNumber = GetNextSequentialStemNumber();

            sending.SetIsolate(new Isolate
            {
                Year = CurrentYear,
                YearlySequentialIsolateNumber = nextSequentialIsolateNumber,
                StemNumber = nextSequentialStemNumber
            });
        }

        private int GetNextSequentialStemNumber()
        {
            var lastSequentialStemNumber =
                IsolateDbSet().DefaultIfEmpty()
                    .Max(i => i == null || !i.StemNumber.HasValue ? 0 : i.StemNumber.Value);
            return lastSequentialStemNumber + 1;
        }

        private ActionResult CreateEditView(TSending sending)
        {
            AddReferenceDataToViewBag(ViewBag, sending);
            sending.LaboratoryNumber = sending.IsolateLaboratoryNumber
                                       ?? ReportFormatter.ToLaboratoryNumber(GetNextSequentialIsolateNumber(), CurrentYear);
            return View(sending);
        }

        private TSending FindSendingById(int? id)
        {
            return SendingDbSet().Find(id);
        }

        private int GetNextSequentialIsolateNumber()
        {
            var lastSequentialIsolateNumber =
                db.Isolates.Where(i => i.Year == CurrentYear)
                    .DefaultIfEmpty()
                    .Max(i => i == null ? 0 : i.YearlySequentialIsolateNumber);
            return lastSequentialIsolateNumber + 1;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}