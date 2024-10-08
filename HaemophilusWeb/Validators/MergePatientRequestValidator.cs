﻿using FluentValidation;
using HaemophilusWeb.Models;
using HaemophilusWeb.ViewModels;

namespace HaemophilusWeb.Validators
{
    public class MergePatientRequestValidator : AbstractValidator<MergePatientRequest>
    {
        public MergePatientRequestValidator()
        {
            RuleFor(m => m.PatientOneId).NotEqual(m => m.PatientTwoId)
                .WithMessage("Bitte geben Sie zum Zusammenfügen zwei verschiedenen Patientennummern ein");
        }
    }
}