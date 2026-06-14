using FluentValidation;
using HaemophilusWeb.ViewModels;

namespace HaemophilusWeb.Validators
{
    public class ConfigurationViewModelValidator : AbstractValidator<ConfigurationViewModel>
    {
        public ConfigurationViewModelValidator()
        {
            When(model => !string.IsNullOrWhiteSpace(model.AnnouncementText), () =>
            {
                RuleFor(model => model.AnnouncementStart)
                    .NotNull().WithMessage("Bitte ein Startdatum für die Mitteilung angeben.");
                RuleFor(model => model.AnnouncementEnd)
                    .NotNull().WithMessage("Bitte ein Enddatum für die Mitteilung angeben.");
            });

            RuleFor(model => model.AnnouncementEnd)
                .Must((model, end) => !model.AnnouncementStart.HasValue || !end.HasValue
                                      || end.Value.Date >= model.AnnouncementStart.Value.Date)
                .WithMessage("Das Enddatum darf nicht vor dem Startdatum liegen.");
        }
    }
}
