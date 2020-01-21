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
                GrowthOnBloodAgar = Growth.No,
                GrowthOnMartinLewisAgar = Growth.TypicalGrowth,
                LaboratoryNumber = "120/21",
                CsbPcr = NativeMaterialTestResult.Positive,
                CswyPcr = NativeMaterialTestResult.Negative,
                CscPcr = NativeMaterialTestResult.Inhibitory,
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

            var twoSeroGenoGroupsPositive = CreateMeningoIsolateViewModel();
            twoSeroGenoGroupsPositive.CsbPcr = NativeMaterialTestResult.Positive;
            twoSeroGenoGroupsPositive.CswyPcr = NativeMaterialTestResult.Positive;
            yield return Tuple.Create(twoSeroGenoGroupsPositive, new[] { "CsbPcr", "CswyPcr" });
        }
    }
}