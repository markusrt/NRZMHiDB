using FluentValidation;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Utils;

namespace HaemophilusWeb.Validators
{
    public class IsolateViewModelValidatorBase<TViewModel> : AbstractValidator<TViewModel>
    {
        protected const string PropertyMustNotBeEmpty = "{PropertyName} darf nicht leer sein.";

        protected static bool BeSetIfDetermined(double? value, UnspecificTestResult testResult)
        {
            return testResult == UnspecificTestResult.NotDetermined || value.HasValue;
        }

        protected static bool BeSetIfDetermined(string value, UnspecificTestResult testResult)
        {
            return testResult == UnspecificTestResult.NotDetermined || !string.IsNullOrWhiteSpace(value);
        }

        protected static bool BeSetIfDetermined(string value, UnspecificOrNoTestResult testResult)
        {
            return testResult != UnspecificOrNoTestResult.Determined || !string.IsNullOrWhiteSpace(value);
        }

        protected static bool BeSetIfPositive(double? value, NativeMaterialTestResult testResult)
        {
            return testResult != NativeMaterialTestResult.Positive || value.HasValue;
        }

        protected static bool BeSetIfPositive(RealTimePcrResult result, NativeMaterialTestResult testResult)
        {
            return testResult != NativeMaterialTestResult.Positive || (result != 0 && result.IsDefinedEnumValue());
        }

        protected static bool BeSetIfPositive(string value, NativeMaterialTestResult testResult)
        {
            return testResult != NativeMaterialTestResult.Positive || !string.IsNullOrWhiteSpace(value);
        }
    }
}