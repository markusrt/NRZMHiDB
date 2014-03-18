using FluentValidation;
using HaemophilusWeb.Models;

namespace HaemophilusWeb.Validators
{
    public class PatientValidator : AbstractValidator<Patient>
    {
        public PatientValidator()
        {
            RuleFor(p => p.Initials).NotEmpty();
            RuleFor(p => p.Initials).Matches(@"([a-zA-ZäöüÄÖÜ]\.)+").WithMessage(
                "Die Initialen müssen in der Form 'E.M.' eingegeben werden.");
            RuleFor(p => p.PostalCode).Matches(Validations.PostalCodeValidation).WithMessage(
                Validations.PostalCodeValidationError);
            RuleFor(p => p.Gender).NotEmpty();
        }
    }
}