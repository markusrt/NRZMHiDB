using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using FluentAssertions;
using HaemophilusWeb.Controllers;
using HaemophilusWeb.Mocks;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.ViewModels;
using NSubstitute;
using NUnit.Framework;

namespace HaemophilusWeb.Automapper
{
    public class IsolateViewModelMappingActionTests
    {
        private ApplicationDbContextMock DbMock { get; set; }

        [SetUp]
        public void SetUp()
        {
            DbMock = new ApplicationDbContextMock();
            IsolateViewModelMappingActionBase.DbForTest = DbMock;
            CreateMockData();
        }

        [TestCase("Test Department", "Test Department")]
        [TestCase(null, "")]
        public void ProcessModelToViewModel_PopulatesSenderData(string department, string expectedDepartment)
        {
            var sut = new IsolateViewModelMappingAction();
            var isolateViewModel = new IsolateViewModel();
            var isolate = CreateEmptyIsolate();
            DbMock.Senders.Find(1).Name = "Test Sender";
            DbMock.Senders.Find(1).Department = department;
            DbMock.Senders.Find(1).PostalCode = "12345";
            DbMock.Senders.Find(1).City = "The City";
            DbMock.Senders.Find(1).StreetWithNumber = "Long Street 123456";

            sut.Process(isolate, isolateViewModel, null);

            isolateViewModel.SenderName.Should().Be("Test Sender");
            isolateViewModel.SenderCity.Should().Be("12345 The City");
            isolateViewModel.SenderStreet.Should().Be("Long Street 123456");
            isolateViewModel.SenderDepartment.Should().Be(expectedDepartment);
        }

        [TestCase(SamplingLocation.OtherNonInvasive, "Nein")]
        [TestCase(SamplingLocation.OtherInvasive, "Ja")]
        public void ProcessModelToViewModel_PopulatesInvasive(SamplingLocation samplingLocation, string expectedInvasive)
        {
            var sut = new IsolateViewModelMappingAction();
            var isolateViewModel = new IsolateViewModel();
            var isolate = CreateEmptyIsolate();
            isolate.Sending.SamplingLocation = samplingLocation;
            isolate.Sending.OtherSamplingLocation = "other location";

            sut.Process(isolate, isolateViewModel, null);

            isolateViewModel.SamplingLocation.Should().Be("other location");
            isolateViewModel.Invasive.Should().Be(expectedInvasive);
        }

        [Test]
        public void ProcessModelToViewModel_PopulatesDemisIdImageUrl()
        {
            var sut = new IsolateViewModelMappingAction();
            var isolateViewModel = new IsolateViewModel();
            var isolate = CreateEmptyIsolate();
            isolate.Sending.DemisId = "aa8fbf39-5cec-4000-9361-a2023a9a013c";

            sut.Process(isolate, isolateViewModel, null);

            isolateViewModel.DemisIdQrImageUrl.Should().StartWith("data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAZoAAAGaCAYAAAA2BoVjAAAA");
            isolateViewModel.DemisIdQrImageUrl.Should().EndWith("/aNZaa31qf2jWWmt9an9o1lprfWp/aNZaa33of//7PzFUPlvfvwGnAAAAAElFTkSuQmCC");
        }
        
        // specs/004 FR-006, FR-007: one free row, then the saved measurements, then the standard antibiotics of this
        // database without a measurement
        [Test]
        public void EpsilometerTestsModelToViewModel_BuildsFreeSavedAndStandardRows()
        {
            var ciprofloxacin = new EucastClinicalBreakpoint
            {
                EucastClinicalBreakpointId = 7, Antibiotic = Antibiotic.Ciprofloxacin, ValidFor = DatabaseType.Haemophilus
            };
            var sut = new ETestRowMapping(
                new List<Antibiotic> { Antibiotic.Ampicillin, Antibiotic.Meropenem, Antibiotic.Rifampicin },
                ciprofloxacin,
                new EucastClinicalBreakpoint { Antibiotic = Antibiotic.Ampicillin, ValidFor = DatabaseType.Haemophilus },
                new EucastClinicalBreakpoint { Antibiotic = Antibiotic.Meropenem, ValidFor = DatabaseType.Haemophilus },
                new EucastClinicalBreakpoint { Antibiotic = Antibiotic.Rifampicin, ValidFor = DatabaseType.Meningococci });

            var rows = sut.ToViewModels(new EpsilometerTest
            {
                EucastClinicalBreakpointId = 7,
                EucastClinicalBreakpoint = ciprofloxacin,
                Measurement = 0.5f,
                Result = EpsilometerTestResult.Susceptible
            }).ToList();

            rows.Select(r => r.Antibiotic).Should().Equal(null, Antibiotic.Ciprofloxacin, Antibiotic.Ampicillin, Antibiotic.Meropenem);
            rows.Select(r => r.ReadonlyAntibiotic).Should().Equal(false, true, true, true);
            rows.Select(r => r.Measurement).Should().Equal(null, 0.5f, null, null);
            rows[1].Result.Should().Be(EpsilometerTestResult.Susceptible);
        }

        // specs/004 FR-006: only rows with a measurement are stored
        [Test]
        public void EpsilometerTestsViewModelToModel_RowWithoutMeasurement_IsNotStored()
        {
            var sut = new ETestRowMapping(new List<Antibiotic>());

            var eTests = sut.ToModels(
                new EpsilometerTestViewModel(Antibiotic.Ampicillin),
                new EpsilometerTestViewModel(Antibiotic.Meropenem)
                {
                    EucastClinicalBreakpointId = 5, Measurement = 0.25f, Result = EpsilometerTestResult.Susceptible
                }).ToList();

            eTests.Should().ContainSingle();
            eTests[0].EucastClinicalBreakpointId.Should().Be(5);
            eTests[0].Measurement.Should().Be(0.25f);
            eTests[0].Result.Should().Be(EpsilometerTestResult.Susceptible);
        }

        private class ETestRowMapping : IsolateViewModelMappingActionBase
        {
            public ETestRowMapping(List<Antibiotic> primaryAntibiotics, params EucastClinicalBreakpoint[] breakpoints)
                : base(DatabaseType.Haemophilus, primaryAntibiotics)
            {
                var breakpointSet = new InMemoryDbSet<EucastClinicalBreakpoint>(new HashSet<EucastClinicalBreakpoint>());
                foreach (var breakpoint in breakpoints)
                {
                    breakpointSet.Add(breakpoint);
                }

                db = Substitute.For<IApplicationDbContext>();
                db.EucastClinicalBreakpoints.Returns(breakpointSet);
            }

            public ICollection<EpsilometerTestViewModel> ToViewModels(params EpsilometerTest[] eTests)
            {
                return EpsilometerTestsModelToViewModel(eTests);
            }

            public ICollection<EpsilometerTest> ToModels(params EpsilometerTestViewModel[] rows)
            {
                return EpsilometerTestsViewModelToModel(rows);
            }
        }

        private static Isolate CreateEmptyIsolate()
        {
            return new Isolate
            {
                EpsilometerTests = new List<EpsilometerTest>(),
                Sending = new Sending { Patient = new Patient(), SenderId = 1}
            };
        }

        private void CreateMockData()
        {
            DbMock.Senders.Add(new Sender {SenderId = 1});
        }
    }
}