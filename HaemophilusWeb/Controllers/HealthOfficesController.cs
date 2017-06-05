using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using HaemophilusWeb.Models;
using HaemophilusWeb.Tools;

namespace HaemophilusWeb.Controllers
{
    public class HealthOfficesController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        private readonly RkiTool rkiTool = new RkiTool();

        // GET: HealthOffices
        public ActionResult Index()
        {
            return View(db.HealthOffices.ToList());
        }

        // GET: HealthOffices/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var healthOffice = db.HealthOffices.Find(id);
            if (healthOffice == null)
                return HttpNotFound();
            return View(healthOffice);
        }

        // GET: HealthOffices/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: HealthOffices/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "HealthOfficeId,Address,Email,Phone,Fax,PostalCode")] HealthOffice healthOffice)
        {
            if (ModelState.IsValid)
            {
                db.HealthOffices.Add(healthOffice);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(healthOffice);
        }
        

        // GET: HealthOffices/Edit/5
        public ActionResult Edit(int? id, string postalCode = null)
        {
            if (id == null && string.IsNullOrEmpty(postalCode))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            HealthOffice healthOffice;
            if (!string.IsNullOrEmpty(postalCode))
            {
                healthOffice = rkiTool.QueryHealthOffice(postalCode);
                if (healthOffice != null)
                {
                    var healthOfficeAddressWithoutNewline = healthOffice.Address.Replace("\n", "");
                    healthOffice = db.HealthOffices.FirstOrDefault(ho => ho.Address.Replace("\n", "").Replace("\r", "").Equals(healthOfficeAddressWithoutNewline));
                }
            }
            else
            {
                healthOffice = db.HealthOffices.Find(id);
            }

            if (healthOffice == null)
                return HttpNotFound();
            return View(healthOffice);
        }

        // POST: HealthOffices/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "HealthOfficeId,Address,Email,Phone,Fax,PostalCode")] HealthOffice healthOffice)
        {
            if (ModelState.IsValid)
            {
                db.Entry(healthOffice).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(healthOffice);
        }

        // GET: HealthOffices/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var healthOffice = db.HealthOffices.Find(id);
            if (healthOffice == null)
                return HttpNotFound();
            return View(healthOffice);
        }

        // POST: HealthOffices/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var healthOffice = db.HealthOffices.Find(id);
            db.HealthOffices.Remove(healthOffice);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}