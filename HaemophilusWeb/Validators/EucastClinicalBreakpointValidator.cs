using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using HaemophilusWeb.Models;

namespace HaemophilusWeb.Validators
{
    public class EucastClinicalBreakpointValidator : AbstractValidator<EucastClinicalBreakpoint>
    {
        private const string EucastPropertiesEitherEmptyOrAllSet =
            "Die Felder 'Version', 'Gültig ab', 'Sensibel <=' und 'Resistent >' müssen entweder alle gefüllt oder alle leer sein.";

        public EucastClinicalBreakpointValidator()
        {
            RuleFor(e => e.AntibioticDetails).NotEmpty();

            RuleFor(e => e.Version).Must(EucastPropertiesAllBeNullOrAllBeNotNull).WithMessage(EucastPropertiesEitherEmptyOrAllSet);
            RuleFor(e => e.ValidFrom).Must((model, value)
                => EucastPropertiesAllBeNullOrAllBeNotNull(model, value)).WithMessage(EucastPropertiesEitherEmptyOrAllSet);
            RuleFor(e => e.MicBreakpointResistent).Must((model, value)
                => EucastPropertiesAllBeNullOrAllBeNotNull(model, value)).WithMessage(EucastPropertiesEitherEmptyOrAllSet);
            RuleFor(e => e.MicBreakpointSusceptible).Must((model, value)
                => EucastPropertiesAllBeNullOrAllBeNotNull(model, value)).WithMessage(EucastPropertiesEitherEmptyOrAllSet);
        }

        private static bool EucastPropertiesAllBeNullOrAllBeNotNull(EucastClinicalBreakpoint model, object value)
        {
            var values = new List<object>
            {
                value,
                model.MicBreakpointResistent,
                model.MicBreakpointSusceptible,
                model.ValidFrom,
                model.Version
            };
            return values.All(v => v == null) || values.All(v => v != null);
        }
    }
}