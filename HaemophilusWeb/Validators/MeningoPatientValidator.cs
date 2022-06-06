using System;
using FluentValidation;
using HaemophilusWeb.Controllers;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;

namespace HaemophilusWeb.Validators
{
    public class MeningoPatientValidator : AbstractValidator<MeningoPatient>
    {
        public MeningoPatientValidator()
        {
            RuleFor(p => p.Initials).NotEmpty();
            RuleFor(p => p.BirthDate).GreaterThan(new DateTime(1800, 1, 1));
            RuleFor(p => p.Initials).Matches(@"([a-zA-ZäöüÄÖÜ\?]\.)+").WithMessage(
                "Die Initialen müssen in der Form 'E.M.' eingegeben werden.");
            RuleFor(p => p.Gender).NotEmpty();
            RuleFor(p => p.Country).Must(BeNotDefaultIfOverseas)
                .WithMessage("{PropertyName} darf nicht auf Deutschland stehen, wenn das Bundesland den Wert \"Ausland\" hat.");
            RuleFor(p => p.Country).Must(BeNotOverseasIfDefaultCountry)
                .WithMessage("{PropertyName} muss auf Deutschland stehen, wenn das Bundesland nicht den Wert \"Ausland\" hat.");
        }

        private static bool BeNotDefaultIfOverseas(MeningoPatient patient, string country)
        {
            return patient.State != State.Overseas || !GeonamesController.DefaultCountryIsoAlpha3.Equals(country);
        }

        private static bool BeNotOverseasIfDefaultCountry(MeningoPatient patient, string country)
        {
            return patient.State == State.Overseas || GeonamesController.DefaultCountryIsoAlpha3.Equals(country);
        }
    }
}