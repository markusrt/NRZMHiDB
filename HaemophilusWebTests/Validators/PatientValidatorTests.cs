using System;
using System.Collections.Generic;
using HaemophilusWeb.Models;
using NUnit.Framework;

namespace HaemophilusWeb.Validators
{
    [TestFixture]
    public class PatientValidatorTests : AbstractValidatorTests<PatientValidator, Patient>
    {
        protected static IEnumerable<Tuple<Patient, string[]>> InvalidModels;

        static PatientValidatorTests()
        {
            InvalidModels = CreateInvalidModels();
        }

        protected override Patient CreateValidModel()
        {
            return CreatePatient();
        }

        private static Patient CreatePatient()
        {
            return new Patient
            {
                Initials = "E.M.",
                PostalCode = "78987",
                Gender = Gender.Female
            };
        }

        protected static IEnumerable<Tuple<Patient, string[]>> CreateInvalidModels()
        {
            var invalidPatient = new Patient
            {
                Initials = "Herbert",
                State = State.ST
            };

            yield return Tuple.Create(invalidPatient, new[] {"Initials", "Gender" });

            yield return Tuple.Create(CreatePatientWithEmptyInitials(), new[] {"Initials"});
        }

        private static Patient CreatePatientWithEmptyInitials()
        {
            var patient = CreatePatient();
            patient.Initials = string.Empty;
            return patient;
        }
    }
}