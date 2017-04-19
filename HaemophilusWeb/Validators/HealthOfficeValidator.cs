using FluentValidation;
using HaemophilusWeb.Models;

namespace HaemophilusWeb.Validators
{
    public class HealthOfficeValidator : AbstractValidator<HealthOffice>
    {
        public HealthOfficeValidator()
        {
            RuleFor(p => p.PostalCode).NotEmpty();
        }
    }
}