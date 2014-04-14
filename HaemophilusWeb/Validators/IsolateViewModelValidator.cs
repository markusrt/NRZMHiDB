using FluentValidation;
using HaemophilusWeb.ViewModels;

namespace HaemophilusWeb.Validators
{
    public class IsolateViewModelValidator : AbstractValidator<IsolateViewModel>
    {
        private const string MeasurementMustBeGreaterThanZero = "Der Wert von '{PropertyName}' muss grösser sein als '0'.";

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
        }

        private bool BeValidMeasurement(EpsilometerTestViewModel eTest, float measurement)
        {
            if (!eTest.EucastClinicalBreakpointId.HasValue)
            {
                return true;
            }
            return measurement > 0;
        }
    }
}