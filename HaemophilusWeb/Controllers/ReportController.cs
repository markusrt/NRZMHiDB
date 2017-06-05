using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using HaemophilusWeb.Models;
using HaemophilusWeb.Tools;
using HaemophilusWeb.Utils;

namespace HaemophilusWeb.Controllers
{
    public class ReportController : Controller
    {
        private readonly IApplicationDbContext db;

        private readonly IsolateController isolateController;

        private readonly RkiTool rkiTool = new RkiTool();

        public ReportController()
            : this(new ApplicationDbContextWrapper(new ApplicationDbContext()))
        {
        }

        public ReportController(IApplicationDbContext applicationDbContext)
        {
            db = applicationDbContext;
            isolateController = new IsolateController(db);
        }


        public ActionResult Isolate(int? id)
        {
            AddReportTemplatesToViewBag();
            AddReportSignersToViewBag();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var isolate = isolateController.LoadIsolateById(id);
            if (isolate == null)
            {
                return HttpNotFound();
            }

            var isolateViewModel = isolateController.ModelToViewModel(isolate);
            return View(isolateViewModel);
        }

        [HttpPost]
        public JsonResult QueryHealthOffice(string postalCode)
        {
            var healthOffice = rkiTool.QueryHealthOffice(postalCode);
            if (healthOffice == null)
            {
                return Json(null);
            }

            var healthOfficeAddressWithoutNewline = healthOffice.Address.Replace("\n", "");
            var existingHealthOffice = db.HealthOffices.FirstOrDefault(ho => ho.Address.Replace("\n", "").Replace("\r", "").Equals(healthOfficeAddressWithoutNewline));

            if (existingHealthOffice == null)
            {
                db.HealthOffices.Add(healthOffice);
                db.SaveChanges();
            }

            return existingHealthOffice != null ? Json(existingHealthOffice) : Json(healthOffice);
        }


        [HttpPost]
        public JsonResult ReportGenerated(int? id)
        {
            var isolate = isolateController.LoadIsolateById(id);
            if (isolate != null)
            {
                isolate.ReportDate = DateTime.Now;
                db.MarkAsModified(isolate);
                db.SaveChanges();
            }
            return Json(true);
        }

        private void AddReportTemplatesToViewBag()
        {
            var templatePath = Server.MapPath("~/ReportTemplates");
            var lister = new FileLister(templatePath, ".docx");
            ViewBag.ReportTemplates = lister.Files;
        }

        private void AddReportSignersToViewBag()
        {
            ViewBag.ReportSigners = ConfigurationManager.AppSettings["reportSigners"].Split(';');
        }

    }
}