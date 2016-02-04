using System;
using System.Collections.Generic;
using HaemophilusWeb.Models;
using NUnit.Framework;

namespace HaemophilusWeb.Validators
{
    [TestFixture]
    public class PatientValidatorTests : AbstractValidatorTests<PatientValidator, Patient>
    {
        protected override Patient CreateValidModel()
        {
            return new Patient
            {
                Initials = "E.M.",
                PostalCode = "78987",
                Gender = Gender.Female
            };
        }

        protected override IEnumerable<Tuple<Patient, string[]>> CreateInvalidModels()
        {
            var invalidPatient = new Patient()
            {
                PostalCode = "abc",
                Initials = "Herbert"
            };

            yield return Tuple.Create(invalidPatient, new[] {"Initials", "Gender"});

            yield return Tuple.Create(CreatePatientWithEmptyInitials(), new[] {"Initials"});
        }

        private Patient CreatePatientWithEmptyInitials()
        {
            var patient = CreateValidModel();
            patient.Initials = string.Empty;
            return patient;
        }
    }
}