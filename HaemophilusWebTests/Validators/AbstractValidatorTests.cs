using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NUnit.Framework;

namespace HaemophilusWeb.Validators
{
    [TestFixture]
    public abstract class AbstractValidatorTests<TValidator, TModel> where TValidator : AbstractValidator<TModel>, new()
    {
        /// <summary>
        ///     Has to return an instance of <typeparamref name="TModel" /> which is valid in the means of
        ///     <typeparamref
        ///         name="TValidator" />
        ///     .
        /// </summary>
        /// <returns>Must not return null</returns>
        protected abstract TModel CreateValidModel();

        /// <summary>
        ///     Has to return a sequence of <typeparamref name="TModel" />s which have all validated parameters set to a invalid
        ///     value.
        ///     For all those fields the <see cref="Validate_WithAMaximalInvalidModel_ReturnsAllInvalidFields" /> test uses this
        ///     information
        ///     to check that the validator reports all as invalid.
        /// </summary>
        /// <returns>
        ///     A list of <see cref="Tuple" />s containing the dto instance and the names of all validated fields of this
        ///     particular
        ///     <typeparamref
        ///         name="TModel" />
        /// </returns>

        protected readonly TValidator Validator = new TValidator();


        [TestCaseSource("InvalidModels")]
        public void Validate_WithAMaximalInvalidModel_ReturnsAllInvalidFields(
            Tuple<TModel, string[]> invalidDtoDescription)
        {
            var instance = invalidDtoDescription.Item1;
            var expectedErrors = invalidDtoDescription.Item2;

            var errors = from error in Validate(instance).Errors
                select error.PropertyName;

            CollectionAssert.AreEquivalent(expectedErrors, errors.Distinct(),
                "the validated fields defined in the test for {0} differ from the fields which {1} thinks are validated",
                typeof (TModel), typeof (TValidator));
        }


        [Test]
        public void Validate_WithAValidModel_IsValid()
        {
            var dto = CreateValidModel();

            var validationResult = Validate(dto);
            AssertIsValid(validationResult);
        }

        protected ValidationResult Validate(TModel instance)
        {
            return Validator.Validate(instance);
        }

        protected static void AssertIsValid(ValidationResult validationResult)
        {
            validationResult.IsValid.Should().BeTrue("a valid {0} should be recognized as, this failed due to {1}",
                typeof(TModel), string.Join(",", validationResult.Errors));
        }
    }
}