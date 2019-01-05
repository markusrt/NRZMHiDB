using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using HaemophilusWeb.Models;
using HaemophilusWeb.Utils;
using HaemophilusWeb.ViewModels;

namespace HaemophilusWeb.Controllers
{
    public abstract class IsolateControllerBase<TIsolateModel> : Controller where TIsolateModel : IsolateCommon
    {
        private IApplicationDbContext db;

        private readonly AntibioticPriorityListComparer antibioticPriorityListComparer;
        private readonly List<Antibiotic> primaryAntibiotics;

        public IsolateControllerBase(IApplicationDbContext applicationDbContext)
        {
            db = applicationDbContext;
            antibioticPriorityListComparer = new AntibioticPriorityListComparer(ConfigurationManager.AppSettings["AntibioticsOrder"]);
            primaryAntibiotics = EnumUtils.ParseCommaSeperatedListOfNames<Antibiotic>(ConfigurationManager.AppSettings["PrimaryAntibiotics"]);
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

        protected ActionResult CreateEditView(IsolateViewModel isolateViewModel)
        {
            AddBreakpointsAndAntibioticsToViewBag();
            return View(isolateViewModel);
        }

        public abstract TIsolateModel LoadIsolateById(int? id);

        public abstract IsolateViewModel ModelToViewModel(TIsolateModel isolate);

        protected void AddBreakpointsAndAntibioticsToViewBag()
        {
            ViewBag.ClinicalBreakpoints = db.EucastClinicalBreakpoints.OrderByDescending(b => b.ValidFrom);
            ViewBag.Antibiotics = AvailableAntibiotics.OrderBy(a => a, antibioticPriorityListComparer);
        }

        protected ICollection<EpsilometerTestViewModel> EpsilometerTestsModelToViewModel(IEnumerable<EpsilometerTest> epsilometerTests)
        {
            var epsilometerTestViewModels = new List<EpsilometerTestViewModel>();
            var availableAntibiotics = AvailableAntibiotics;
            var missingPrimaryAntibiotics = new List<Antibiotic>(Enumerable.Where<Antibiotic>(primaryAntibiotics, a => availableAntibiotics.Contains(a)));

            //First one is empty
            epsilometerTestViewModels.Add(new EpsilometerTestViewModel());

            foreach (var epsilometerTest in epsilometerTests.OrderBy(e => e.EucastClinicalBreakpoint.Antibiotic, antibioticPriorityListComparer))
            {
                var epsilometerTestViewModel = new EpsilometerTestViewModel
                {
                    EucastClinicalBreakpointId = epsilometerTest.EucastClinicalBreakpointId,
                    Measurement = epsilometerTest.Measurement,
                    Result = epsilometerTest.Result,
                    ReadonlyAntibiotic = true
                };

                var eucastClinicalBreakpoint = epsilometerTest.EucastClinicalBreakpoint;
                if (eucastClinicalBreakpoint != null)
                {
                    epsilometerTestViewModel.MicBreakpointResistent =
                        eucastClinicalBreakpoint.MicBreakpointResistent;
                    epsilometerTestViewModel.MicBreakpointSusceptible =
                        eucastClinicalBreakpoint.MicBreakpointSusceptible;
                    if (eucastClinicalBreakpoint.ValidFrom.HasValue)
                    {
                        epsilometerTestViewModel.ValidFromYear = eucastClinicalBreakpoint.ValidFrom.Value.Year;
                    }

                    epsilometerTestViewModel.Antibiotic = eucastClinicalBreakpoint.Antibiotic;
                    missingPrimaryAntibiotics.Remove(eucastClinicalBreakpoint.Antibiotic);
                }

                epsilometerTestViewModels.Add(epsilometerTestViewModel);
            }

            epsilometerTestViewModels.AddRange(
                missingPrimaryAntibiotics.Select(
                    missingPrimaryAntibiotic => new EpsilometerTestViewModel
                    {
                        Antibiotic = missingPrimaryAntibiotic,
                        ReadonlyAntibiotic = true
                    }
                ));

            epsilometerTestViewModels.OrderBy(e => e.Antibiotic.Value, antibioticPriorityListComparer);

            return epsilometerTestViewModels;
        }

        protected ICollection<EpsilometerTest> EpsilometerTestsViewModelToModel(ICollection<EpsilometerTestViewModel> epsilometerTestViewModels)
        {
            var epsilometerTests = new List<EpsilometerTest>();
            foreach (var eTestViewModel in epsilometerTestViewModels)
            {
                if (HasValidETestValue(eTestViewModel))
                {
                    var eTest = new EpsilometerTest
                    {
                        EucastClinicalBreakpointId = eTestViewModel.EucastClinicalBreakpointId.Value,
                        Measurement = eTestViewModel.Measurement.Value,
                        Result = eTestViewModel.Result.Value
                    };
                    epsilometerTests.Add(eTest);
                }
            }
            return epsilometerTests;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private static bool HasValidETestValue(EpsilometerTestViewModel eTestViewModel)
        {
            return eTestViewModel.Antibiotic.HasValue && eTestViewModel.Measurement.HasValue;
        }

        private List<Antibiotic> AvailableAntibiotics
        {
            get
            {
                return db.EucastClinicalBreakpoints.Select(e => e.Antibiotic)
                    .Distinct()
                    .ToList();
            }
        }

    }
}