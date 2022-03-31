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

            export.Columns.Count.Should().Be(12);
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
        public void DataTable_ContainsNoSpecies()
        {
            var sut = CreateExportDefinition();
            Sending.Isolate.MaldiTofBestMatch = null;
            Sending.Isolate.RibosomalRna16SBestMatch = null;
            Sending.Isolate.RealTimePcr = NativeMaterialTestResult.NotDetermined;

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["Spezies"].Should().Be(DBNull.Value);
        }

        [Test]
        public void DataTable_ContainsSpeciesForMaldiTof()
        {
            var sut = CreateExportDefinition();
            Sending.Isolate.RibosomalRna16SBestMatch = null;
            Sending.Isolate.MaldiTofBestMatch = "Spezies 1";

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["Spezies"].Should().Be("Spezies 1");
        }

        [Test]
        public void DataTable_ContainsSpeciesFor16S()
        {
            var sut = CreateExportDefinition();
            Sending.Isolate.MaldiTofBestMatch = null;
            Sending.Isolate.RibosomalRna16SBestMatch = "Spezies 2";

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["Spezies"].Should().Be("Spezies 2");
        }

        [Test]
        public void DataTable_ContainsSpeciesForRealTimePCR()
        {
            var sut = CreateExportDefinition();
            Sending.Isolate.MaldiTofBestMatch = null;
            Sending.Isolate.RibosomalRna16SBestMatch = null;
            Sending.Isolate.RealTimePcr = NativeMaterialTestResult.Positive;
            Sending.Isolate.RealTimePcrResult = RealTimePcrResult.HaemophilusInfluenzae;

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["Spezies"].Should().Be("Haemophilus influenzae");
        }

        [Test]
        public void DataTable_ContainsSimpleProperties()
        {
            var sut = CreateExportDefinition();
            Sending.Isolate.PorAPcr = NativeMaterialTestResult.Positive;
            Sending.Isolate.FetAPcr = NativeMaterialTestResult.Positive;
            Sending.Isolate.PorAVr1 = "5-2";
            Sending.Isolate.PorAVr2 = "10-1";
            Sending.Isolate.FetAVr = "4-1";
            Sending.Patient.County = "Berlin, Stadt";

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["Patientnr NRZM"].Should().Be(Sending.MeningoPatientId);
            export.Rows[0]["PorA VR1"].Should().Be("5-2");
            export.Rows[0]["PorA VR2"].Should().Be("10-1");
            export.Rows[0]["FetA VR"].Should().Be("4-1");
            export.Rows[0]["Landkreis"].Should().Be("11000");
            export.Rows[0]["Bundesland"].Should().Be("11");
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
        public void DataTable_ContainsEmptySerogroup()
        {
            var sut = CreateExportDefinition();

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["Serogruppe"].Should().Be(DBNull.Value);
        }

        [Test]
        public void DataTable_ContainsInterpretedSerogroup()
        {
            var sut = CreateExportDefinition();
            var isolate = new MeningoIsolate
            {
                CsbPcr = NativeMaterialTestResult.Negative,
                CscPcr = NativeMaterialTestResult.Positive,
                CswyPcr =  NativeMaterialTestResult.Negative,
                PorAPcr = NativeMaterialTestResult.Positive,
                FetAPcr = NativeMaterialTestResult.Positive,
                PorAVr1 = "X",
                PorAVr2 = "Y",
                FetAVr = "Z",
                Sending = Sending,
                EpsilometerTests = new List<EpsilometerTest>()
            };
            isolate.Sending.Isolate.GrowthOnMartinLewisAgar = Growth.TypicalGrowth;
            isolate.Sending.Material = MeningoMaterial.NativeMaterial;

            Sending.Isolate = isolate;

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["Serogruppe"].Should().Be("C");
        }

        private static MeningoStateAuthorityExport CreateExportDefinition()
        {
            return new MeningoStateAuthorityExport(new List<County> {new County()
            {
                CountyNumber = "11000",
                Name = "Berlin, Stadt"
            }});
        }
    }
}