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
            RuleFor(p => p.Gender).NotEmpty();
            RuleFor(p => p.PostalCode).Must(BeNotEmptyIfNotOverseas)
                .WithMessage("{PropertyName} darf nicht leer sein.");

            //TODO implement Country -> MeningoPatientValidator
        }

        private static bool BeNotEmptyIfNotOverseas(Patient patient, string postalcode)
        {
            return patient.State == State.Overseas || !string.IsNullOrEmpty(postalcode);
        }
    }
}