using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using HaemophilusWeb.Models;
using HaemophilusWeb.Views.Utils;

namespace HaemophilusWeb.Controllers
{
    public class PatientController : Controller
    {
        private readonly IApplicationDbContext db;

        public PatientController() : this(new ApplicationDbContextWrapper(new ApplicationDbContext()))
        {
        }

        public PatientController(IApplicationDbContext applicationDbContext)
        {
            db = applicationDbContext;
        }

        // GET: /Patient/
        public ActionResult Index()
        {
            return View(db.Patients.ToList());
        }

        // GET: /Patient/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var patient = db.Patients.Find(id);
            if (patient == null)
            {
                return HttpNotFound();
            }
            return View(patient);
        }

        // GET: /Patient/Create
        public ActionResult Create()
        {
            return CreateEditView(new Patient());
        }

        // POST: /Patient/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Patient patient)
        {
            if (ModelState.IsValid)
            {
                CreatePatient(patient);
                return RedirectToAction("Index");
            }

            return CreateEditView(patient);
        }

        internal void CreatePatient(Patient patient)
        {
            db.Patients.Add(patient);
            db.SaveChanges();
        }

        // GET: /Patient/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var patient = db.Patients.Find(id);
            if (patient == null)
            {
                return HttpNotFound();
            }
            return CreateEditView(patient);
        }

        private ActionResult CreateEditView(Patient patient)
        {
            AddReferenceDataToViewBag(ViewBag);
            return View(patient);
        }

        internal void AddReferenceDataToViewBag(dynamic viewBag)
        {
            viewBag.PossibleOtherClinicalInformation = db.Patients.Where(
                s => !string.IsNullOrEmpty(s.OtherClinicalInformation)).
                Select(s => s.OtherClinicalInformation).AsDataList();
            viewBag.PossibleTherapyDetails = db.Patients.Where(
                s => !string.IsNullOrEmpty(s.TherapyDetails)).
                Select(s => s.TherapyDetails).AsDataList();
        }

        // POST: /Patient/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Patient patient)
        {
            if (ModelState.IsValid)
            {
                db.Entry(patient).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return CreateEditView(patient);
        }

        // GET: /Patient/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var patient = db.Patients.Find(id);
            if (patient == null)
            {
                return HttpNotFound();
            }
            return View(patient);
        }

        // POST: /Patient/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var patient = db.Patients.Find(id);
            db.Patients.Remove(patient);
            db.SaveChanges();
            return RedirectToAction("Index");
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