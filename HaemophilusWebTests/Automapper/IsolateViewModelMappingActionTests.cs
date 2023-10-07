using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using FluentAssertions;
using HaemophilusWeb.Controllers;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.ViewModels;
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

            sut.Process(isolate, isolateViewModel);

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

            sut.Process(isolate, isolateViewModel);

            isolateViewModel.SamplingLocation.Should().Be("other location");
            isolateViewModel.Invasive.Should().Be(expectedInvasive);
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