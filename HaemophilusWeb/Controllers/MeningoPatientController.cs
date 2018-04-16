using System.Linq;
using System.Net;
using System.Web.Mvc;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Utils;
using HaemophilusWeb.Views.Utils;

namespace HaemophilusWeb.Controllers
{
    //TODO unify duplicate code with PatientController
    [Authorize(Roles = DefaultRoles.User)]
    public class MeningoPatientController : Controller
    {
        private readonly IApplicationDbContext db;

        public MeningoPatientController() : this(new ApplicationDbContextWrapper(new ApplicationDbContext()))
        {
        }

        public MeningoPatientController(IApplicationDbContext applicationDbContext)
        {
            db = applicationDbContext;
        }

        // GET: /Patient/
        public ActionResult Index()
        {
            return View(db.MeningoPatients.ToList());
        }

        // GET: /Patient/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var patient = db.MeningoPatients.Find(id);
            if (patient == null)
            {
                return HttpNotFound();
            }
            return View(patient);
        }

        // GET: /Patient/Create
        public ActionResult Create()
        {
            return CreateEditView(new MeningoPatient());
        }

        // POST: /Patient/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MeningoPatient patient)
        {
            PopulateEnumFlagProperties(patient);
            if (ModelState.IsValid)
            {
                CreatePatient(patient);
                return RedirectToAction("Index");
            }

            return CreateEditView(patient);
        }

        private void PopulateEnumFlagProperties(MeningoPatient patient)
        {
            patient.ClinicalInformation =
                EnumUtils.ParseCommaSeperatedListOfNamesAsFlagsEnum<MeningoClinicalInformation>(
                    Request.Form["ClinicalInformation"]);
            patient.Epidemiology =
                EnumUtils.ParseCommaSeperatedListOfNamesAsFlagsEnum<Epidemiology>(
                    Request.Form["Epidemiology"]);
            patient.RiskFactors =
                EnumUtils.ParseCommaSeperatedListOfNamesAsFlagsEnum<RiskFactors>(
                    Request.Form["RiskFactors"]);
        }

        internal void CreatePatient(MeningoPatient patient)
        {
            db.MeningoPatients.Add(patient);
            db.SaveChanges();
        }

        // GET: /Patient/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var patient = db.MeningoPatients.Find(id);
            if (patient == null)
            {
                return HttpNotFound();
            }
            return CreateEditView(patient);
        }

        private ActionResult CreateEditView(MeningoPatient patient)
        {
            AddReferenceDataToViewBag(ViewBag);
            return View(patient);
        }

        internal void AddReferenceDataToViewBag(dynamic viewBag)
        {
            viewBag.PossibleOtherClinicalInformation = db.MeningoPatients.Where(
                s => !string.IsNullOrEmpty(s.OtherClinicalInformation)).
                Select(s => s.OtherClinicalInformation).AsDataList();
            viewBag.PossibleTherapyDetails = db.MeningoPatients.Where(
                s => !string.IsNullOrEmpty(s.TherapyDetails)).
                Select(s => s.TherapyDetails).AsDataList();
            viewBag.PossibleOtherUnderlyingDisease = db.MeningoPatients.Where(
                    s => !string.IsNullOrEmpty(s.OtherUnderlyingDisease)).
                Select(s => s.OtherUnderlyingDisease).AsDataList();
            viewBag.PossibleOtherRiskFactors = db.MeningoPatients.Where(
                    s => !string.IsNullOrEmpty(s.OtherRiskFactor)).
                Select(s => s.OtherRiskFactor).AsDataList();
        }

        // POST: /Patient/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MeningoPatient patient)
        {
            PopulateEnumFlagProperties(patient);
            if (ModelState.IsValid)
            {
                EditPatient(patient);
                return RedirectToAction("Index");
            }
            return CreateEditView(patient);
        }

        internal void EditPatient(MeningoPatient patient)
        {
            db.MarkAsModified(patient);
            db.SaveChanges();
        }

        // GET: /Patient/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var patient = db.MeningoPatients.Find(id);
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
            var patient = db.MeningoPatients.Find(id);
            db.MeningoPatients.Remove(patient);
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