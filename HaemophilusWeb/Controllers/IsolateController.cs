using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using HaemophilusWeb.Models;
using HaemophilusWeb.ViewModels;

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
            var isolate = db.Isolates.Single(i => i.IsolateId==id);
            if (isolate == null)
            {
                return HttpNotFound();
            }

            var isolateViewModel = new IsolateViewModel {TheIsolate = isolate};
            isolateViewModel.CurrentClinicalBreakpoints = new List<EucastClinicalBreakpoint>();
            isolateViewModel.CurrentClinicalBreakpoints.Add(new EucastClinicalBreakpoint() { Penicillin = "Ampicillin" });
            return View(isolateViewModel);
        }

        // POST: /Isolate/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "TheIsolate,Ampicillin,AmoxicillinClavulanate")]
            IsolateViewModel isolate)
        {
            //if (ModelState.IsValid)
            //{
            //    db.Entry(isolate).State = EntityState.Modified;
            //    db.SaveChanges();
            //    return RedirectToAction("Index", "PatientSending");
            //}
            //ViewBag.SendingId = new SelectList(db.Sendings, "SendingId", "OtherMaterial", isolate.SendingId);
            if (ModelState.IsValid)
            {
                return View(isolate);
            }
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