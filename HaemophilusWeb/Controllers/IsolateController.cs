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

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var isolate = db.Isolates.Single(i => i.IsolateId == id);
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
                isolate)
        {
            if (ModelState.IsValid)
            {
                return CreateEditView(isolate);
            }
            return CreateEditView(isolate);
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