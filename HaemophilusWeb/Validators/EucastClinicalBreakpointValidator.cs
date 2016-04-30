using FluentValidation;
using HaemophilusWeb.Models;

namespace HaemophilusWeb.Validators
{
    public class EucastClinicalBreakpointValidator : AbstractValidator<EucastClinicalBreakpoint>
    {
        public EucastClinicalBreakpointValidator()
        {
            RuleFor(e => e.AntibioticDetails).NotEmpty();
        }
    }
}