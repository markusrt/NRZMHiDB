using System;
using System.Collections.Generic;
using FluentAssertions;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Services;
using HaemophilusWeb.TestUtils;
using NUnit.Framework;
using static HaemophilusWeb.Services.PubMlstService;
using static HaemophilusWeb.TestUtils.MockData;

namespace HaemophilusWeb.Tools
{
    public class MeningoSendingRkiExportTests
    {
        private IEnumerable<MeningoSending> Sendings => new List<MeningoSending> {Sending};

        private MeningoSending Sending { get; set; }

        private List<County> Counties { get; set; }

        [SetUp]
        public void Setup()
        {
            Sending  = CreateInstance<MeningoSending>();
            Sending.Isolate.EpsilometerTests.Clear();

            Counties = new List<County>();
            for (var i = 1; i < 4; i++)
            {
                var county = MockData.CreateInstance<County>();
                county.CountyId = i;
                county.CountyNumber = (12064000 + i).ToString();
                Counties.Add(county);
            }
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

            export.Columns.Count.Should().Be(35);
        }

        [Test]
        public void DataTable_DatesAreFormattedCorrectly()
        {
            var sut = CreateExportDefinition();

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["Eingang NRZM"].ToString().Should().Be(Sending.ReceivingDate.ToString("dd.MM.yyyy"));
            export.Rows[0]["Entnahme"].ToString().Should().Be(Sending.SamplingDate.Value.ToString("dd.MM.yyyy"));
        }
        
        [Test]
        public void DataTable_ContainsSimpleProperties()
        {
            var sut = CreateExportDefinition();
            Sending.Isolate.PorAPcr = NativeMaterialTestResult.Positive;
            Sending.Isolate.FetAPcr = NativeMaterialTestResult.Positive;

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["PatNr NRZM"].Should().Be(Sending.Patient.PatientId);
            export.Rows[0]["PorA VR1"].Should().Be(Sending.Isolate.PorAVr1);
            export.Rows[0]["PorA VR2"].Should().Be(Sending.Isolate.PorAVr2);
            export.Rows[0]["FetA VR"].Should().Be(Sending.Isolate.FetAVr);
            export.Rows[0]["demis_id"].Should().Be(Sending.DemisId);
        }

        [Test]
        public void DataTable_SupportsNullForSimpleProperties()
        {
            var sut = CreateExportDefinition();
            Sending.DemisId = null;
            Sending.Isolate.PorAPcr = NativeMaterialTestResult.Positive;
            Sending.Isolate.PorAVr1 = null;
            Sending.Isolate.PorAVr2 = null;
            Sending.Isolate.FetAPcr = NativeMaterialTestResult.Positive;
            Sending.Isolate.FetAVr = null;

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["PorA VR1"].Should().Be("");
            export.Rows[0]["PorA VR2"].Should().Be("");
            export.Rows[0]["FetA VR"].Should().Be("");
            export.Rows[0]["demis_id"].Should().Be("");
        }

        [Test]
        public void DataTable_ContainsNegativePorAAndFetA()
        {
            var sut = CreateExportDefinition();
            Sending.Isolate.PorAPcr = NativeMaterialTestResult.Negative;
            Sending.Isolate.FetAPcr = NativeMaterialTestResult.Negative;

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["PorA VR1"].Should().Be("negativ");
            export.Rows[0]["PorA VR2"].Should().Be("negativ");
            export.Rows[0]["FetA VR"].Should().Be("negativ");
        }

        [Test]
        public void DataTable_ContainsEnumStringProperties()
        {
            var sut = CreateExportDefinition();
            Sending.Isolate.Agglutination = MeningoSerogroupAgg.WY;
            Sending.Isolate.SerogroupPcr = MeningoSerogroupPcr.Negative;
            Sending.Isolate.CscPcr = NativeMaterialTestResult.Negative;
            Sending.Isolate.CswyPcr = NativeMaterialTestResult.Positive;
            Sending.Isolate.CswyAllele = CswyAllel.Allele3;
            Sending.Patient.Gender = Gender.Intersex;
            Sending.Isolate.RealTimePcr = NativeMaterialTestResult.Positive;
            Sending.Isolate.RealTimePcrResult = RealTimePcrResult.NeisseriaMeningitidis;

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["Agglutination"].Should().Be("W/Y");
            export.Rows[0]["Serogruppen-PCR"].Should().Be("negativ");
            export.Rows[0]["Geschlecht"].Should().Be("d");
            export.Rows[0]["csc-PCR"].Should().Be("negativ");
            export.Rows[0]["cswy-PCR"].Should().Be("positiv");
            export.Rows[0]["cswy-Allel"].Should().Be("Allel 3 WY");
            export.Rows[0]["NHS Real-Time-PCR"].Should().Be("positiv");
            export.Rows[0][ "NHS Real-Time-PCR Auswertung (RIDOM)"].Should().Be("Neisseria meningitidis");
        }

        [Test]
        public void DataTable_ContainsNullForUndefinedEnumValues()
        {
            var sut = CreateExportDefinition();
            Sending.Isolate.CswyAllele = 0;
            Sending.Isolate.RealTimePcrResult = 0;

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["cswy-Allel"].Should().Be(DBNull.Value);
            export.Rows[0][ "NHS Real-Time-PCR Auswertung (RIDOM)"].Should().Be(DBNull.Value);
        }

        [Test]
        public void DataTable_ContainsPubMlstProperties()
        {
            var sut = CreateExportDefinition();

            var export = sut.ToDataTable(Sendings);

            var pubMlst = Sending.Isolate.NeisseriaPubMlstIsolate;
            export.Rows[0]["PubMLST ID"].Should().Be(pubMlst.PubMlstId);
            export.Rows[0]["Datenbank"].Should().Be(pubMlst.Database);
            export.Rows[0][PenA].Should().Be(pubMlst.PenA);
            export.Rows[0][GyrA].Should().Be(pubMlst.GyrA);
            export.Rows[0][RpoB].Should().Be(pubMlst.RpoB);
            export.Rows[0][SequenceType].Should().Be(pubMlst.SequenceType);
            export.Rows[0][ClonalComplex].Should().Be(pubMlst.ClonalComplex);
            export.Rows[0][PorAVr1].Should().Be(pubMlst.PorAVr1);
            export.Rows[0][PorAVr2].Should().Be(pubMlst.PorAVr2);
            export.Rows[0][FetAVr].Should().Be(pubMlst.FetAVr);
        }

        [Test]
        public void DataTable_PubMlstPropertiesAreEmptyForUnmatched()
        {
            var sut = CreateExportDefinition();
            Sending.Isolate.NeisseriaPubMlstIsolate = null;

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["PubMLST ID"].Should().Be("-");
            export.Rows[0][PenA].Should().Be(DBNull.Value);
            export.Rows[0][GyrA].Should().Be(DBNull.Value);
            export.Rows[0][RpoB].Should().Be(DBNull.Value);
            export.Rows[0][SequenceType].Should().Be(DBNull.Value);
            export.Rows[0][ClonalComplex].Should().Be(DBNull.Value);
            export.Rows[0][PorAVr1].Should().Be(DBNull.Value);
            export.Rows[0][PorAVr2].Should().Be(DBNull.Value);
            export.Rows[0][FetAVr].Should().Be(DBNull.Value);
        }

        [Test]
        public void DataTable_ContainsMicValues()
        {
            var sut = CreateExportDefinition();
            Sending.Isolate.EpsilometerTests.Clear();
            Sending.Isolate.EpsilometerTests.Add(Antibiotic.Benzylpenicillin, 1.0f);
            Sending.Isolate.EpsilometerTests.Add(Antibiotic.Benzylpenicillin, 2.0f);
            Sending.Isolate.EpsilometerTests.Add(Antibiotic.Ciprofloxacin, .75f);
            Sending.Isolate.EpsilometerTests.Add(Antibiotic.Cefotaxime, 1.25f);
            Sending.Isolate.EpsilometerTests.Add(Antibiotic.Rifampicin, 0.125f);

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["Penicillin MHK Etest"].Should().Be(2.0d);
            export.Rows[0]["Ciprofloxacin MHK Etest"].Should().Be(.75d);
            export.Rows[0]["Cefotaxim MHK Etest"].Should().Be(1.25d);
            export.Rows[0]["Rifampicin MHK Etest"].Should().Be(.125d);
        }

        private MeningoSendingRkiExport CreateExportDefinition()
        {
            return new MeningoSendingRkiExport(Counties);
        }
    }
}