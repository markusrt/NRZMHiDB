using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using HaemophilusWeb.Models;
using HaemophilusWeb.Utils;
using HaemophilusWeb.Views.Utils;

namespace HaemophilusWeb.Controllers
{
    [Authorize(Roles = DefaultRoles.User)]
    public class SendingController : Controller
    {
        private readonly IApplicationDbContext db;

        public SendingController() : this(new ApplicationDbContextWrapper(new ApplicationDbContext()))
        {
        }

        public SendingController(IApplicationDbContext applicationDbContext)
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
            return View(db.Sendings.Where(s => !s.Deleted).ToList());
        }

        // GET: /Sending/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var sending = db.Sendings.Find(id);
            if (sending == null)
            {
                return HttpNotFound();
            }
            return View(sending);
        }

        // GET: /Sending/Create
        public ActionResult Create()
        {
            return CreateEditView(new Sending());
        }

        // POST: /Sending/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Sending sending)
        {
            if (ModelState.IsValid)
            {
                CreateSendingAndAssignStemnumber(sending);
                return RedirectToAction("Index");
            }

            return CreateEditView(sending);
        }

        internal void CreateSendingAndAssignStemnumber(Sending sending)
        {
            db.Sendings.Add(sending);
            db.SaveChanges();
            AssignStemNumber(sending.SendingId);
        }

        internal void AddReferenceDataToViewBag(dynamic viewBag, Sending sending)
        {
            viewBag.PossibleSenders = db.Senders.Where(s => !s.Deleted || s.SenderId == sending.SenderId);
            viewBag.PossiblePatients = db.Patients;
            viewBag.PossibleOtherSamplingLocations = db.Sendings.Where(
                s => !string.IsNullOrEmpty(s.OtherSamplingLocation)).Select(s => s.OtherSamplingLocation).AsDataList();
        }

        // GET: /Sending/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var sending = db.Sendings.Find(id);
            if (sending == null)
            {
                return HttpNotFound();
            }
            return CreateEditView(sending);
        }

        public Isolate AssignStemNumber(int? id)
        {
            if (id == null)
            {
                throw new ArgumentException("Sending id is required", "id");
            }
            var sending = db.Sendings.Find(id);
            if (sending == null)
            {
                throw new ArgumentException("Sending id does not exist", "id");
            }
            if (sending.Isolate == null)
            {
                db.WrapInTransaction(() => CreateIsolateWithNewStemAndLaboratoryNumber(sending));
            }
            return sending.Isolate;
        }

        private void CreateIsolateWithNewStemAndLaboratoryNumber(Sending sending)
        {
            var nextSequentialIsolateNumber = GetNextSequentialIsolateNumber();
            var nextSequentialStemNumber = GetNextSequentialStemNumber();

            sending.Isolate = new Isolate
            {
                Year = CurrentYear,
                YearlySequentialIsolateNumber = nextSequentialIsolateNumber,
                StemNumber = nextSequentialStemNumber
            };
        }

        private int GetNextSequentialStemNumber()
        {
            var lastSequentialStemNumber =
                db.Isolates.DefaultIfEmpty()
                    .Max(i => i == null || !i.StemNumber.HasValue ? 0 : i.StemNumber.Value);
            return lastSequentialStemNumber + 1;
        }

        private int GetNextSequentialIsolateNumber()
        {
            var lastSequentialIsolateNumber =
                db.Isolates.Where(i => i.Year == CurrentYear)
                    .DefaultIfEmpty()
                    .Max(i => i == null ? 0 : i.YearlySequentialIsolateNumber);
            return lastSequentialIsolateNumber + 1;
        }

        // POST: /Sending/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Sending sending)
        {
            if (ModelState.IsValid)
            {
                db.MarkAsModified(sending);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return CreateEditView(sending);
        }

        private ActionResult CreateEditView(Sending sending)
        {
            AddReferenceDataToViewBag(ViewBag, sending);
            sending.LaboratoryNumber = sending.Isolate == null
                ? ReportFormatter.ToLaboratoryNumber(GetNextSequentialIsolateNumber(), CurrentYear)
                : sending.Isolate.LaboratoryNumber;
            return View(sending);
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