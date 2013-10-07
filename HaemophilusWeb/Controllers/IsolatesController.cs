using System.Data;
using System.Linq;
using System.Web.Mvc;
using HaemophilusWeb.Models;

namespace HaemophilusWeb.Controllers
{
    public class IsolatesController : Controller
    {
        private readonly HaemophilusWebContext context = new HaemophilusWebContext();

        //
        // GET: /Isolates/

        public ViewResult Index()
        {
            return View(context.Isolates.ToList());
        }

        //
        // GET: /Isolates/Details/5

        public ViewResult Details(int id)
        {
            Isolate isolate = context.Isolates.Single(x => x.HaemophilusId == id);
            return View(isolate);
        }

        //
        // GET: /Isolates/Create

        public ActionResult Create()
        {
            ViewBag.PossibleSenders = context.Senders;
            return View(new Isolate());
        }

        //
        // POST: /Isolates/Create

        [HttpPost]
        public ActionResult Create(Isolate isolate)
        {
            if (ModelState.IsValid)
            {
                context.Isolates.Add(isolate);
                context.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.PossibleSenders = context.Senders;
            return View(isolate);
        }

        //
        // GET: /Isolates/Edit/5

        public ActionResult Edit(int id)
        {
            Isolate isolate = context.Isolates.Single(x => x.HaemophilusId == id);
            ViewBag.PossibleSenders = context.Senders;
            return View(isolate);
        }

        //
        // POST: /Isolates/Edit/5

        [HttpPost]
        public ActionResult Edit(Isolate isolate)
        {
            if (ModelState.IsValid)
            {
                context.Entry(isolate).State = EntityState.Modified;
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.PossibleSenders = context.Senders;
            return View(isolate);
        }

        //
        // GET: /Isolates/Delete/5

        public ActionResult Delete(int id)
        {
            Isolate isolate = context.Isolates.Single(x => x.HaemophilusId == id);
            return View(isolate);
        }

        //
        // POST: /Isolates/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Isolate isolate = context.Isolates.Single(x => x.HaemophilusId == id);
            context.Isolates.Remove(isolate);
            context.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}