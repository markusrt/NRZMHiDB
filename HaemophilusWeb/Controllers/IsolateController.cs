using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity;
using HaemophilusWeb.Models;
using HaemophilusWeb.ViewModels;

namespace HaemophilusWeb.Controllers
{
    public class IsolateController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var isolate = db.Isolates.Include(i => i.Sending).Single(i => i.IsolateId == id);
            if (isolate == null)
            {
                return HttpNotFound();
            }


            return CreateEditView(new IsolateViewModel
            {
                TheIsolate = isolate,
            });
        }

        private ActionResult CreateEditView(IsolateViewModel isolateViewModel)
        {
            ViewBag.ClinicalBreakpoints = db.EucastClinicalBreakpoints.OrderByDescending(b => b.ValidFrom);
            return View(isolateViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "TheIsolate,Ampicillin,AmoxicillinClavulanate,Cefotaxime,Meropenem")] IsolateViewModel
                isolateViewModel)
        {
            if (ModelState.IsValid)
            {
                var isolate = isolateViewModel.TheIsolate;
                db.Entry(isolate).State = EntityState.Modified;
                db.Entry(isolate).Property(x => x.Sending).IsModified = false;
                db.SaveChanges();
                return RedirectToAction("Index", "PatientSending");
            }
            return CreateEditView(isolateViewModel);
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