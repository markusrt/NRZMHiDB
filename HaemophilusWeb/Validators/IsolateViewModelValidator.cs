using FluentValidation;
using HaemophilusWeb.Models;
using HaemophilusWeb.ViewModels;

namespace HaemophilusWeb.Validators
{
    public class IsolateViewModelValidator : IsolateViewModelValidatorBase<IsolateViewModel>
    {

        public IsolateViewModelValidator()
        {
            RuleFor(i => i.EpsilometerTestViewModels).SetCollectionValidator(new EpsilometerTestValidator());
            
            RuleFor(i => i.RibosomalRna16SBestMatch).Must((model, value)
                => BeSetIfPositive(value, model.RibosomalRna16S)).WithMessage(PropertyMustNotBeEmpty);
            RuleFor(i => i.RibosomalRna16SMatchInPercent).Must((model, value)
                => BeSetIfPositive(value, model.RibosomalRna16S)).WithMessage(PropertyMustNotBeEmpty);
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
    }
}