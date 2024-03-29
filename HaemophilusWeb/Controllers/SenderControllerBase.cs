﻿using HaemophilusWeb.Models;
using HaemophilusWeb.Tools;
using HaemophilusWeb.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System;
using System.Data.Entity;
using HaemophilusWeb.Utils;

namespace HaemophilusWeb.Controllers
{

    public abstract class SenderControllerBase<TPatient, TSending> : ControllerBase
        where TPatient : PatientBase, new()
        where TSending : SendingBase<TPatient>, new()
    {
        protected abstract DatabaseType DatabaseType { get; }

        protected abstract IQueryable<TSending> QuerySendings();

        private readonly IApplicationDbContext db;

        protected SenderControllerBase(IApplicationDbContext db)
        {
            this.db = db;
        }

        public ActionResult Index()
        {
            return View(NotDeletedSenders().ToList());
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
            [Bind(Include = "SenderId,Name,Department,StreetWithNumber,PostalCode,City,Phone1,Phone2,Fax,Email,Remark")]
            Sender sender)
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
            [Bind(Include = "SenderId,Name,Department,StreetWithNumber,PostalCode,City,Phone1,Phone2,Fax,Email,Remark")]
            Sender sender)
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

            var sendings = db.Sendings.Where(s => s.SenderId == sender.SenderId && !s.Deleted).ToList();
            ViewBag.Sendings = sendings;

            var meningoSendings = db.MeningoSendings.Where(s => s.SenderId == sender.SenderId && !s.Deleted).ToList();
            ViewBag.MeningoSendings = meningoSendings;

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

        public ActionResult Export(FromToQuery query)
        {
            if (query.From == DateTime.MinValue)
            {
                var lastYear = DateTime.Now.Year - 1;
                var exportQuery = new FromToQuery
                {
                    From = new DateTime(lastYear, 1, 1),
                    To = new DateTime(lastYear, 12, 31)
                };
                return View(exportQuery);
            }

            var sendings = SendersMatchingExportQuery(query).ToList();
            return ExportToExcel(query, sendings, CreateExportDefinition(query),
                $"Einsender-{EnumUtils.GetEnumDescription<DatabaseType>(DatabaseType)}");
        }

        private ExportDefinition<Sender> CreateExportDefinition(FromToQuery query)
        {
            var export = new ExportDefinition<Sender>();
            var sendings = QuerySendings().Where
            (s => (s.SamplingDate == null && s.ReceivingDate >= query.From && s.ReceivingDate <= query.To)
                  || (s.SamplingDate >= query.From && s.SamplingDate <= query.To)).ToList();

            Func<Sender, long> senderSendingCount = sender => sendings.Count(s => sender.SenderId == s.SenderId);
            Func<Sender, string> senderLaboratoryNumbers =
                sender => string.Join(",",
                    sendings.Where(s => sender.SenderId == s.SenderId)
                        .Select(s => s.GetIsolate().LaboratoryNumberWithPrefix));
            Func<Sender, string> senderStemNumbers =
                sender => string.Join(",",
                    sendings.Where(s => sender.SenderId == s.SenderId).Select(s =>
                        s.GetIsolate().StemNumber.HasValue ? s.GetIsolate().StemNumber.ToString() : "-"));

            export.AddField(s => s.SenderId);
            export.AddField(s => s.Name);
            export.AddField(s => s.Department);
            export.AddField(s => s.StreetWithNumber);
            export.AddField(s => s.PostalCode);
            export.AddField(s => s.City);
            export.AddField(s => s.Phone1);
            export.AddField(s => s.Phone2);
            export.AddField(s => s.Fax);
            export.AddField(s => s.Email);
            export.AddField(s => s.Remark);
            export.AddField(s => senderSendingCount(s), "Anzahl Einsendungen");
            export.AddField(s => senderStemNumbers(s), "Stammnummern");
            export.AddField(s => senderLaboratoryNumbers(s), "Labornummern");

            return export;
        }

        private List<Sender> SendersMatchingExportQuery(FromToQuery query)
        {
            var senderIds = QuerySendings().Where(s => !s.Deleted)
                .Include(s => s.Patient)
                .Where
                (s => (s.SamplingDate == null && s.ReceivingDate >= query.From && s.ReceivingDate <= query.To)
                      || (s.SamplingDate >= query.From && s.SamplingDate <= query.To))
                .Select(s => s.SenderId)
                .ToList();
            return NotDeletedSenders().Where(s => senderIds.Contains(s.SenderId)).ToList();

        }

        private IQueryable<Sender> NotDeletedSenders()
        {
            return db.Senders.Where(s => !s.Deleted);
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