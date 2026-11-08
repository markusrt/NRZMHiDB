using System.Globalization;
using System.Threading;
using FluentAssertions;
using HaemophilusWeb.Models;
using NUnit.Framework;

namespace HaemophilusWeb.ViewModels
{
    public class EpsilometerTestReportModelTests
    {
        private CultureInfo previousCulture;

        [SetUp]
        public void SetUp()
        {
            previousCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
        }

        [TearDown]
        public void TearDown()
        {
            Thread.CurrentThread.CurrentCulture = previousCulture;
        }

        // specs/017 FR-012: the report data carry the thresholds and the year of "Gültig ab" next to the stored result
        [Test]
        public void CreateFromViewModel_WithBreakpoints_CarriesThresholdsYearAndStoredResult()
        {
            var report = EpsilometerTestReportModel.CreateFromViewModel(new EpsilometerTestViewModel(Antibiotic.Ceftriaxone)
            {
                Measurement = 0.25f,
                Result = EpsilometerTestResult.Susceptible,
                MicBreakpointSusceptible = 0.25f,
                MicBreakpointResistent = 0.5f,
                ValidFromYear = 2025
            });

            report.Antibiotic.Should().Be("Ceftriaxon");
            report.Measurement.Should().Be("0,25");
            report.Result.Should().Be("sensibel");
            report.MicBreakpointSusceptible.Should().Be("0,25");
            report.MicBreakpointResistent.Should().Be("0,5");
            report.ValidFromYear.Should().Be("2025");
        }

        // specs/017 FR-012, SC-004: a "not determined" result shows "---" for both thresholds and the year
        [Test]
        public void CreateFromViewModel_NotDetermined_ShowsDashesForThresholdsAndYear()
        {
            var report = EpsilometerTestReportModel.CreateFromViewModel(new EpsilometerTestViewModel(Antibiotic.Ceftriaxone)
            {
                Measurement = 0.5f,
                Result = EpsilometerTestResult.NotDetermined
            });

            report.Result.Should().Be("n.d.");
            report.Measurement.Should().Be("0,5");
            report.MicBreakpointSusceptible.Should().Be("---");
            report.MicBreakpointResistent.Should().Be("---");
            report.ValidFromYear.Should().Be("---");
        }
    }
}
