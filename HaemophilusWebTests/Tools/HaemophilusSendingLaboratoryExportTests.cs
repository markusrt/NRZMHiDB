﻿using System.Collections.Generic;
using FluentAssertions;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.TestUtils;
using NUnit.Framework;

namespace HaemophilusWeb.Tools
{
    public class HaemophilusSendingLaboratoryExportTests
    {
        private IEnumerable<Sending> Sendings => new List<Sending> {Sending};

        private Sending Sending { get; set; }

        [SetUp]
        public void Setup()
        {
            Sending  = MockData.CreateInstance<Sending>();
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

            export.Columns.Count.Should().Be(74);
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

        [TestCase(SamplingLocation.OtherNonInvasive)]
        [TestCase(SamplingLocation.OtherInvasive)]
        public void DataTable_ContainsOtherInvasiveSamplingLocation(SamplingLocation samplingLocation)
        {
            var sut = CreateExportDefinition();
            Sending.SamplingLocation = samplingLocation;
            Sending.OtherSamplingLocation = "Other Location";

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["Entnahmeort"].Should().Be("Other Location");
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
            export.Rows[0]["Patienten-Nr."].Should().Be(Sending.PatientId);
            export.Rows[0]["DEMIS ID"].Should().Be(Sending.DemisId);
        }

        [Test]
        public void DataTable_ContainsEnumStringProperties()
        {
            var sut = CreateExportDefinition();
            Sending.Material = Material.IsolatedDna;
            Sending.Patient.Gender = Gender.Intersex;
            Sending.Patient.State = State.BY;
            Sending.Isolate.Growth = YesNoOptional.No;
            Sending.Isolate.Oxidase = TestResult.Positive;
            Sending.Isolate.GenomeSequencing = UnspecificTestResult.Determined;

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["Material"].Should().Be("Isolierte DNA");
            export.Rows[0]["Geschlecht"].Should().Be("divers");
            export.Rows[0]["Bundesland"].Should().Be("Bayern");
            export.Rows[0]["Wachstum"].Should().Be("Nein");
            export.Rows[0]["Oxidase"].Should().Be("positiv");
            export.Rows[0]["Genomsequenzierung"].Should().Be("durchgeführt");
        }

        [Test]
        public void DataTable_ContainsRkiMatchRecord()
        {
            var sut = CreateExportDefinition();
            Sending.RkiMatchRecord.RkiStatus = RkiStatus.Possible;

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["RKI InterneRef"].Should().Be(Sending.RkiMatchRecord.RkiReferenceId);
            export.Rows[0]["RKI Aktenzeichen"].Should().Be(Sending.RkiMatchRecord.RkiReferenceNumber);
            export.Rows[0]["RKI Status"].Should().Be("möglich");
        }

        [Test]
        public void DataTable_ContainsRealTimePcrFields()
        {
            var sut = CreateExportDefinition();
            Sending.Isolate.RealTimePcr = NativeMaterialTestResult.Positive;
            Sending.Isolate.RealTimePcrResult = RealTimePcrResult.StreptococcusPneumoniae;

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["NHS Real-Time-PCR"].Should().Be("positiv");
            export.Rows[0]["NHS Real-Time-PCR Auswertung (RIDOM)"].Should().Be("Streptococcus pneumoniae");
        }

        private static HaemophilusSendingLaboratoryExport CreateExportDefinition()
        {
            return new HaemophilusSendingLaboratoryExport();
        }
    }
}