using System.Linq;
using System.Net;
using System.Web.Mvc;
using HaemophilusWeb.Models;

namespace HaemophilusWeb.Controllers
{
    public class SenderController : Controller
    {
        private readonly IApplicationDbContext db;

        public SenderController() : this(new ApplicationDbContextWrapper(new ApplicationDbContext()))
        {
        }

        public SenderController(IApplicationDbContext applicationDbContext)
        {
            db = applicationDbContext;
        }

        // GET: /Sender/
        public ActionResult Index()
        {
            return View(db.Senders.Where(s => !s.Deleted).ToList());
        }

        // GET: /Sender/Deleted
        public ActionResult Deleted()
        {
            return View(db.Senders.Where(s => s.Deleted).ToList());
        }

        // GET: /Sender/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var sender = db.Senders.Find(id);
            if (sender == null)
            {
                return HttpNotFound();
            }
            return View(sender);
        }

        // GET: /Sender/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /Sender/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "SenderId,Name,Department,StreetWithNumber,PostalCode,City,Phone1,Phone2,Fax,Email,Remark")] Sender sender)
        {
            if (ModelState.IsValid)
            {
                db.Senders.Add(sender);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(sender);
        }

        // GET: /Sender/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var sender = db.Senders.Find(id);
            if (sender == null)
            {
                return HttpNotFound();
            }
            return View(sender);
        }

        // POST: /Sender/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "SenderId,Name,Department,StreetWithNumber,PostalCode,City,Phone1,Phone2,Fax,Email,Remark")] Sender sender)
        {
            if (ModelState.IsValid)
            {
                db.MarkAsModified(sender);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(sender);
        }

        // GET: /Sender/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var sender = db.Senders.Find(id);
            if (sender == null)
            {
                return HttpNotFound();
            }
            var sendings = db.Sendings.Where(s => s.SenderId == sender.SenderId).ToList();
            ViewBag.Sendings = sendings;
            return View(sender);
        }

        // POST: /Sender/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var sender = db.Senders.Find(id);
            sender.Deleted = true;
            return EditUnvalidated(sender);
        }

        private ActionResult EditUnvalidated(Sender sender)
        {
            ActionResult result = View();
            db.PerformWithoutSaveValidation(() => result = Edit(sender));
            return result;
        }

        // GET: /Sender/Undelete/5
        public ActionResult Undelete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var sender = db.Senders.Find(id);
            if (sender == null)
            {
                return HttpNotFound();
            }
            sender.Deleted = false;
            return EditUnvalidated(sender);
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