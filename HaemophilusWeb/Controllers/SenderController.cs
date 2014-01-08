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
    public class SenderController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: /Sender/
        public ActionResult Index()
        {
            return View(db.Senders.ToList());
        }

        // GET: /Sender/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sender sender = db.Senders.Find(id);
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
        public ActionResult Create([Bind(Include="SenderId,Name,Department,StreetWithNumber,PostalCode,City,Phone1,Phone2,Fax,Email,Remark")] Sender sender)
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
            Sender sender = db.Senders.Find(id);
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
        public ActionResult Edit([Bind(Include="SenderId,Name,Department,StreetWithNumber,PostalCode,City,Phone1,Phone2,Fax,Email,Remark")] Sender sender)
        {
            if (ModelState.IsValid)
            {
                db.Entry(sender).State = EntityState.Modified;
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
            Sender sender = db.Senders.Find(id);
            if (sender == null)
            {
                return HttpNotFound();
            }
            return View(sender);
        }

        // POST: /Sender/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Sender sender = db.Senders.Find(id);
            db.Senders.Remove(sender);
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
