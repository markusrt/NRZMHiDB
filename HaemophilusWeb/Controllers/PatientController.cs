using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using HaemophilusWeb.Models;

namespace HaemophilusWeb.Controllers
{
    public class PatientController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

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
            return View(new Patient());
        }

        // POST: /Patient/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(
                Include =
                    "PatientId,Initials,BirthDate,PostalCode,Gender,City,County,State,ClinicalInformation,OtherClinicalInformation,HibVaccination,HibVaccinationDate"
                )] Patient patient)
        {
            if (ModelState.IsValid)
            {
                patient.Initials = patient.Initials.ToUpper(CultureInfo.InvariantCulture);
                db.Patients.Add(patient);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(patient);
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
            return View(patient);
        }

        // POST: /Patient/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(
                Include =
                    "PatientId,Initials,BirthDate,PostalCode,Gender,City,County,State,ClinicalInformation,OtherClinicalInformation,HibVaccination,HibVaccinationDate"
                )] Patient patient)
        {
            if (ModelState.IsValid)
            {
                patient.Initials = patient.Initials.ToUpper(CultureInfo.InvariantCulture);
                db.Entry(patient).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(patient);
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
        [HttpPost, ActionName("Delete")]
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