using System;
using System.Collections.Generic;
using System.Linq;
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

            export.Columns.Count.Should().Be(77);
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
            export.Rows[0]["Spezies Einsender"].Should().Be(Sending.SenderSpecies);
            export.Rows[0]["Serotyp Einsender"].Should().Be(Sending.SerotypeSender);
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

        // specs/024 FR-011, US5.3: without RKI match record the reference and file number stay empty and the status
        // reads "kein Match", the same word as a stored state None
        [Test]
        public void DataTable_WithoutRkiMatchRecord_ShowsEmptyReferenceAndKeinMatch()
        {
            var sut = CreateExportDefinition();
            Sending.RkiMatchRecord = null;

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["RKI InterneRef"].Should().Be(System.DBNull.Value);
            export.Rows[0]["RKI Aktenzeichen"].Should().Be(System.DBNull.Value);
            export.Rows[0]["RKI Status"].Should().Be("kein Match");
        }

        [Test]
        public void DataTable_ContainsRealTimePcrFields()
        {
            var sut = CreateExportDefinition();
            Sending.Isolate.RealTimePcr = NativeMaterialTestResult.Positive;
            Sending.Isolate.RealTimePcrResult = RealTimePcrResult.StreptococcusPneumoniae;

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["Real-Time-PCR"].Should().Be("positiv");
            export.Rows[0]["Real-Time-PCR Auswertung (RIDOM)"].Should().Be("Streptococcus pneumoniae");
        }

        [Test]
        public void DataTable_ContainsPenicillinAdtWithMmSuffix()
        {
            var sut = CreateExportDefinition();
            Sending.Isolate.PenicillinAdt = PenicillinAdt.Six;

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["Penicillin ADT"].Should().Be("6 mm");
        }

        [Test]
        public void DataTable_ContainsPenicillinAdtNotDetermined()
        {
            var sut = CreateExportDefinition();
            Sending.Isolate.PenicillinAdt = PenicillinAdt.NotDetermined;

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["Penicillin ADT"].Should().Be(DBNull.Value);
        }

        [Test]
        public void DataTable_ContainsPenicillinAdtGreaterThanTwelve()
        {
            var sut = CreateExportDefinition();
            Sending.Isolate.PenicillinAdt = PenicillinAdt.GreaterThanTwelve;

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["Penicillin ADT"].Should().Be("≥ 12 mm");
        }

        // specs/011 FR-004, FR-008, FR-015, US1.5: header names and order of the Haemophilus laboratory export as
        // observed in the downloaded file (011 verification 2026-11-08).
        [Test]
        public void DataTable_ColumnHeadersAreInDocumentedOrder()
        {
            var export = CreateExportDefinition().ToDataTable(Sendings);

            export.Columns.Cast<System.Data.DataColumn>().Select(c => c.ColumnName).Should().Equal(
                "Patienten-Nr.", "Labornummer", "Stammnummer", "Einsendernummer", "Eingangsdatum", "Entnahmedatum",
                "Labnr. Einsender", "DEMIS ID", "Entnahmeort", "Material", "Invasiv", "Serotyp Einsender",
                "Spezies Einsender", "Initialen", "Geburtsdatum", "Patientenalter bei Entnahme", "Geschlecht",
                "Postleitzahl", "Wohnort", "Landkreis", "Bundesland", "Klinische Angaben", "Hib-Impfung",
                "Datum Hib-Impfung", "Bemerkung (Einsendung)", "Wachstum", "Art des Wachstums", "Oxidase",
                "ß-Laktamase", "Penicillin ADT", "Agglutination", "Faktoren-test", "Ampicillin MHK",
                "Ampicillin Bewertung", "Amoxicillin / Clavulansäure MHK", "Amoxicillin / Clavulansäure Bewertung",
                "Meropenem MHK", "Meropenem Bewertung", "Cefotaxim MHK", "Cefotaxim Bewertung", "Imipenem MHK",
                "Imipenem Bewertung", "Ciprofloxacin MHK", "Ciprofloxacin Bewertung", "ompP2", "bexA", "Serotyp-PCR",
                "fucK", "ompP6", "Real-Time-PCR", "Real-Time-PCR Gerät", "Real-Time-PCR Auswertung (RIDOM)",
                "16S rDNA", "16S rDNA beste Übereinstimmung", "16S rDNA Übereinstimmung", "api NH",
                "api NH beste Übereinstimmung", "api NH Übereinstimmung", "MALDI-TOF (VITEK MS)",
                "MALDI-TOF (VITEK MS) beste Übereinstimmung", "MALDI-TOF (VITEK MS) Übereinstimmung",
                "MALDI-TOF (Biotyper)", "MALDI-TOF (Biotyper) beste Übereinstimmung",
                "MALDI-TOF (Biotyper) Übereinstimmung", "Genomsequenzierung", "ftsI", "ftsI Beurteilung",
                "ftsI Beurteilung 2", "ftsI Beurteilung 3", "MLST", "MLST Sequenztyp", "Beurteilung", "Befund am",
                "Bemerkung (Isolat)", "RKI InterneRef", "RKI Aktenzeichen", "RKI Status");
        }

        private static HaemophilusSendingLaboratoryExport CreateExportDefinition()
        {
            return new HaemophilusSendingLaboratoryExport();
        }
    }
}