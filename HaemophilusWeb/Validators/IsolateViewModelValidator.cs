using FluentValidation;
using HaemophilusWeb.Models;
using HaemophilusWeb.ViewModels;

namespace HaemophilusWeb.Validators
{
    public class IsolateViewModelValidator : AbstractValidator<IsolateViewModel>
    {
        private const string PropertyMustNotBeEmpty = "{PropertyName} darf nicht leer sein.";

        public IsolateViewModelValidator()
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
            RuleFor(i => i.MlstSequenceType).Must((model, value)
                => BeSetIfDetermined(value, model.Mlst)).WithMessage(PropertyMustNotBeEmpty);
            RuleFor(p => p.LaboratoryNumber).NotEmpty();
            RuleFor(p => p.TypeOfGrowth).Must(
                (model, value) => model.Growth != YesNoOptional.Yes || value > 0).WithMessage(
                    "Die Art des Wachstums muss angegeben werden.");
            RuleFor(p => p.LaboratoryNumber).Matches(@"\d+/\d\d").WithMessage(
                "Die Labornummer muss in der Form '39/14' eingegeben werden.");
        }

        private static bool BeSetIfDetermined(double? value, UnspecificTestResult testResult)
        {
            return testResult == UnspecificTestResult.NotDetermined || value.HasValue;
        }

        private static bool BeSetIfDetermined(string value, UnspecificTestResult testResult)
        {
            return testResult == UnspecificTestResult.NotDetermined || !string.IsNullOrWhiteSpace(value);
        }

        private static bool BeSetIfDetermined(string value, UnspecificOrNoTestResult testResult)
        {
            return testResult != UnspecificOrNoTestResult.Determined || !string.IsNullOrWhiteSpace(value);
        }
    }
}