using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;

namespace HaemophilusWeb.Controllers
{
    [Authorize(Roles = DefaultRoles.User)]
    public abstract class PatientControllerBase<TPatient> : Controller where TPatient : PatientBase, new()
    {
        protected readonly IApplicationDbContext db;

        protected PatientControllerBase(IApplicationDbContext applicationDbContext)
        {
            db = applicationDbContext;
        }

        protected abstract IDbSet<TPatient> DbSet();

        public void CreatePatient(TPatient patient)
        {
            DbSet().Add(patient);
            db.SaveChanges();
        }

        private void DeletePatient(TPatient patient)
        {
            DbSet().Remove(patient);
            db.SaveChanges();
        }

        private TPatient FindPatientById(int? id)
        {
            return DbSet().Find(id);
        }
        
        public abstract void PopulateEnumFlagProperties(TPatient meningoPatient, HttpRequestBase request);

        public abstract void AddReferenceDataToViewBag(dynamic viewBag);

        // GET: /Patient/
        public ActionResult Index()
        {
            return View(DbSet().ToList());
        }

        // GET: /Patient/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var patient = FindPatientById(id);
            if (patient == null)
            {
                return HttpNotFound();
            }
            return View(patient);
        }

        // GET: /Patient/Create
        public ActionResult Create()
        {
            return CreateEditView(new TPatient());
        }

        // POST: /Patient/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TPatient patient)
        {
            PopulateEnumFlagProperties(patient, Request);
            if (ModelState.IsValid)
            {
                CreatePatient(patient);
                return RedirectToAction("Index");
            }

            return CreateEditView(patient);
        }

        // GET: /Patient/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var patient = FindPatientById(id);
            if (patient == null)
            {
                return HttpNotFound();
            }
            return CreateEditView(patient);
        }


        private ActionResult CreateEditView(TPatient patient)
        {
            AddReferenceDataToViewBag(ViewBag);
            return View(patient);
        }

        // POST: /Patient/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(TPatient patient)
        {
            PopulateEnumFlagProperties(patient, Request);
            if (ModelState.IsValid)
            {
                EditPatient(patient);
                return RedirectToAction("Index");
            }
            return CreateEditView(patient);
        }

        internal void EditPatient(TPatient patient)
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
            var patient = FindPatientById(id);
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
            var patient = FindPatientById(id);
            DeletePatient(patient);
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