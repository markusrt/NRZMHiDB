using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using HaemophilusWeb.Models;

namespace HaemophilusWeb.Controllers
{
    [Authorize(Roles = DefaultRoles.User)]
    public class EucastClinicalBreakpointsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: EucastClinicalBreakpoints
        public ActionResult Index()
        {
            return View(db.EucastClinicalBreakpoints.ToList());
        }

        // GET: EucastClinicalBreakpoints/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EucastClinicalBreakpoint eucastClinicalBreakpoint = db.EucastClinicalBreakpoints.Find(id);
            if (eucastClinicalBreakpoint == null)
            {
                return HttpNotFound();
            }
            return View(eucastClinicalBreakpoint);
        }

        // GET: EucastClinicalBreakpoints/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: EucastClinicalBreakpoints/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "EucastClinicalBreakpointId,Antibiotic,AntibioticDetails,Version,ValidFor,ValidFrom,MicBreakpointSusceptible,MicBreakpointResistent")] EucastClinicalBreakpoint eucastClinicalBreakpoint)
        {
            if (ModelState.IsValid)
            {
                db.EucastClinicalBreakpoints.Add(eucastClinicalBreakpoint);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(eucastClinicalBreakpoint);
        }

        // GET: EucastClinicalBreakpoints/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EucastClinicalBreakpoint eucastClinicalBreakpoint = db.EucastClinicalBreakpoints.Find(id);
            if (eucastClinicalBreakpoint == null)
            {
                return HttpNotFound();
            }
            return View(eucastClinicalBreakpoint);
        }

        // POST: EucastClinicalBreakpoints/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "EucastClinicalBreakpointId,Antibiotic,AntibioticDetails,Version,ValidFor,ValidFrom,MicBreakpointSusceptible,MicBreakpointResistent")] EucastClinicalBreakpoint eucastClinicalBreakpoint)
        {
            if (ModelState.IsValid)
            {
                db.Entry(eucastClinicalBreakpoint).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(eucastClinicalBreakpoint);
        }

        // GET: EucastClinicalBreakpoints/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EucastClinicalBreakpoint eucastClinicalBreakpoint = db.EucastClinicalBreakpoints.Find(id);
            if (eucastClinicalBreakpoint == null)
            {
                return HttpNotFound();
            }
            return View(eucastClinicalBreakpoint);
        }

        // POST: EucastClinicalBreakpoints/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            EucastClinicalBreakpoint eucastClinicalBreakpoint = db.EucastClinicalBreakpoints.Find(id);
            db.EucastClinicalBreakpoints.Remove(eucastClinicalBreakpoint);
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
