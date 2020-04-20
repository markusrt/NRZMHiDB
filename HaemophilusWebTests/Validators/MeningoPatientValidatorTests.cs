using System;
using System.Collections.Generic;
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
                Initials = "E.M.",
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

        private static MeningoPatient CreatePatientWithEmptyInitials()
        {
            var patient = CreatePatient();
            patient.Initials = string.Empty;
            return patient;
        }
    }
}