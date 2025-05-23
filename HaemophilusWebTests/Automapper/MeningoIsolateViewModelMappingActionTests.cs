﻿using System.Collections.Generic;
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
    public class MeningoIsolateViewModelMappingActionTests
    {
        private ApplicationDbContextMock DbMock { get; set; }

        [SetUp]
        public void SetUp()
        {
            DbMock = new ApplicationDbContextMock();
            IsolateViewModelMappingActionBase.DbForTest = DbMock;
            CreateMockData();
        }

        [TestCase(MeningoMaterial.IsolatedDna, MeningoMaterial.IsolatedDna)]
        [TestCase(MeningoMaterial.VitalStem, MeningoMaterial.NoGrowth)]
        [TestCase(MeningoMaterial.NoGrowth, MeningoMaterial.NoGrowth)]
        [TestCase(MeningoMaterial.NativeMaterial, MeningoMaterial.NativeMaterial)]
        public void ProcessViewModelToModel_NoGrowthAtAll_SetFieldMeningoMaterialToNoGrowthExceptForNativeAndDnaMaterial(MeningoMaterial currentMaterial, MeningoMaterial expectedMaterial)
        {
            var sut = new MeningoIsolateViewModelMappingAction();
            var isolateViewModel = new MeningoIsolateViewModel();
            var isolate = CreateEmptyIsolate();
            isolate.Sending.Material = currentMaterial;

            sut.Process(isolateViewModel, isolate, null);

            isolate.Sending.Material.Should().Be(expectedMaterial);
        }

        [Test]
        public void ProcessModelToViewModel_ConvertsEnumValuesToDescription()
        {
            var sut = new MeningoIsolateViewModelMappingAction();
            var isolateViewModel = new MeningoIsolateViewModel();
            var isolate = CreateEmptyIsolate();
            isolate.Sending.SamplingLocation = MeningoSamplingLocation.BloodAndLiquor;
            isolate.Sending.Material = MeningoMaterial.IsolatedDna;

            sut.Process(isolate, isolateViewModel, null);

            isolateViewModel.SamplingLocation.Should().Be("Blut und Liquor");
            isolateViewModel.Material.Should().Be("Isolierte DNA");
            isolateViewModel.Invasive.Should().Be("Ja");
        }

        [Test]
        public void ProcessModelToViewModel_OtherInvasiveSamplingLocation()
        {
            var sut = new MeningoIsolateViewModelMappingAction();
            var isolateViewModel = new MeningoIsolateViewModel();
            var isolate = CreateEmptyIsolate();
            isolate.Sending.SamplingLocation = MeningoSamplingLocation.OtherInvasive;
            isolate.Sending.OtherInvasiveSamplingLocation = "Invasive Location";

            sut.Process(isolate, isolateViewModel, null);

            isolateViewModel.SamplingLocation.Should().Be("Invasive Location");
        }

        [Test]
        public void ProcessModelToViewModel_OtherNonInvasiveSamplingLocation()
        {
            var sut = new MeningoIsolateViewModelMappingAction();
            var isolateViewModel = new MeningoIsolateViewModel();
            var isolate = CreateEmptyIsolate();
            isolate.Sending.SamplingLocation = MeningoSamplingLocation.OtherNonInvasive;
            isolate.Sending.OtherNonInvasiveSamplingLocation = "Noninvasive Location";

            sut.Process(isolate, isolateViewModel, null);

            isolateViewModel.SamplingLocation.Should().Be("Noninvasive Location");
        }

        [Test]
        public void ProcessModelToViewModel_SetsPatientId()
        {
            var sut = new MeningoIsolateViewModelMappingAction();
            var isolateViewModel = new MeningoIsolateViewModel();
            var isolate = CreateEmptyIsolate();
            isolate.Sending.Patient.PatientId = 123;

            sut.Process(isolate, isolateViewModel, null);

            isolateViewModel.PatientId.Should().Be(123);
        }

        [TestCase("Test Department", "Test Department")]
        [TestCase(null, "")]
        public void ProcessModelToViewModel_PopulatesSenderData(string department, string expectedDepartment)
        {
            var sut = new MeningoIsolateViewModelMappingAction();
            var isolateViewModel = new MeningoIsolateViewModel();
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

        [Test]
        public void ProcessModelToViewModel_PopulatesDemisIdImageUrl()
        {
            var sut = new MeningoIsolateViewModelMappingAction();
            var isolateViewModel = new MeningoIsolateViewModel();
            var isolate = CreateEmptyIsolate();
            isolate.Sending.DemisId = "aa8fbf39-5cec-4000-9361-a2023a9a013c";

            sut.Process(isolate, isolateViewModel, null);

            isolateViewModel.DemisIdQrImageUrl.Should().StartWith("data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAZoAAAGaCAYAAAA2BoVjAAAA");
            isolateViewModel.DemisIdQrImageUrl.Should().EndWith("/aNZaa31qf2jWWmt9an9o1lprfWp/aNZaa33of//7PzFUPlvfvwGnAAAAAElFTkSuQmCC");
        }

        private static MeningoIsolate CreateEmptyIsolate()
        {
            return new MeningoIsolate
            {
                EpsilometerTests = new List<EpsilometerTest>(),
                Sending = new MeningoSending { Patient = new MeningoPatient(), SenderId = 1}
            };
        }

        private void CreateMockData()
        {
            DbMock.Senders.Add(new Sender {SenderId = 1});
        }
    }
}