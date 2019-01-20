using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using HaemophilusWeb.Models;
using HaemophilusWeb.Utils;
using HaemophilusWeb.ViewModels;

namespace HaemophilusWeb.Controllers
{
    public abstract class IsolateControllerBase<TIsolateModel, TISolateViewModel> : Controller 
        where TIsolateModel : IsolateCommon
        where TISolateViewModel : IsolateCommon
    {
        private IApplicationDbContext db;
        private readonly DatabaseType databaseType;

        private readonly AntibioticPriorityListComparer antibioticPriorityListComparer;

        public IsolateControllerBase(IApplicationDbContext applicationDbContext, DatabaseType databaseType)
        {
            db = applicationDbContext;
            this.databaseType = databaseType;
            antibioticPriorityListComparer = new AntibioticPriorityListComparer(ConfigurationManager.AppSettings["AntibioticsOrder"]);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var isolate = LoadIsolateById(id);
            if (isolate == null)
            {
                return HttpNotFound();
            }

            var isolateViewModel = ModelToViewModel(isolate);
            return CreateEditView(isolateViewModel);
        }

        protected ActionResult CreateEditView(TISolateViewModel isolateViewModel)
        {
            AddBreakpointsAndAntibioticsToViewBag();
            return View(isolateViewModel);
        }

        public abstract TIsolateModel LoadIsolateById(int? id);

        public abstract TISolateViewModel ModelToViewModel(TIsolateModel isolate);

        protected void AddBreakpointsAndAntibioticsToViewBag()
        {
            ViewBag.ClinicalBreakpoints = db.EucastClinicalBreakpoints.Where(e => e.ValidFor==databaseType).OrderByDescending(b => b.ValidFrom);
            ViewBag.Antibiotics = AvailableAntibiotics.OrderBy(a => a, antibioticPriorityListComparer);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private List<Antibiotic> AvailableAntibiotics
        {
            get
            {
                return db.EucastClinicalBreakpoints.Where(e => e.ValidFor == databaseType).Select(e => e.Antibiotic)
                    .Distinct()
                    .ToList();
            }
        }

    }
}