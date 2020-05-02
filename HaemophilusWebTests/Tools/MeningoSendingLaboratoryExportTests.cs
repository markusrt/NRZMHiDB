using System.Collections.Generic;
using FluentAssertions;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.TestUtils;
using NUnit.Framework;

namespace HaemophilusWeb.Tools
{
    public class MeningoSendingLaboratoryExportTests
    {
        private IEnumerable<MeningoSending> Sendings => new List<MeningoSending> {Sending};

        private MeningoSending Sending { get; set; }

        [SetUp]
        public void Setup()
        {
            Sending  = MockData.CreateInstance<MeningoSending>();
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

            export.Columns.Count.Should().Be(42);
        }

        [Test]
        public void DataTable_DatesAreFormattedCorrectly()
        {
            var sut = CreateExportDefinition();

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["Eingangsdatum"].ToString().Should().Match("??.??.????");
            export.Rows[0]["Entnahmedatum"].ToString().Should().Match("??.??.????");
            export.Rows[0]["Geburtsdatum"].ToString().Should().Match("??.??.????");
        }

        [Test]
        public void DataTable_ContainsOtherInvasiveSamplingLocation()
        {
            var sut = CreateExportDefinition();
            Sending.SamplingLocation = MeningoSamplingLocation.OtherInvasive;
            Sending.OtherInvasiveSamplingLocation = "Invasive Location";

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["Entnahmeort"].Should().Be("Invasive Location");
        }

        [Test]
        public void DataTable_ContainsOtherNonInvasiveSamplingLocation()
        {
            var sut = CreateExportDefinition();
            Sending.SamplingLocation = MeningoSamplingLocation.OtherInvasive;
            Sending.OtherInvasiveSamplingLocation = "Non-Invasive Location";

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["Entnahmeort"].Should().Be("Non-Invasive Location");
        }

        [Test]
        public void DataTable_ContainsSimpleProperties()
        {
            var sut = CreateExportDefinition();

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["Labornummer"].Should().Be(Sending.Isolate.LaboratoryNumberWithPrefix);
            export.Rows[0]["Stammnummer"].Should().Be(Sending.Isolate.StemNumberWithPrefix);
            export.Rows[0]["Einsendernummer"].Should().Be(Sending.SenderId);
            export.Rows[0]["Labnr. Einsender"].Should().Be(Sending.SenderLaboratoryNumber);
        }

        private static MeningoSendingLaboratoryExport CreateExportDefinition()
        {
            return new MeningoSendingLaboratoryExport();
        }
    }
}