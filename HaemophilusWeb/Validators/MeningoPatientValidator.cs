using FluentValidation;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;

namespace HaemophilusWeb.Validators
{
    public class MeningoPatientValidator : AbstractValidator<MeningoPatient>
    {
        public MeningoPatientValidator()
        {
            RuleFor(p => p.Initials).NotEmpty();
            RuleFor(p => p.Initials).Matches(@"([a-zA-ZäöüÄÖÜ\?]\.)+").WithMessage(
                "Die Initialen müssen in der Form 'E.M.' eingegeben werden.");
            RuleFor(p => p.Gender).NotEmpty();
            RuleFor(p => p.PostalCode).Must(BeNotEmptyIfNotOverseas).WithMessage("{PropertyName} darf nicht leer sein.");
        }

        private static bool BeNotEmptyIfNotOverseas(MeningoPatient patient, string postalcode)
        {
            return patient.State == State.Overseas || !string.IsNullOrEmpty(postalcode);
        }
    }
}