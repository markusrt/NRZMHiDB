using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using HaemophilusWeb.Models;
using HaemophilusWeb.Utils;
using HaemophilusWeb.ViewModels;

namespace HaemophilusWeb.Automapper
{
    public class IsolateViewModelMappingActionBase
    {
        private readonly DatabaseType databaseType;

        //TODO Ugly quick fix: Make this use dependency injection
        public static IApplicationDbContext DbForTest;

        protected IApplicationDbContext db = DbForTest ?? new ApplicationDbContextWrapper(new ApplicationDbContext());

        //TODO Refactor copied code from IsolateControllerBase
        protected List<Antibiotic> AvailableAntibiotics
        {
            get
            {
                return db.EucastClinicalBreakpoints.Where(e => e.ValidFor == databaseType).Select(e => e.Antibiotic)
                    .Distinct()
                    .ToList();
            }
        }

        private readonly List<Antibiotic> primaryAntibiotics;

        private readonly AntibioticPriorityListComparer antibioticPriorityListComparer;

        public IsolateViewModelMappingActionBase(DatabaseType databaseType)
        {
            this.databaseType = databaseType;
            primaryAntibiotics = EnumUtils.ParseCommaSeperatedListOfNames<Antibiotic>(ConfigurationManager.AppSettings["PrimaryAntibiotics"]);
            antibioticPriorityListComparer = new AntibioticPriorityListComparer(ConfigurationManager.AppSettings["AntibioticsOrder"]);
        }

        protected ICollection<EpsilometerTestViewModel> EpsilometerTestsModelToViewModel(IEnumerable<EpsilometerTest> epsilometerTests)
        {
            var epsilometerTestViewModels = new List<EpsilometerTestViewModel>();
            var availableAntibiotics = AvailableAntibiotics;
            var missingPrimaryAntibiotics = new List<Antibiotic>(primaryAntibiotics.Where(a => availableAntibiotics.Contains(a)));

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

        private static bool HasValidETestValue(EpsilometerTestViewModel eTestViewModel)
        {
            return eTestViewModel.Antibiotic.HasValue && eTestViewModel.Measurement.HasValue;
        }

        internal static void ParseAndMapLaboratoryNumber(IsolateCommon source, IsolateCommon destination)
        {
            var decadeAndNumber = source.LaboratoryNumber.Split('/');
            int.TryParse(decadeAndNumber[0], out var yearlySequentialIsolateNumber);
            int.TryParse(decadeAndNumber[1], out var decade);
            destination.Year = DateTime.Now.Year / 100 * 100 + decade;
            destination.YearlySequentialIsolateNumber = yearlySequentialIsolateNumber;
        }

    }
}