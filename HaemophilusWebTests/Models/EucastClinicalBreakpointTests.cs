using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;

namespace HaemophilusWeb.Models
{
    public class EucastClinicalBreakpointTests
    {
        private CultureInfo previousCulture;

        [SetUp]
        public void SetUp()
        {
            previousCulture = Thread.CurrentThread.CurrentCulture;
            // the application runs with the German culture (Web.config globalization)
            Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
        }

        [TearDown]
        public void TearDown()
        {
            Thread.CurrentThread.CurrentCulture = previousCulture;
        }

        // specs/017 FR-008, US2.7: label "<details> - v<version> vom <dd. MMM. yy>" as observed on the isolate form
        // ("SV-017-Ceftriaxon Men - vSV-017-1 vom 01. Mrz. 25", 017 verification 2026-11-08)
        [Test]
        public void Title_WithBreakpoints_ShowsDetailsVersionAndValidFromDate()
        {
            var breakpoint = new EucastClinicalBreakpoint
            {
                AntibioticDetails = "SV-017-Ceftriaxon Men",
                Version = "SV-017-1",
                ValidFrom = new DateTime(2025, 3, 1),
                MicBreakpointSusceptible = 0.125f,
                MicBreakpointResistent = 0.125f
            };

            breakpoint.NoEucastAvailable.Should().BeFalse();
            breakpoint.Title.Should().Be("SV-017-Ceftriaxon Men - vSV-017-1 vom 01. Mrz. 25");
        }

        // specs/017 FR-007, US2.6: all four values empty is an "ohne Grenzwerte" record
        [Test]
        public void Title_WithoutVersionDateAndThresholds_IsOhneGrenzwerte()
        {
            var breakpoint = new EucastClinicalBreakpoint { AntibioticDetails = "SV-017-Ceftriaxon Men" };

            breakpoint.NoEucastAvailable.Should().BeTrue();
            breakpoint.Title.Should().Be("SV-017-Ceftriaxon Men - ohne Grenzwerte");
        }

        // specs/017 FR-007: a single value makes the record count as one with breakpoints
        [TestCase("Version")]
        [TestCase("ValidFrom")]
        [TestCase("MicBreakpointSusceptible")]
        [TestCase("MicBreakpointResistent")]
        public void NoEucastAvailable_AnyOfTheFourValuesSet_IsFalse(string member)
        {
            var breakpoint = new EucastClinicalBreakpoint { AntibioticDetails = "SV-017" };
            var property = typeof(EucastClinicalBreakpoint).GetProperty(member);
            object value = member == "Version" ? (object) "1.0"
                : member == "ValidFrom" ? (object) new DateTime(2025, 1, 1)
                : 0.5f;
            property.SetValue(breakpoint, value);

            breakpoint.NoEucastAvailable.Should().BeFalse();
        }

        // specs/017 FR-005, US2.5: details at most 128 characters, version at most 64 characters
        [Test]
        public void Validate_DetailsAndVersionAtTheirLimits_IsValid()
        {
            var breakpoint = new EucastClinicalBreakpoint
            {
                AntibioticDetails = new string('x', 128),
                Version = new string('v', 64)
            };

            Validate(breakpoint).Should().BeEmpty();
        }

        [Test]
        public void Validate_DetailsAndVersionTooLong_FailsOnBothMembers()
        {
            var breakpoint = new EucastClinicalBreakpoint
            {
                AntibioticDetails = new string('x', 129),
                Version = new string('v', 65)
            };

            Validate(breakpoint).Should().BeEquivalentTo("AntibioticDetails", "Version");
        }

        private static IEnumerable<string> Validate(EucastClinicalBreakpoint breakpoint)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(breakpoint, new ValidationContext(breakpoint), results, true);
            return results.SelectMany(r => r.MemberNames).ToList();
        }
    }
}
