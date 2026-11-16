using System;
using System.Collections.Generic;
using System.Linq;
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

        // specs/004 FR-010: laboratory numbers in the '39/14' form are accepted
        [TestCase("39/14")]
        [TestCase("0000123/15")]
        public void Validate_WellFormedLaboratoryNumber_IsValid(string laboratoryNumber)
        {
            var model = CreateIsolateViewModel();
            model.LaboratoryNumber = laboratoryNumber;

            AssertIsValid(Validate(model));
        }

        // specs/004 FR-010: empty and malformed laboratory numbers fail validation
        [TestCase("")]
        [TestCase("abc")]
        [TestCase("12/3")]
        [TestCase("12-34")]
        public void Validate_MalformedLaboratoryNumber_FailsOnLaboratoryNumber(string laboratoryNumber)
        {
            var model = CreateIsolateViewModel();
            model.LaboratoryNumber = laboratoryNumber;

            Validate(model).Errors.Select(e => e.PropertyName).Should().Contain("LaboratoryNumber");
        }

        // specs/004 FR-004: a type of growth is required only when growth is "Ja"
        [TestCase(YesNoOptional.No)]
        [TestCase(YesNoOptional.NotStated)]
        public void Validate_GrowthOtherThanYes_RequiresNoTypeOfGrowth(YesNoOptional growth)
        {
            var model = CreateIsolateViewModel();
            model.Growth = growth;
            model.TypeOfGrowth = GrowthType.None;

            AssertIsValid(Validate(model));
        }

        // specs/004 FR-011: details are required only for "positiv" and "durchgeführt"
        [Test]
        public void Validate_OtherTestResults_RequireNoDetails()
        {
            var model = CreateIsolateViewModel();
            model.RibosomalRna16S = NativeMaterialTestResult.Inhibitory;
            model.RealTimePcr = NativeMaterialTestResult.Negative;
            model.Mlst = UnspecificOrNoTestResult.NoResult;

            AssertIsValid(Validate(model));
        }

        // specs/004 FR-011: the RIDOM evaluation of a positive Real-Time PCR may name several species
        [Test]
        public void Validate_SeveralRealTimePcrResults_IsValid()
        {
            var model = CreateIsolateViewModel();
            model.RealTimePcr = NativeMaterialTestResult.Positive;
            model.RealTimePcrResult = RealTimePcrResult.HaemophilusInfluenzae | RealTimePcrResult.StreptococcusPneumoniae;

            AssertIsValid(Validate(model));
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
            noMaldiTofDetails.MaldiTofVitek = UnspecificTestResult.Determined;
            yield return Tuple.Create(noMaldiTofDetails, new[] { "MaldiTofVitekBestMatch", "MaldiTofVitekMatchConfidence" });

            // specs/004 FR-011: MALDI-TOF (Biotyper) details are required when it is "durchgeführt"
            var noMaldiTofBiotyperDetails = CreateIsolateViewModel();
            noMaldiTofBiotyperDetails.MaldiTofBiotyper = UnspecificTestResult.Determined;
            yield return Tuple.Create(noMaldiTofBiotyperDetails, new[] { "MaldiTofBiotyperBestMatch", "MaldiTofBiotyperMatchConfidence" });

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