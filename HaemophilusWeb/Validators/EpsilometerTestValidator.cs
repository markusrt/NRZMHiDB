using FluentValidation;
using HaemophilusWeb.Models;
using HaemophilusWeb.ViewModels;

namespace HaemophilusWeb.Validators
{
    public class EpsilometerTestValidator : AbstractValidator<EpsilometerTestViewModel>
    {
        private const string MeasurementMustBeGreaterThanZero = "Der Wert von '{PropertyName}' muss grösser sein als '0'.";
        private const string EucastClinicalBreakpointIdIsRequired = "'{PropertyName}' darf nicht leer sein";

        public EpsilometerTestValidator()
        {
            RuleFor(e => e.Measurement).Must(BeValidMeasurement).WithMessage(MeasurementMustBeGreaterThanZero);
            RuleFor(e => e.EucastClinicalBreakpointId).Must(BeSetIfMeasurementHasValue).WithMessage(EucastClinicalBreakpointIdIsRequired);
            RuleFor(e => e.Antibiotic).Must(BeSetIfMeasurementHasValue).WithMessage(EucastClinicalBreakpointIdIsRequired);
        }

        private static bool BeSetIfMeasurementHasValue(EpsilometerTestViewModel eTest, int? eucastClinicalBreakpointId)
        {
            return !eTest.Measurement.HasValue || eucastClinicalBreakpointId.HasValue;
        }

        private static bool BeSetIfMeasurementHasValue(EpsilometerTestViewModel eTest, Antibiotic? antibiotic)
        {
            return !eTest.Measurement.HasValue || antibiotic.HasValue;
        }

        private static bool BeValidMeasurement(EpsilometerTestViewModel eTest, float? measurement)
        {
            return !measurement.HasValue || measurement > 0;
        }
    }
}