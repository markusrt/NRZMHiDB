using System;
using System.Collections.Generic;
using FluentAssertions;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Services;
using HaemophilusWeb.TestUtils;
using NUnit.Framework;
using static HaemophilusWeb.Services.PubMlstService;

namespace HaemophilusWeb.Tools
{
    public class MeningoStateAuthorityExportTests
    {
        private IEnumerable<MeningoSending> Sendings => new List<MeningoSending> {Sending};

        private MeningoSending Sending { get; set; }

        [SetUp]
        public void Setup()
        {
            Sending  = MockData.CreateInstance<MeningoSending>();
            Sending.Isolate.EpsilometerTests.Clear();
            Sending.Isolate.EpsilometerTests.Add(MockData.CreateInstance<EpsilometerTest>());
        }

        [Test]
        public void Ctor_DoesNotCrash()
        {
            var sut = CreateExportDefinition();

            sut.Should().NotBeNull();
        }

        [Test]
        public void DataTable_ContainsAllColumns()
        {
            var sut = CreateExportDefinition();

            var export = sut.ToDataTable(Sendings);

            export.Columns.Count.Should().Be(10);
        }

        [Test]
        public void DataTable_DatesAreFormattedCorrectly()
        {
            var sut = CreateExportDefinition();

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["Eingangsdatum"].ToString().Should().Match("??.??.????");
            export.Rows[0]["Entnahmedatum"].ToString().Should().Match("??.??.????");
            export.Rows[0]["Geburtsmonat"].ToString().Should().Match("?? / ????");
        }

        [Test]
        public void DataTable_ContainsGenderFirstLetter()
        {
            var sut = CreateExportDefinition();
            Sending.Patient.Gender = Gender.Female;

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["Geschlecht"].Should().Be("w");
        }

        [Test]
        public void DataTable_ContainsGenderEmpty()
        {
            var sut = CreateExportDefinition();
            Sending.Patient.Gender = null;

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["Geschlecht"].Should().Be(DBNull.Value);
        }

        [Test]
        public void DataTable_ContainsSimpleProperties()
        {
            var sut = CreateExportDefinition();
            Sending.Isolate.PorAVr1 = "5-2";
            Sending.Isolate.PorAVr2 = "10-1";
            Sending.Isolate.FetAVr = "4-1";
            Sending.Patient.County = "Berlin, Stadt";

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["Patientnr NRZM"].Should().Be(Sending.MeningoPatientId);
            export.Rows[0]["PorA VR1"].Should().Be("5-2");
            export.Rows[0]["PorA VR2"].Should().Be("10-1");
            export.Rows[0]["FetA VR"].Should().Be("4-1");
            export.Rows[0]["Kreis"].Should().Be("Berlin, Stadt");
        }

        private static MeningoStateAuthorityExport CreateExportDefinition()
        {
            return new MeningoStateAuthorityExport();
        }
    }
}