using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DocumentFormat.OpenXml.Office2016.Excel;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Utils;
using HaemophilusWeb.ViewModels;
using HaemophilusWeb.Views.Utils;

namespace HaemophilusWeb.Controllers
{
    public abstract class IsolateControllerBase<TIsolateModel, TISolateViewModel> : Controller 
        where TIsolateModel : IsolateCommon
        where TISolateViewModel : IsolateCommon
    {
        private IApplicationDbContext db;
        private readonly DatabaseType databaseType;
        private const string HaemophilusDefaultMaldiTofBiotyperBestMatch = "Haemophilus influenzae";
        private const string MeningococciDefaultMaldiTofBiotyperBestMatch = "Neisseria meningitidis";

        private readonly AntibioticPriorityListComparer antibioticPriorityListComparer;

        protected IsolateControllerBase(IApplicationDbContext applicationDbContext, DatabaseType databaseType)
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

        private void AddBreakpointsAndAntibioticsToViewBag()
        {
            ViewBag.ClinicalBreakpoints = db.EucastClinicalBreakpoints.Where(e => e.ValidFor==databaseType).OrderByDescending(b => b.ValidFrom);
            ViewBag.Antibiotics = AvailableAntibiotics.OrderBy(a => a, antibioticPriorityListComparer);
            ViewBag.PossibleMaldiTofBiotyperBestMatches = PossibleMaldiTofBiotyperBestMatches();
            ViewBag.DefaultMaldiTofBiotyperBestMatch = DefaultMaldiTofBiotyperBestMatch();
        }

        private IEnumerable<string> PossibleMaldiTofBiotyperBestMatches()
        {
            var defaultBestMatch = DefaultMaldiTofBiotyperBestMatch();
            if (databaseType == DatabaseType.Haemophilus)
            {
                return db.Isolates.Where(i => !string.IsNullOrEmpty(i.MaldiTofBiotyperBestMatch))
                    .Select(i => i.MaldiTofBiotyperBestMatch)
                    .Concat(new[] { defaultBestMatch })
                    .AsDataList();
            }

            return db.MeningoIsolates.Where(i => !string.IsNullOrEmpty(i.MaldiTofBiotyperBestMatch))
                .Select(i => i.MaldiTofBiotyperBestMatch)
                .Concat(new[] { defaultBestMatch })
                .AsDataList();
        }

        private string DefaultMaldiTofBiotyperBestMatch()
        {
            return databaseType == DatabaseType.Haemophilus
                ? HaemophilusDefaultMaldiTofBiotyperBestMatch
                : MeningococciDefaultMaldiTofBiotyperBestMatch;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        protected void HandleDbUpdateException(DbUpdateException exception)
        {
            if (exception.AnyMessageMentions("IX_StemNumber"))
            {
                ModelState.AddModelError("StemNumber", "Diese Stammnummer ist bereits vergeben");
            }
            else if (exception.AnyMessageMentions("IX_LaboratoryNumber"))
            {
                ModelState.AddModelError("LaboratoryNumber", "Diese Labornummer ist bereits vergeben");
            }
            else
            {
                throw exception;
            }
        }
        
        protected void CreateAndEditPreparations(TISolateViewModel isolateViewModel)
        {
            isolateViewModel.RealTimePcrResult =
                EnumUtils.ParseCommaSeperatedListOfNamesAsFlagsEnum<RealTimePcrResult>(
                    Request.Form["RealTimePcrResult"]);
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