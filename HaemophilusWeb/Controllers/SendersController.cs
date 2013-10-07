using System.Data;
using System.Linq;
using System.Web.Mvc;
using HaemophilusWeb.Models;

namespace HaemophilusWeb.Controllers
{
    public class SendersController : Controller
    {
        private readonly HaemophilusWebContext context = new HaemophilusWebContext();

        //
        // GET: /Senders/

        public ViewResult Index()
        {
            return View(context.Senders.ToList());
        }

        //
        // GET: /Senders/Details/5

        public ViewResult Details(int id)
        {
            Sender sender = context.Senders.Single(x => x.SenderId == id);
            return View(sender);
        }

        //
        // GET: /Senders/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Senders/Create

        [HttpPost]
        public ActionResult Create(Sender sender)
        {
            if (ModelState.IsValid)
            {
                context.Senders.Add(sender);
                context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(sender);
        }

        //
        // GET: /Senders/Edit/5

        public ActionResult Edit(int id)
        {
            Sender sender = context.Senders.Single(x => x.SenderId == id);
            return View(sender);
        }

        //
        // POST: /Senders/Edit/5

        [HttpPost]
        public ActionResult Edit(Sender sender)
        {
            if (ModelState.IsValid)
            {
                context.Entry(sender).State = EntityState.Modified;
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(sender);
        }

        //
        // GET: /Senders/Delete/5

        public ActionResult Delete(int id)
        {
            Sender sender = context.Senders.Single(x => x.SenderId == id);
            return View(sender);
        }

        //
        // POST: /Senders/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Sender sender = context.Senders.Single(x => x.SenderId == id);
            context.Senders.Remove(sender);
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