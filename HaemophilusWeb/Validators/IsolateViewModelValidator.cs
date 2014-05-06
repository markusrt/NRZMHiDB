using FluentValidation;
using HaemophilusWeb.Models;
using HaemophilusWeb.ViewModels;

namespace HaemophilusWeb.Validators
{
    public class IsolateViewModelValidator : AbstractValidator<IsolateViewModel>
    {
        private const string MeasurementMustBeGreaterThanZero = "Der Wert von '{PropertyName}' muss grösser sein als '0'.";
        private const string PropertyMustNotBeEmpty = "{PropertyName} darf nicht leer sein.";

        public IsolateViewModelValidator()
        {
            RuleFor(i => i.Ampicillin.Measurement).Must((model, value) 
                => BeValidMeasurement(model.Ampicillin, value)).WithMessage(MeasurementMustBeGreaterThanZero);
            RuleFor(i => i.AmoxicillinClavulanate.Measurement).Must((model, value)
               => BeValidMeasurement(model.AmoxicillinClavulanate, value)).WithMessage(MeasurementMustBeGreaterThanZero);
            RuleFor(i => i.Cefotaxime.Measurement).Must((model, value)
               => BeValidMeasurement(model.Cefotaxime, value)).WithMessage(MeasurementMustBeGreaterThanZero);
            RuleFor(i => i.Meropenem.Measurement).Must((model, value)
               => BeValidMeasurement(model.Meropenem, value)).WithMessage(MeasurementMustBeGreaterThanZero);
            RuleFor(i => i.RibosomalRna16SMatchInPercent).Must((model, value)
                => BeSetIfDetermined(value, model.RibosomalRna16S)).WithMessage(PropertyMustNotBeEmpty);
            RuleFor(i => i.ApiNhMatchInPercent).Must((model, value)
                => BeSetIfDetermined(value, model.ApiNh)).WithMessage(PropertyMustNotBeEmpty);
            RuleFor(i => i.MaldiTofMatchConfidence).Must((model, value)
                => BeSetIfDetermined(value, model.MaldiTof)).WithMessage(PropertyMustNotBeEmpty);
        }

        private static bool BeSetIfDetermined(double? value, UnspecificTestResult testResult)
        {
            return testResult == UnspecificTestResult.NotDetermined || value.HasValue;
        }

        private static bool BeValidMeasurement(EpsilometerTestViewModel eTest, float measurement)
        {
            if (!eTest.EucastClinicalBreakpointId.HasValue)
            {
                return true;
            }
            return measurement > 0;
        }
    }
}