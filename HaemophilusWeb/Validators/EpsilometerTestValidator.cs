using System.Collections.Generic;
using FluentValidation;
using HaemophilusWeb.Models;
using HaemophilusWeb.ViewModels;

namespace HaemophilusWeb.Validators
{
    public class EpsilometerTestValidator : AbstractValidator<EpsilometerTestViewModel>
    {
        public static readonly List<float> AmpicillinAndAmoxicillinClavulanateETestScale = new List<float> { 256, 192, 128, 96, 64, 48, 32, 24, 16, 12, 8, 6, 4, 3, 2, 1.5f, 1.0f, .75f, .50f, .38f, .25f, .19f, .125f, .094f, .064f, .047f, .032f, .023f, .016f };
        public static readonly List<float> OtherAntibioticsETestScale = new List<float> { 32, 24, 16, 12, 8, 6, 4, 3, 2, 1.5f, 1.0f, .75f, .50f, .38f, .25f, .19f, .125f, .094f, .064f, .047f, .032f, .023f, .016f, .012f, .008f, .006f, .004f, .003f, .002f };

        private const string MeasurementMustBeGreaterThanZero = "'{PropertyName}' muss ein Wert von der E-Test Skala sein.";
        private const string EucastClinicalBreakpointIdIsRequired = "'{PropertyName}' darf nicht leer sein";

        public EpsilometerTestValidator()
        {
            RuleFor(e => e.Measurement).Must(BeValidMeasurement).WithMessage(MeasurementMustBeGreaterThanZero);
            RuleFor(e => e.EucastClinicalBreakpointId).Must(BeSetIfMeasurementHasValue).WithMessage(EucastClinicalBreakpointIdIsRequired);
            RuleFor(e => e.Antibiotic).Must(BeSetIfMeasurementHasValue).WithMessage(EucastClinicalBreakpointIdIsRequired);
        }

        public static List<float> GetETestScale(Antibiotic? antibiotic)
        {
            return antibiotic.HasValue && (antibiotic.Value == Antibiotic.Ampicillin || antibiotic.Value == Antibiotic.AmoxicillinClavulanate)
                ? AmpicillinAndAmoxicillinClavulanateETestScale
                : OtherAntibioticsETestScale;
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
            bool measurementIsValid = false;
            if (eTest.Antibiotic.HasValue && measurement.HasValue)
            {
                measurementIsValid = GetETestScale(eTest.Antibiotic.Value).Contains(measurement.Value);
            }
            return !measurement.HasValue || measurementIsValid;
        }
    }
}