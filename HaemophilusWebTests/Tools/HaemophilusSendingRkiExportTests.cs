using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.TestUtils;
using NUnit.Framework;
using static HaemophilusWeb.TestUtils.MockData;

namespace HaemophilusWeb.Tools
{
    public class HaemophilusSendingRkiExportTests
    {
        private IEnumerable<Sending> Sendings => new List<Sending> {Sending};
        
        private List<County> Counties { get; set; }

        private Sending Sending { get; set; }

        [SetUp]
        public void Setup()
        {
            Sending  = CreateInstance<Sending>();
            Sending.Isolate.EpsilometerTests.Clear();
            Sending.Isolate.EpsilometerTests.Add(Antibiotic.Ampicillin, 2);
            Sending.Isolate.EpsilometerTests.Add(Antibiotic.AmoxicillinClavulanate, 0.75f, EpsilometerTestResult.Susceptible);

            Counties = new List<County>();
            for (var i = 1; i < 4; i++)
            {
                var county = CreateInstance<County>();
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
            
            export.Columns.Count.Should().Be(26);
        }

        [Test]
        public void DataTable_ContainsOnlyPayloadColumnsAfterCleanup()
        {
            var duplicatePatientResolver = new DuplicatePatientResolver(new RkiExportColumns());
            var sut = CreateExportDefinition();

            var export = sut.ToDataTable(Sendings);
            
            duplicatePatientResolver.RemovePatientData(export);
            export.Columns.Count.Should().Be(23);
        }

        [TestCase(SamplingLocation.OtherInvasive)]
        [TestCase(SamplingLocation.OtherNonInvasive)]
        public void DataTable_ContainsRkiColumns(SamplingLocation samplingLocation)
        {
            var sut = CreateExportDefinition();
            var county = Counties.First();

            Sending.SamplingLocation = samplingLocation;
            Sending.OtherSamplingLocation = "Other Location";
            Sending.Patient.BirthDate = new DateTime(2005, 8, 31);
            Sending.Patient.County = county.Name;
            Sending.Patient.State = State.BB;
            Sending.Patient.Gender = Gender.Female;
            Sending.Patient.HibVaccination = VaccinationStatus.Yes;
            Sending.Isolate.Evaluation = Evaluation.HaemophilusTypeA;
            Sending.Isolate.BetaLactamase = TestResult.Negative;
            Sending.Isolate.RealTimePcr = NativeMaterialTestResult.Positive;
            Sending.Isolate.RealTimePcrResult = RealTimePcrResult.HaemophilusInfluenzae;

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["klhi_nr"].Should().Be(Sending.Isolate.StemNumber);
            export.Rows[0]["eing"].ToString().Should().Match("??.??.????");
            export.Rows[0]["ent"].ToString().Should().Match("??.??.????");
            export.Rows[0]["mat"].Should().Be("Other Location");
            export.Rows[0]["geb_monat"].Should().Be(8);
            export.Rows[0]["geb_jahr"].Should().Be(2005);
            export.Rows[0]["geschlecht"].Should().Be("w");
            export.Rows[0]["hib_impf"].Should().Be("Ja");
            export.Rows[0]["styp"].Should().Be("Hia");
            export.Rows[0]["b_lac"].Should().Be("negativ");
            export.Rows[0]["kreis_nr"].Should().Be(county.CountyNumber);
            export.Rows[0]["bundesland"].Should().Be("12");
            export.Rows[0]["einsender"].Should().Be(Sending.SenderId);
            export.Rows[0]["landkreis"].Should().Be(Sending.Patient.County);
            export.Rows[0]["bundeslandName"].Should().Be("Brandenburg");
            export.Rows[0]["ampicillinMHK"].Should().Be(2.0);
            export.Rows[0]["ampicillinBewertung"].Should().Be("resistent");
            export.Rows[0]["amoxicillinClavulansaeureMHK"].Should().Be(0.75);
            export.Rows[0]["bewertungAmoxicillinClavulansaeure"].Should().Be("sensibel");
            export.Rows[0]["Real-Time-PCR"].Should().Be("positiv");
            export.Rows[0][ "Real-Time-PCR Auswertung (RIDOM)"].Should().Be("Haemophilus influenzae");
        }

        [Test]
        public void DataTable_ContainsSimpleProperties()
        {
            var sut = CreateExportDefinition();

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["demis_id"].Should().Be(Sending.DemisId);
        }

        [Test]
        public void DataTable_SupportsNullForSimpleProperties()
        {
            var sut = CreateExportDefinition();
            Sending.DemisId = null;

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["demis_id"].Should().Be("");
        }

        [Test]
        public void DataTable_ContainsNullForUndefinedEnumValues()
        {
            var sut = CreateExportDefinition();
            Sending.Isolate.RealTimePcrResult = 0;

            var export = sut.ToDataTable(Sendings);

            export.Rows[0][ "Real-Time-PCR Auswertung (RIDOM)"].Should().Be(DBNull.Value);
        }


        // specs/012 FR-010: identifier removal keeps the indirect patient attributes of the RKI layout.
        [Test]
        public void DataTable_KeepsIndirectPatientColumnsAfterCleanup()
        {
            var col = new RkiExportColumns();
            var export = CreateExportDefinition().ToDataTable(Sendings);

            new DuplicatePatientResolver(col).RemovePatientData(export);

            var columns = export.Columns.Cast<System.Data.DataColumn>().Select(c => c.ColumnName).ToList();
            columns.Should().NotContain(new[] {col.Initials, col.DateOfBirth, col.PostalCode});
            columns.Should().Contain(new[]
            {
                col.BirthMonth, col.BirthYear, col.Sex, col.County, col.CountyNumber, col.State, col.StateNumber,
                col.SenderId, col.DemisId, col.SampleDate, col.ReceivedDate
            });
        }

        // specs/012 Edge Cases (empty-strain removal never takes effect): the RKI serotype/evaluation value is never
        // empty, whatever the evaluation.
        [Test]
        public void DataTable_SerotypeIsNeverEmpty([Values] Evaluation evaluation)
        {
            Sending.Isolate.Evaluation = evaluation;

            var export = CreateExportDefinition().ToDataTable(Sendings);

            export.Rows[0][new RkiExportColumns().Serotype].ToString().Should().NotBeNullOrEmpty();
        }

        // specs/012 FR-006, Edge Cases (stem numbers compared as numbers in the RKI export), US1.6: with the RKI
        // layout the stem numbers are ordered numerically (999 before 1000), the German receiving date is read, the
        // identical strain received ten days later is dropped and the remaining two rows of the patient form a group.
        [Test]
        public void CleanOrMarkDuplicates_WithRkiLayout_KeepsLowerNumericStemNumberAndMarksGroup()
        {
            var first = CreateSendingOfSamePatient(1000, new DateTime(2020, 1, 15), Evaluation.HaemophilusTypeB);
            var second = CreateSendingOfSamePatient(999, new DateTime(2020, 1, 5), Evaluation.HaemophilusTypeB);
            var third = CreateSendingOfSamePatient(1001, new DateTime(2020, 1, 20), Evaluation.HaemophilusNonEncapsulated);
            var col = new RkiExportColumns();
            var export = CreateExportDefinition().ToDataTable(new List<Sending> {second, first, third});

            new DuplicatePatientResolver(col).CleanOrMarkDuplicates(export);

            export.Rows.Count.Should().Be(2);
            export.Rows[0][col.StemNumber].Should().Be(999);
            export.Rows[1][col.StemNumber].Should().Be(1001);
            export.Rows[0][DuplicatePatientResolver.ColDuplicateGroup].Should().Be("Group 1");
            export.Rows[1][DuplicatePatientResolver.ColDuplicateGroup].Should().Be("Group 1");
            export.Columns.Contains(col.Initials).Should().BeFalse();
        }

        private Sending CreateSendingOfSamePatient(int stemNumber, DateTime receivingDate, Evaluation evaluation)
        {
            var sending = CreateInstance<Sending>();
            sending.Patient = Sending.Patient;
            sending.SamplingLocation = SamplingLocation.Blood;
            sending.ReceivingDate = receivingDate;
            sending.SamplingDate = receivingDate.AddDays(-2);
            sending.Isolate.StemNumber = stemNumber;
            sending.Isolate.Evaluation = evaluation;
            sending.Isolate.BetaLactamase = TestResult.Negative;
            sending.Isolate.EpsilometerTests.Clear();
            return sending;
        }

        private HaemophilusSendingRkiExport CreateExportDefinition()
        {
            return new HaemophilusSendingRkiExport(Counties);
        }
    }
}