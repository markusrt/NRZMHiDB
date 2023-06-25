using System;
using FluentValidation;
using HaemophilusWeb.Models;

namespace HaemophilusWeb.Validators
{
    public class SendingValidator : AbstractValidator<Sending>
    {
        public SendingValidator()
        {
            RuleFor(sending => sending.SenderId).NotNull();
            RuleFor(sending => sending.PatientId).NotNull();
            RuleFor(sending => sending.ReceivingDate).NotNull();
            RuleFor(sending => sending.Invasive).NotEmpty();

            RuleFor(sending => sending.ReceivingDate).GreaterThanOrEqualTo(
                sample => sample.SamplingDate ?? DateTime.MinValue)
                .WithMessage("Das Eingangsdatum muss nach dem Entnahmedatum liegen");
            RuleFor(sending => sending.SenderLaboratoryNumber).NotEmpty();
            RuleFor(sending => sending.SenderConclusion).NotEmpty();
            RuleFor(sending => sending.OtherSamplingLocation).Must(BeNotEmptyIfSamplingLocationIsOther).WithMessage(
                "{PropertyName} darf nicht leer sein.");
        }

        private static bool BeNotEmptyIfSamplingLocationIsOther(Sending sending, string otherSamplingLocation)
        {
            return sending.SamplingLocation != SamplingLocation.OtherNonInvasive || !string.IsNullOrEmpty(otherSamplingLocation);  //TODO OtherInvasive
        }
    }
}