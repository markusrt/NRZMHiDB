﻿using System.Collections.Generic;
using FluentAssertions;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Services;
using HaemophilusWeb.TestUtils;
using NUnit.Framework;
using static HaemophilusWeb.Services.PubMlstService;

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
        public void DataTable_ContainsRealTimePcrFields()
        {
            var sut = CreateExportDefinition();
            Sending.Isolate.RealTimePcr = NativeMaterialTestResult.Positive;
            Sending.Isolate.RealTimePcrResult = RealTimePcrResult.StreptococcusPneumoniae;

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["NHS Real-Time-PCR"].Should().Be("positiv");
            export.Rows[0]["NHS Real-Time-PCR Auswertung (RIDOM)"].Should().Be("Streptococcus pneumoniae");
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
            export.Rows[0]["Patienten-Nr."].Should().Be(Sending.MeningoPatientId);
        }

        [Test]
        public void DataTable_ContainsEnumStringProperties()
        {
            var sut = CreateExportDefinition();
            Sending.Material = MeningoMaterial.IsolatedDna;
            Sending.Patient.Gender = Gender.Intersex;
            Sending.Patient.State = State.BY;
            Sending.Isolate.GrowthOnMartinLewisAgar = Growth.ATypicalGrowth;
            Sending.Isolate.Oxidase = TestResult.Positive;

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["Material"].Should().Be("Isolierte DNA");
            export.Rows[0]["Geschlecht"].Should().Be("divers");
            export.Rows[0]["Bundesland"].Should().Be("Bayern");
            export.Rows[0]["Wachstum auf Martin-Lewis-Agar"].Should().Be("atypisches Wachstum");
            export.Rows[0]["Oxidase"].Should().Be("positiv");
        }

        [Test]
        public void DataTable_ContainsPubMlstProperties()
        {
            var sut = CreateExportDefinition();

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["PubMLST ID"].Should().Be(Sending.Isolate.NeisseriaPubMlstIsolate.PubMlstId);
            export.Rows[0][PorAVr1].Should().Be(Sending.Isolate.NeisseriaPubMlstIsolate.PorAVr1);
            export.Rows[0][PorAVr2].Should().Be(Sending.Isolate.NeisseriaPubMlstIsolate.PorAVr2);
            export.Rows[0][FetAVr].Should().Be(Sending.Isolate.NeisseriaPubMlstIsolate.FetAVr);
            export.Rows[0][PorB].Should().Be(Sending.Isolate.NeisseriaPubMlstIsolate.PorB);
            export.Rows[0][NhbaPeptide].Should().Be(Sending.Isolate.NeisseriaPubMlstIsolate.Nhba);
            export.Rows[0][PenA].Should().Be(Sending.Isolate.NeisseriaPubMlstIsolate.PenA);
            export.Rows[0][GyrA].Should().Be(Sending.Isolate.NeisseriaPubMlstIsolate.GyrA);
            export.Rows[0]["parC"].Should().Be(Sending.Isolate.NeisseriaPubMlstIsolate.ParC);
            export.Rows[0][RpoB].Should().Be(Sending.Isolate.NeisseriaPubMlstIsolate.RpoB);
            export.Rows[0][RplF].Should().Be(Sending.Isolate.NeisseriaPubMlstIsolate.RplF);
            export.Rows[0][Fhbp].Should().Be(Sending.Isolate.NeisseriaPubMlstIsolate.Fhbp);
            export.Rows[0]["parE"].Should().Be(Sending.Isolate.NeisseriaPubMlstIsolate.ParE);
            export.Rows[0][SequenceType].Should().Be(Sending.Isolate.NeisseriaPubMlstIsolate.SequenceType);
            export.Rows[0][ClonalComplex].Should().Be(Sending.Isolate.NeisseriaPubMlstIsolate.ClonalComplex);
            export.Rows[0][BexseroReactivity].Should().Be(Sending.Isolate.NeisseriaPubMlstIsolate.BexseroReactivity);
            export.Rows[0][TrumenbaReactivity].Should().Be(Sending.Isolate.NeisseriaPubMlstIsolate.TrumenbaReactivity);
        }

        [Test]
        public void DataTable_OtherRiskFactors()
        {
            var sut = CreateExportDefinition();
            Sending.Patient.RiskFactors = RiskFactors.Asplenia | RiskFactors.Other;
            Sending.Patient.OtherRiskFactor = "Cardiac Insufficiency";

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["Risikofaktoren"].Should().Be("Asplenie, Cardiac Insufficiency");
        }

        [Test]
        public void DataTable_OtherClinicalInformation()
        {
            var sut = CreateExportDefinition();
            Sending.Patient.ClinicalInformation = MeningoClinicalInformation.Meningitis | MeningoClinicalInformation.Other;
            Sending.Patient.OtherClinicalInformation = "Craniocerebral Injury";

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["Klinische Angaben"].Should().Be("Meningitis, Craniocerebral Injury");
        }

        private static MeningoSendingLaboratoryExport CreateExportDefinition()
        {
            return new MeningoSendingLaboratoryExport();
        }
    }
}