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
    public class MeningoIsolateViewModelValidatorTests : AbstractValidatorTests<MeningoIsolateViewModelValidator, MeningoIsolateViewModel>
    {
        protected static IEnumerable<Tuple<MeningoIsolateViewModel, string[]>> InvalidModels;

        static MeningoIsolateViewModelValidatorTests()
        {
            InvalidModels = CreateInvalidModels();
        }

        [Test]
        public void Validate_WithNoGrowthButOnlySerogroup_IsValid()
        {
            var dto = new MeningoIsolateViewModel
            {
                GrowthOnBloodAgar = Growth.No,
                GrowthOnMartinLewisAgar = Growth.No,
                LaboratoryNumber = "10/10",
                SerogroupPcr = MeningoSerogroupPcr.A
            };

            var validationResult = Validate(dto);

           AssertIsValid(validationResult);
        }

        [Test]
        public void Validate_WithOxidaseNotSet_IsInvalid()
        {
            var dto = CreateValidModel();
            dto.Oxidase = TestResult.NotDetermined;

            var validationResult = Validate(dto);

            AssertIsInvalid(validationResult);
        }

        [Test]
        public void Validate_WithOxidaseNotSetButMaldiBiotyperPerformed_IsValid()
        {
            var dto = CreateValidModel();
            dto.Oxidase = TestResult.NotDetermined;
            dto.MaldiTofBiotyper = UnspecificTestResult.Determined;
            dto.MaldiTofBiotyperBestMatch = "N. meningitidis";
            dto.MaldiTofBiotyperMatchConfidence = 2.3;

            var validationResult = Validate(dto);

            AssertIsValid(validationResult);
        }

        [TestCase(Growth.ATypicalGrowth, Growth.No, TestResult.Negative)]
        [TestCase(Growth.TypicalGrowth, Growth.No, TestResult.Positive)]
        [TestCase(Growth.No, Growth.ATypicalGrowth, TestResult.Negative)]
        [TestCase(Growth.No, Growth.TypicalGrowth, TestResult.Positive)]
        [TestCase(Growth.ATypicalGrowth, Growth.TypicalGrowth, TestResult.Positive)]
        [TestCase(Growth.TypicalGrowth, Growth.ATypicalGrowth, TestResult.Negative)]
        public void Validate_WithOxidaseAndGrowthSet_IsValid(Growth growthGrowthOnBloodAgar, Growth growthOnMartinLewisAgar, TestResult oxidaseTestResult)
        {
            var dto = CreateValidModel();
            dto.GrowthOnBloodAgar = growthGrowthOnBloodAgar;
            dto.GrowthOnMartinLewisAgar = growthOnMartinLewisAgar;
            dto.Oxidase = oxidaseTestResult;

            var validationResult = Validate(dto);

            AssertIsValid(validationResult);
        }

        [TestCase(NativeMaterialTestResult.Negative)]
        [TestCase(NativeMaterialTestResult.Inhibitory)]
        [TestCase(NativeMaterialTestResult.NotDetermined)]
        public void Validate_RibosomalRna16SNotPositive_IsValid(NativeMaterialTestResult ribosomalRna16S)
        {
            var dto = CreateMeningoIsolateViewModel();
            dto.RibosomalRna16S = ribosomalRna16S;

            var validationResult = Validate(dto);

            AssertIsValid(validationResult);
        }

        protected override MeningoIsolateViewModel CreateValidModel()
        {
            return CreateMeningoIsolateViewModel();
        }

        // specs/005 FR-008: laboratory numbers in the '39/14' form are accepted
        [TestCase("39/14")]
        [TestCase("0000123/15")]
        public void Validate_WellFormedLaboratoryNumber_IsValid(string laboratoryNumber)
        {
            var model = CreateMeningoIsolateViewModel();
            model.LaboratoryNumber = laboratoryNumber;

            AssertIsValid(Validate(model));
        }

        // specs/005 FR-008: empty and malformed laboratory numbers fail validation
        [TestCase("")]
        [TestCase("abc")]
        [TestCase("12/3")]
        [TestCase("12-34")]
        public void Validate_MalformedLaboratoryNumber_FailsOnLaboratoryNumber(string laboratoryNumber)
        {
            var model = CreateMeningoIsolateViewModel();
            model.LaboratoryNumber = laboratoryNumber;

            Validate(model).Errors.Select(e => e.PropertyName).Should().Contain("LaboratoryNumber");
        }

        // specs/005 FR-013: the RIDOM evaluation of a positive Real-Time PCR may name several species
        [Test]
        public void Validate_SeveralRealTimePcrResults_IsValid()
        {
            var model = CreateMeningoIsolateViewModel();
            model.RealTimePcr = NativeMaterialTestResult.Positive;
            model.RealTimePcrResult = RealTimePcrResult.NeisseriaMeningitidis | RealTimePcrResult.StreptococcusPneumoniae;

            AssertIsValid(Validate(model));
        }

        // specs/005 Edge Cases (conditionally displayed molecular fields): PorA VR, FetA VR and the cswy allele
        // are shown for a positive PCR but not required
        [Test]
        public void Validate_PositivePorAFetAAndCswyPcrWithoutDetails_IsValid()
        {
            var model = CreateMeningoIsolateViewModel();
            model.CsbPcr = NativeMaterialTestResult.Negative;
            model.CswyPcr = NativeMaterialTestResult.Positive;
            model.PorAPcr = NativeMaterialTestResult.Positive;
            model.FetAPcr = NativeMaterialTestResult.Positive;

            AssertIsValid(Validate(model));
        }

        [Test]
        public void Validate_WithNoGrowthButSiaACtrAAndCnl_IsValid()
        {
            var dto = new MeningoIsolateViewModel
            {
                GrowthOnBloodAgar = Growth.No,
                GrowthOnMartinLewisAgar = Growth.No,
                LaboratoryNumber = "10/10",
                SiaAGene = TestResult.Negative,
                CapsularTransferGene = TestResult.Positive,
                CapsuleNullLocus = TestResult.Negative
            };

            var validationResult = Validate(dto);

            AssertIsValid(validationResult);
        }

        private static MeningoIsolateViewModel CreateMeningoIsolateViewModel()
        {
            return new MeningoIsolateViewModel
            {
                GrowthOnBloodAgar = Growth.ATypicalGrowth,
                GrowthOnMartinLewisAgar = Growth.No,
                LaboratoryNumber = "120/21",
                CsbPcr = NativeMaterialTestResult.Positive,
                CswyPcr = NativeMaterialTestResult.Negative,
                CscPcr = NativeMaterialTestResult.Inhibitory,
                Oxidase = TestResult.Positive
            };
        }

        protected static IEnumerable<Tuple<MeningoIsolateViewModel, string[]>> CreateInvalidModels()
        {
            var growthIsNoButOtherFieldsAreFilled  = new MeningoIsolateViewModel
            {
                GrowthOnBloodAgar = Growth.No,
                GrowthOnMartinLewisAgar = Growth.No,
                GammaGt = TestResult.Positive,
                LaboratoryNumber = "10/10"
            };
            yield return Tuple.Create(growthIsNoButOtherFieldsAreFilled, new[] { "GrowthOnBloodAgar", "GrowthOnMartinLewisAgar" });

            var growthIsNoButMaldiBiotyperDetermined  = new MeningoIsolateViewModel
            {
                GrowthOnBloodAgar = Growth.No,
                GrowthOnMartinLewisAgar = Growth.No,
                MaldiTofBiotyper = UnspecificTestResult.Determined,
                MaldiTofBiotyperBestMatch = "match",
                MaldiTofBiotyperMatchConfidence = 2.3,
                LaboratoryNumber = "10/10"
            };
            yield return Tuple.Create(growthIsNoButMaldiBiotyperDetermined, new[] { "GrowthOnBloodAgar", "GrowthOnMartinLewisAgar" });


            var twoSeroGenoGroupsPositive = CreateMeningoIsolateViewModel();
            twoSeroGenoGroupsPositive.CsbPcr = NativeMaterialTestResult.Positive;
            twoSeroGenoGroupsPositive.CswyPcr = NativeMaterialTestResult.Positive;
            yield return Tuple.Create(twoSeroGenoGroupsPositive, new[] { "CsbPcr", "CswyPcr" });

            var realTimePcrResultMissing = CreateMeningoIsolateViewModel();
            realTimePcrResultMissing.RealTimePcr = NativeMaterialTestResult.Positive;
            realTimePcrResultMissing.RealTimePcrResult = 0;
            yield return Tuple.Create(realTimePcrResultMissing, new[] { "RealTimePcrResult" });

            // specs/005 FR-010: with no growth at all, oxidase must stay "n.d." as well
            var growthIsNoButOxidaseSet = new MeningoIsolateViewModel
            {
                GrowthOnBloodAgar = Growth.No,
                GrowthOnMartinLewisAgar = Growth.No,
                Oxidase = TestResult.Negative,
                LaboratoryNumber = "10/10"
            };
            yield return Tuple.Create(growthIsNoButOxidaseSet, new[] { "GrowthOnBloodAgar", "GrowthOnMartinLewisAgar" });

            // specs/005 FR-013: a positive 16S rDNA needs best match and percentage
            var noRibosomalRna16SDetails = CreateMeningoIsolateViewModel();
            noRibosomalRna16SDetails.RibosomalRna16S = NativeMaterialTestResult.Positive;
            yield return Tuple.Create(noRibosomalRna16SDetails, new[] { "RibosomalRna16SBestMatch", "RibosomalRna16SMatchInPercent" });

            // specs/005 FR-013: a determined MALDI-TOF (Biotyper) needs best match and confidence
            var noMaldiTofBiotyperDetails = CreateMeningoIsolateViewModel();
            noMaldiTofBiotyperDetails.MaldiTofBiotyper = UnspecificTestResult.Determined;
            yield return Tuple.Create(noMaldiTofBiotyperDetails, new[] { "MaldiTofBiotyperBestMatch", "MaldiTofBiotyperMatchConfidence" });

            // specs/005 FR-013: the same rule holds for the stored MALDI-TOF (VITEK MS) values
            var noMaldiTofVitekDetails = CreateMeningoIsolateViewModel();
            noMaldiTofVitekDetails.MaldiTofVitek = UnspecificTestResult.Determined;
            yield return Tuple.Create(noMaldiTofVitekDetails, new[] { "MaldiTofVitekBestMatch", "MaldiTofVitekMatchConfidence" });
        }
    }
}