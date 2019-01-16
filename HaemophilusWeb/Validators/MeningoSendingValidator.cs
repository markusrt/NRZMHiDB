using System;
using FluentValidation;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;

namespace HaemophilusWeb.Validators
{
    public class MeningoSendingValidator : AbstractValidator<MeningoSending>
    {
        public MeningoSendingValidator()
        {
            RuleFor(sending => sending.SenderId).NotNull();
            RuleFor(sending => sending.MeningoPatientId).NotNull();
            RuleFor(sending => sending.ReceivingDate).NotNull();
            RuleFor(sending => sending.SenderLaboratoryNumber).NotEmpty();
            RuleFor(sending => sending.ReceivingDate).GreaterThanOrEqualTo(
                sample => sample.SamplingDate ?? DateTime.MinValue)
                .WithMessage("Das Eingangsdatum muss nach dem Entnahmedatum liegen");
            RuleFor(sending => sending.OtherInvasiveSamplingLocation).Must(BeNotEmptyIfSamplingLocationIsOtherInvasive).WithMessage(
                "{PropertyName} darf nicht leer sein.");
            RuleFor(sending => sending.OtherNonInvasiveSamplingLocation).Must(BeNotEmptyIfSamplingLocationIsOtherNonInvasive).WithMessage(
                "{PropertyName} darf nicht leer sein.");
        }

        private static bool BeNotEmptyIfSamplingLocationIsOtherInvasive(MeningoSending sending, string samplingLocation)
        {
            return sending.SamplingLocation != MeningoSamplingLocation.OtherInvasive || !string.IsNullOrEmpty(samplingLocation);
        }

        private static bool BeNotEmptyIfSamplingLocationIsOtherNonInvasive(MeningoSending sending, string samplingLocation)
        {
            return sending.SamplingLocation != MeningoSamplingLocation.OtherNonInvasive || !string.IsNullOrEmpty(samplingLocation);
        }
    }
}