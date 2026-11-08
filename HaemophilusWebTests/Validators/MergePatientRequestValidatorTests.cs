using System.Linq;
using FluentAssertions;
using HaemophilusWeb.ViewModels;
using NUnit.Framework;

namespace HaemophilusWeb.Validators
{
    [TestFixture]
    public class MergePatientRequestValidatorTests
    {
        // specs/014 FR-004, US2.1
        [TestCase(8, 8)]
        [TestCase(0, 0)]
        public void EqualPatientNumbers_FailOnPatientOneId(int patientOneId, int patientTwoId)
        {
            var result = new MergePatientRequestValidator().Validate(new MergePatientRequest { PatientOneId = patientOneId, PatientTwoId = patientTwoId });

            result.IsValid.Should().BeFalse();
            result.Errors.Select(e => e.PropertyName).Should().Equal(nameof(MergePatientRequest.PatientOneId));
            result.Errors.Single().ErrorMessage.Should().Be("Bitte geben Sie zum Zusammenfügen zwei verschiedenen Patientennummern ein");
        }

        // specs/014 FR-004, FR-005: different numbers pass; whether they exist is checked by the controller
        [Test]
        public void DifferentPatientNumbers_AreValid()
        {
            var result = new MergePatientRequestValidator().Validate(new MergePatientRequest { PatientOneId = 8, PatientTwoId = 999 });

            result.IsValid.Should().BeTrue();
        }
    }
}
