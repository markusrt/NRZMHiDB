using System.Collections.Generic;
using FluentAssertions;
using HaemophilusWeb.Models;
using NUnit.Framework;

namespace HaemophilusWeb.ViewModels
{
    public class IsolateViewModelTests
    {
        [Test]
        public void Ampicillin_NonExistingETestResult_CreatesNewEntry()
        {
            var isolateViewModel = CreateViewModel();

            var epsilometerTest = isolateViewModel.Ampicillin;
            epsilometerTest.Should().NotBeNull();
            epsilometerTest.EucastClinicalBreakpoint.Penicillin.Should().Be(IsolateViewModel.PenicillinAmpicillin);
        }

        [Test]
        public void AmoxicillinClavulanate_NonExistingETestResult_CreatesNewEntry()
        {
            var isolateViewModel = CreateViewModel();

            var epsilometerTest = isolateViewModel.AmoxicillinClavulanate;
            epsilometerTest.Should().NotBeNull();
            epsilometerTest.EucastClinicalBreakpoint.Penicillin.Should()
                .Be(IsolateViewModel.PenicillinAmoxicillinClavulanate);
        }

        [Test]
        public void Cefotaxime_NonExistingETestResult_CreatesNewEntry()
        {
            var isolateViewModel = CreateViewModel();

            var epsilometerTest = isolateViewModel.Cefotaxime;
            epsilometerTest.Should().NotBeNull();
            epsilometerTest.EucastClinicalBreakpoint.Penicillin.Should().Be(IsolateViewModel.PenicillinCefotaxime);
        }

        [Test]
        public void Meropenem_NonExistingETestResult_CreatesNewEntry()
        {
            var isolateViewModel = CreateViewModel();

            var epsilometerTest = isolateViewModel.Meropenem;
            epsilometerTest.Should().NotBeNull();
            epsilometerTest.EucastClinicalBreakpoint.Penicillin.Should().Be(IsolateViewModel.PenicillinMeropenem);
        }

        private static IsolateViewModel CreateViewModel()
        {
            var isolate = new Isolate {EpsilometerTests = new List<EpsilometerTest>()};
            return new IsolateViewModel
            {
                TheIsolate = isolate,
                CurrentClinicalBreakpoints = CreateMockBreakpoints()
            };
        }

        private static List<EucastClinicalBreakpoint> CreateMockBreakpoints()
        {
            var clinicalBreakPoints = new List<EucastClinicalBreakpoint>
            {
                CreateMockBreakpoint(IsolateViewModel.PenicillinAmpicillin),
                CreateMockBreakpoint(IsolateViewModel.PenicillinAmoxicillinClavulanate),
                CreateMockBreakpoint(IsolateViewModel.PenicillinCefotaxime),
                CreateMockBreakpoint(IsolateViewModel.PenicillinMeropenem)
            };
            return clinicalBreakPoints;
        }

        private static EucastClinicalBreakpoint CreateMockBreakpoint(string penicillin)
        {
            return new EucastClinicalBreakpoint
            {
                Penicillin = penicillin
            };
        }
    }
}