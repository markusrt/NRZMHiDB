using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using HaemophilusWeb.Models;

namespace HaemophilusWeb.Controllers
{
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

        // GET: /Sending/
        public ActionResult Index()
        {
            return View(db.Sendings.ToList());
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
        public ActionResult Create(
            [Bind(
                Include =
                    "SendingId,SenderId,PatientId,SamplingDate,ReceivingDate,Material,OtherMaterial,Invasive,SenderLaboratoryNumber,SenderConclusion,Evaluation"
                )] Sending sending)
        {
            if (ModelState.IsValid)
            {
                db.Sendings.Add(sending);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return CreateEditView(sending);
        }

        internal void AddReferenceDataToViewBag(dynamic viewBag)
        {
            viewBag.PossibleSenders = db.Senders;
            viewBag.PossiblePatients = db.Patients;
            viewBag.PossibleOtherMaterials = db.Sendings.Where(
                s => !string.IsNullOrEmpty(s.OtherMaterial)).Select(s => s.OtherMaterial).Distinct();
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

        public ActionResult AssignStemNumber(int? id)
        {
            if (id == null)
            {
                return
                    Json(new Error {ErrorMessage = "Zur Zuweisung einer Stammnummer wird eine Sendungsnummer benötigt."});
            }
            var sending = db.Sendings.Find(id);
            if (sending == null)
            {
                return
                    Json(new Error
                    {
                        ErrorMessage =
                            string.Format(
                                "Zuweisung einer Stammnummer ist fehlgeschlagen. Die Sendung mit der Nummer '{0}' existiert nicht.",
                                id)
                    });
            }
            if (sending.Isolate == null)
            {
                var currentYear = DateTime.Now.Year;
                var nextSequentialIsolateNumber = GetNextSequentialIsolateNumber(currentYear);

                sending.Isolate = new Isolate
                {
                    Year = currentYear,
                    YearlySequentialIsolateNumber = nextSequentialIsolateNumber
                };

                db.SaveChanges();
            }
            var isolate = sending.Isolate;
            return Json(isolate);
        }

        private int GetNextSequentialIsolateNumber(int currentYear)
        {
            var lastSequentialIsolateNumber =
                db.Isolates.Where(i => i.Year == currentYear)
                    .DefaultIfEmpty()
                    .Max(i => i == null ? 0 : i.YearlySequentialIsolateNumber);
            return lastSequentialIsolateNumber + 1;
        }

        // POST: /Sending/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(
                Include =
                    "SendingId,SenderId,PatientId,SamplingDate,ReceivingDate,Material,OtherMaterial,Invasive,SenderLaboratoryNumber,SenderConclusion,Evaluation"
                )] Sending sending)
        {
            if (ModelState.IsValid)
            {
                db.Entry(sending).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return CreateEditView(sending);
        }

        private ActionResult CreateEditView(Sending sending)
        {
            AddReferenceDataToViewBag(ViewBag);
            return View(sending);
        }

        // GET: /Sending/Delete/5
        public ActionResult Delete(int? id)
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

        // POST: /Sending/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var sending = db.Sendings.Find(id);
            db.Sendings.Remove(sending);
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