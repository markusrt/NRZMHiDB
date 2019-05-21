﻿using FluentValidation;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.ViewModels;

namespace HaemophilusWeb.Validators
{
    public class MeningoIsolateViewModelValidator : IsolateViewModelValidatorBase<MeningoIsolateViewModel>
    {
        private const string OtherPropertiesShouldBeNotDeterminedOnNoGrowth =
            "Wenn {PropertyName} den Wert \"Nein\" hat, dann müssen alle anderen Einträge unter \"Allgemein\" auf \"n.d.\" stehen.";
        private const string MoreThenOneSerogenoIsPositive = "Es darf nur ein Serogenotyp positiv sein.";

        public MeningoIsolateViewModelValidator()
        {
            RuleFor(i => i.EpsilometerTestViewModels).SetCollectionValidator(new EpsilometerTestValidator());
            
            RuleFor(i => i.RibosomalRna16SBestMatch).Must((model, value)
                => BeSetIfDetermined(value, model.RibosomalRna16S)).WithMessage(PropertyMustNotBeEmpty);
            RuleFor(i => i.RibosomalRna16SMatchInPercent).Must((model, value)
                => BeSetIfDetermined(value, model.RibosomalRna16S)).WithMessage(PropertyMustNotBeEmpty);
            RuleFor(i => i.ApiNhBestMatch).Must((model, value)
                => BeSetIfDetermined(value, model.ApiNh)).WithMessage(PropertyMustNotBeEmpty);
            RuleFor(i => i.ApiNhMatchInPercent).Must((model, value)
                => BeSetIfDetermined(value, model.ApiNh)).WithMessage(PropertyMustNotBeEmpty);
            RuleFor(i => i.MaldiTofBestMatch).Must((model, value)
                => BeSetIfDetermined(value, model.MaldiTof)).WithMessage(PropertyMustNotBeEmpty);
            RuleFor(i => i.MaldiTofMatchConfidence).Must((model, value)
                => BeSetIfDetermined(value, model.MaldiTof)).WithMessage(PropertyMustNotBeEmpty);
            RuleFor(i => i.LaboratoryNumber).NotEmpty();
            RuleFor(i => i.LaboratoryNumber).Matches(@"\d+/\d\d").WithMessage(
                "Die Labornummer muss in der Form '39/14' eingegeben werden.");
            RuleFor(i => i.GrowthOnBloodAgar).Must(OtherTypingFieldsAreEmptyIfNoGrowth)
                .WithMessage(OtherPropertiesShouldBeNotDeterminedOnNoGrowth);
            RuleFor(i => i.GrowthOnMartinLewisAgar).Must(OtherTypingFieldsAreEmptyIfNoGrowth)
                .WithMessage(OtherPropertiesShouldBeNotDeterminedOnNoGrowth);
            RuleFor(i => i.CsbPcr).Must(OnlyOneSerogenoGroupShouldBePositive).WithMessage(MoreThenOneSerogenoIsPositive);
            RuleFor(i => i.CscPcr).Must(OnlyOneSerogenoGroupShouldBePositive).WithMessage(MoreThenOneSerogenoIsPositive);
            RuleFor(i => i.CswyPcr).Must(OnlyOneSerogenoGroupShouldBePositive).WithMessage(MoreThenOneSerogenoIsPositive);
        }

        private static bool OnlyOneSerogenoGroupShouldBePositive(MeningoIsolateViewModel model, NativeMaterialTestResult testResult)
        {
            var positives = 0;
            positives += model.CsbPcr == NativeMaterialTestResult.Positive ? 1 : 0;
            positives += model.CscPcr == NativeMaterialTestResult.Positive ? 1 : 0;
            positives += model.CswyPcr == NativeMaterialTestResult.Positive ? 1 : 0;
            var modelIsInvalid = testResult == NativeMaterialTestResult.Positive && positives > 1;
            return !modelIsInvalid;
        }

        private static bool OtherTypingFieldsAreEmptyIfNoGrowth(MeningoIsolateViewModel model, Growth growth)
        {
            var modelIsInvalid =
                model.GrowthOnBloodAgar == Growth.No && model.GrowthOnMartinLewisAgar == Growth.No
                 && (model.Oxidase != TestResult.NotDetermined || model.Agglutination != MeningoSerogroupAgg.NotDetermined
                 || model.Onpg != TestResult.NotDetermined || model.GammaGt != TestResult.NotDetermined
                 || model.SerogroupPcr != MeningoSerogroupPcr.NotDetermined || model.ApiNh != UnspecificTestResult.NotDetermined 
                 || model.MaldiTof != UnspecificTestResult.NotDetermined);
            return !modelIsInvalid;
        }

        private static bool BeSetIfDetermined(double? value, NativeMaterialTestResult testResult)
        {
            return testResult == NativeMaterialTestResult.NotDetermined || value.HasValue;
        }

        private static bool BeSetIfDetermined(string value, NativeMaterialTestResult testResult)
        {
            return testResult == NativeMaterialTestResult.NotDetermined || !string.IsNullOrWhiteSpace(value);
        }
    }
}