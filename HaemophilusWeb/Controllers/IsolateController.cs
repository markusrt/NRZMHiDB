using System.Data.Entity;
using System.Net;
using System.Web.Mvc;
using HaemophilusWeb.Models;

namespace HaemophilusWeb.Controllers
{
    public class IsolateController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // GET: /Isolate/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var isolate = db.Isolates.Find(id);
            if (isolate == null)
            {
                return HttpNotFound();
            }
            ViewBag.SendingId = new SelectList(db.Sendings, "SendingId", "OtherMaterial", isolate.SendingId);
            return View(isolate);
        }

        // POST: /Isolate/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(
                Include =
                    "SendingId,IsolateId,YearlySequentialIsolateNumber,Year,FactorTest,Agglutination,BetaLactamase,Oxidase,OuterMembraneProteinP2,FuculoKinase,OuterMembraneProteinP6,BexA,SerotypePcr,RibosomalRna16S,ApiNh"
                )] Isolate isolate)
        {
            if (ModelState.IsValid)
            {
                db.Entry(isolate).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", "PatientSending");
            }
            ViewBag.SendingId = new SelectList(db.Sendings, "SendingId", "OtherMaterial", isolate.SendingId);
            return View(isolate);
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