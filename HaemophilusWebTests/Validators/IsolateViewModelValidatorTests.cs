using System;
using System.Collections.Generic;
using FluentAssertions;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.ViewModels;
using NUnit.Framework;

namespace HaemophilusWeb.Validators
{
    [TestFixture]
    public class IsolateViewModelValidatorTests : AbstractValidatorTests<IsolateViewModelValidator, IsolateViewModel>
    {
        protected static IEnumerable<Tuple<IsolateViewModel, string[]>> InvalidModels;

        static IsolateViewModelValidatorTests()
        {
            InvalidModels = CreateInvalidModels();
        }

        protected override IsolateViewModel CreateValidModel()
        {
            return CreateIsolateViewModel();
        }

        private static IsolateViewModel CreateIsolateViewModel()
        {
            return new IsolateViewModel
            {
                LaboratoryNumber = "120/21",
                Growth = YesNoOptional.Yes,
                TypeOfGrowth = GrowthType.GrowthOnBlood,
                Mlst = UnspecificOrNoTestResult.NoResult,
            };
        }

        protected static IEnumerable<Tuple<IsolateViewModel, string[]>> CreateInvalidModels()
        {
            var growthIsYesButTypeOfGrowthIsNotFilled  = new IsolateViewModel
            {
                Growth = YesNoOptional.Yes
            };
            yield return Tuple.Create(growthIsYesButTypeOfGrowthIsNotFilled, new[] { "TypeOfGrowth", "LaboratoryNumber" });

            var noRibosomalRna16SDetails = CreateIsolateViewModel();
            noRibosomalRna16SDetails.RibosomalRna16S = NativeMaterialTestResult.Positive;
            yield return Tuple.Create(noRibosomalRna16SDetails, new[] { "RibosomalRna16SBestMatch", "RibosomalRna16SMatchInPercent" });

            var noMaldiTofDetails = CreateIsolateViewModel();
            noMaldiTofDetails.MaldiTof = UnspecificTestResult.Determined;
            yield return Tuple.Create(noMaldiTofDetails, new[] { "MaldiTofBestMatch", "MaldiTofMatchConfidence" });

            var noMlstDetails = CreateIsolateViewModel();
            noMlstDetails.Mlst = UnspecificOrNoTestResult.Determined;
            yield return Tuple.Create(noMlstDetails, new[] { "MlstSequenceType" });

            var invalidEpsilometerTests = CreateIsolateViewModel();
            invalidEpsilometerTests.EpsilometerTestViewModels = new List<EpsilometerTestViewModel>
            {
                new EpsilometerTestViewModel(Antibiotic.Amikacin) {Measurement = 123, EucastClinicalBreakpointId = 1}
            };
            yield return Tuple.Create(invalidEpsilometerTests, new[] { "EpsilometerTestViewModels[0].Measurement" });

            var realTimePcrResultMissing = CreateIsolateViewModel();
            realTimePcrResultMissing.RealTimePcr = NativeMaterialTestResult.Positive;
            realTimePcrResultMissing.RealTimePcrResult = 0;
            yield return Tuple.Create(realTimePcrResultMissing, new[] { "RealTimePcrResult" });
        }
    }
}