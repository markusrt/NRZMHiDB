using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using NUnit.Framework;

namespace HaemophilusWeb.Validators
{
    [TestFixture]
    public class MeningoPatientValidatorTests : AbstractValidatorTests<MeningoPatientValidator, MeningoPatient>
    {
        protected static IEnumerable<Tuple<MeningoPatient, string[]>> InvalidModels;

        static MeningoPatientValidatorTests()
        {
            InvalidModels = CreateInvalidModels();
        }

        protected override MeningoPatient CreateValidModel()
        {
            return CreatePatient();
        }

        private static MeningoPatient CreatePatient()
        {
            return new MeningoPatient
            {
                Initials = "E-S.M-S.",
                PostalCode = "78987",
                Gender = Gender.Female
            };
        }

        protected static IEnumerable<Tuple<MeningoPatient, string[]>> CreateInvalidModels()
        {
            var invalidPatient = new MeningoPatient
            {
                Initials = "Herbert",
                State = State.ST
            };

            yield return Tuple.Create(invalidPatient, new[] {"Initials", "Gender" });

            yield return Tuple.Create(CreatePatientWithEmptyInitials(), new[] {"Initials"});

            var overseasPatient = CreatePatient();
            overseasPatient.State = State.Overseas;
            yield return Tuple.Create(overseasPatient, new[] { "Country" });

            var foreignPatient = CreatePatient();
            foreignPatient.Country = "Wonderland";
            yield return Tuple.Create(foreignPatient, new[] { "Country" });
        }

        // specs/003 FR-007: the birth date is optional; only a given date must be after 01.01.1800
        [Test]
        public void Validate_WithoutBirthDate_IsValid()
        {
            var patient = CreatePatient();
            patient.BirthDate = null;

            AssertIsValid(Validate(patient));
        }

        // specs/003 FR-007, Edge Cases (birth date too early)
        [TestCase(1800, 1, 1, false)]
        [TestCase(1800, 1, 2, true)]
        public void Validate_BirthDateAroundFirstJanuary1800_IsValidOnlyAfterIt(int year, int month, int day, bool expectedValid)
        {
            var patient = CreatePatient();
            patient.BirthDate = new DateTime(year, month, day);

            var result = Validate(patient);

            result.IsValid.Should().Be(expectedValid);
            if (!expectedValid)
            {
                result.Errors.Select(e => e.PropertyName).Should().Equal("BirthDate");
            }
        }

        // specs/003 FR-007: "?" stands for an unknown letter
        [Test]
        public void Validate_QuestionMarkInitials_IsValid()
        {
            var patient = CreatePatient();
            patient.Initials = "?.?.";

            AssertIsValid(Validate(patient));
        }

        private static MeningoPatient CreatePatientWithEmptyInitials()
        {
            var patient = CreatePatient();
            patient.Initials = string.Empty;
            return patient;
        }
    }
}