using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using HaemophilusWeb.Models;
using NUnit.Framework;

namespace HaemophilusWeb.Validators
{
    // specs/017 FR-005, US2.3, US2.4, US2.6: non-empty details; version, valid-from date and both thresholds either
    // all filled or all empty. The length limits are DataAnnotations rules (Models/EucastClinicalBreakpointTests).
    [TestFixture]
    public class EucastClinicalBreakpointValidatorTests
        : AbstractValidatorTests<EucastClinicalBreakpointValidator, EucastClinicalBreakpoint>
    {
        private const string AllOrNoneMessage =
            "Die Felder 'Version', 'Gültig ab', 'Sensibel <=' und 'Resistent >' müssen entweder alle gefüllt oder alle leer sein.";

        private static readonly string[] AllOrNoneFields =
            { "Version", "ValidFrom", "MicBreakpointResistent", "MicBreakpointSusceptible" };

        protected static readonly IEnumerable<Tuple<EucastClinicalBreakpoint, string[]>> InvalidModels;

        static EucastClinicalBreakpointValidatorTests()
        {
            InvalidModels = CreateInvalidModels();
        }

        protected override EucastClinicalBreakpoint CreateValidModel()
        {
            return CreateDatedBreakpoint();
        }

        private static EucastClinicalBreakpoint CreateDatedBreakpoint()
        {
            return new EucastClinicalBreakpoint
            {
                Antibiotic = Antibiotic.Ceftriaxone,
                AntibioticDetails = "SV-017-Ceftriaxon",
                Version = "14.0",
                ValidFor = DatabaseType.Meningococci,
                ValidFrom = new DateTime(2025, 3, 1),
                MicBreakpointSusceptible = 0.125f,
                MicBreakpointResistent = 0.125f
            };
        }

        private static IEnumerable<Tuple<EucastClinicalBreakpoint, string[]>> CreateInvalidModels()
        {
            var withoutDetails = CreateDatedBreakpoint();
            withoutDetails.AntibioticDetails = "";
            yield return Tuple.Create(withoutDetails, new[] { "AntibioticDetails" });

            var partial = new EucastClinicalBreakpoint
            {
                AntibioticDetails = null,
                Version = "SV-017-1",
                MicBreakpointSusceptible = 0.5f
            };
            yield return Tuple.Create(partial, new[] { "AntibioticDetails" }.Concat(AllOrNoneFields).ToArray());
        }

        // specs/017 FR-005, US2.6, FR-007: all four values empty is an "ohne Grenzwerte" record and valid
        [Test]
        public void Validate_AllFourValuesEmpty_IsValid()
        {
            var breakpoint = new EucastClinicalBreakpoint
            {
                Antibiotic = Antibiotic.Azithromycin,
                AntibioticDetails = "SV-017-Azithromycin",
                ValidFor = DatabaseType.Meningococci
            };

            AssertIsValid(Validate(breakpoint));
        }

        // specs/017 FR-005, US2.4: one missing value fails all four fields with the same message
        [TestCase("Version")]
        [TestCase("ValidFrom")]
        [TestCase("MicBreakpointSusceptible")]
        [TestCase("MicBreakpointResistent")]
        public void Validate_OneOfTheFourValuesMissing_FailsAllFourWithAllOrNoneMessage(string missing)
        {
            var breakpoint = CreateDatedBreakpoint();
            typeof(EucastClinicalBreakpoint).GetProperty(missing).SetValue(breakpoint, null);

            var errors = Validate(breakpoint).Errors;

            errors.Select(e => e.PropertyName).Should().BeEquivalentTo(AllOrNoneFields);
            errors.Select(e => e.ErrorMessage).Should().AllBe(AllOrNoneMessage);
        }
    }
}
