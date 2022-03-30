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
using Moq;
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

        [Test]
        public void ProcessModelToViewModel_PopulatesSenderData()
        {
            var sut = new IsolateViewModelMappingAction();
            var isolateViewModel = new IsolateViewModel();
            var isolate = CreateEmptyIsolate();
            DbMock.Senders.Find(1).Name = "Test Sender";
            DbMock.Senders.Find(1).Department = "Test Department";
            DbMock.Senders.Find(1).PostalCode = "12345";
            DbMock.Senders.Find(1).City = "The City";
            DbMock.Senders.Find(1).StreetWithNumber = "Long Street 123456";

            sut.Process(isolate, isolateViewModel);

            isolateViewModel.SenderName.Should().Be("Test Sender");
            isolateViewModel.SenderCity.Should().Be("12345 The City");
            isolateViewModel.SenderStreet.Should().Be("Long Street 123456");
            isolateViewModel.SenderDepartment.Should().Be("Test Department");
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