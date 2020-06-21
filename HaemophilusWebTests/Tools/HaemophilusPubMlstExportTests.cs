using System;
using System.Collections.Generic;
using FluentAssertions;
using HaemophilusWeb.Models;
using HaemophilusWeb.TestUtils;
using NUnit.Framework;

namespace HaemophilusWeb.Tools
{
    public class HaemophilusPubMlstExportTests
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

            export.Columns.Count.Should().Be(42);
        }

        [TestCase(1, "aliases")]
        [TestCase(2, "references")]
        [TestCase(8, "non_culture")]
        [TestCase(12, "genotype")]
        [TestCase(13, "biotype")]
        [TestCase(14, "ribotype")]
        [TestCase(19, "disease")]
        [TestCase(20, "epidemiology")]
        [TestCase(21, "MLEE_lineage")]
        [TestCase(22, "MLEE_ET")]
        [TestCase(24, "AMX_MIC")]
        [TestCase(30, "CRO_MIC")]
        [TestCase(31, "CRO_SIR")]
        [TestCase(32, "ENA_accession")]
        [TestCase(33, "comments")]
        [TestCase(34, "adk")]
        [TestCase(35, "atpG")]
        [TestCase(36, "frdB")]
        [TestCase(37, "fucK")]
        [TestCase(38, "mdh")]
        [TestCase(39, "pgi")]
        [TestCase(40, "recA")]
        public void DataTable_ContainsEmptyColumnsAtIndex(int index, string caption)
        {
            var sut = CreateExportDefinition();

            var export = sut.ToDataTable(Sendings);

            export.Columns[index].Caption.Should().Be(caption);
            export.Rows[0][caption].Should().Be(DBNull.Value);
        }

        [Test]
        public void DataTable_DatesAreFormattedCorrectly()
        {
            var sut = CreateExportDefinition();

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["date_sampled"].ToString().Should().Match("????-??-??");
            export.Rows[0]["date_received"].ToString().Should().Match("????-??-??");
        }

        [Test]
        public void DataTable_ContainsYear()
        {
            Sending.SamplingDate = new DateTime(2007, 8, 12);
            var sut = CreateExportDefinition();

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["year"].Should().Be(2007);
        }

        [Test]
        public void DataTable_DoesNotContainEmptyYear()
        {
            Sending.SamplingDate = null;
            var sut = CreateExportDefinition();

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["year"].Should().Be(DBNull.Value);
        }

        [TestCase(8, 0, 8)]
        [TestCase(25, 2, null)]
        public void DataTable_PatientAgeIsCalculated(int monthOld, int expectedYearsOld, object expectedMonthsOld)
        {
            expectedMonthsOld ??= DBNull.Value;
            var sut = CreateExportDefinition();
            var now = DateTime.Now;
            Sending.Isolate.Sending.SamplingDate = now;
            Sending.Isolate.Sending.Patient.BirthDate = now.AddMonths(-monthOld);

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["age_yr"].Should().Be(expectedYearsOld);
            export.Rows[0]["age_mth"].Should().Be(expectedMonthsOld);
        }

        [Test]
        public void DataTable_ContainsSimpleProperties()
        {
            var sut = CreateExportDefinition();

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["isolate"].Should().Be(Sending.Isolate.StemNumberWithPrefix);
            export.Rows[0]["country"].Should().Be("Germany");
        }

        [TestCase(Evaluation.HaemophilusTypeA, "a")]
        [TestCase(Evaluation.HaemophilusTypeF, "f")]
        [TestCase(Evaluation.NoHaemophilusSpecies, "ND")]
        [TestCase(Evaluation.HaemophilusNonEncapsulated, "NT")]
        public void DataTable_ContainsEvaluationProperties(Evaluation evaluation, string expectedValue)
        {
            var sut = CreateExportDefinition();
            Sending.Isolate.Evaluation = evaluation;

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["serotype"].Should().Be(expectedValue);
        }

        [TestCase(SerotypeAgg.A, "a")]
        [TestCase(SerotypeAgg.F, "f")]
        [TestCase(SerotypeAgg.NotDetermined, "")]
        [TestCase(SerotypeAgg.Negative, "NT")]
        public void DataTable_ContainsAgglutinationProperties(SerotypeAgg agglutination, string expectedValue)
        {
            var sut = CreateExportDefinition();
            Sending.Isolate.Agglutination = agglutination;

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["serotype_by_serology"].Should().Be(expectedValue);
        }

        [TestCase(SerotypePcr.A, "a")]
        [TestCase(SerotypePcr.D, "d")]
        [TestCase(SerotypePcr.NotDetermined, "")]
        [TestCase(SerotypePcr.Negative, "NT")]
        public void DataTable_ContainsSerotypePcrProperties(SerotypePcr serotypePcr, string expectedValue)
        {
            var sut = CreateExportDefinition();
            Sending.Isolate.SerotypePcr = serotypePcr;

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["serotype_by_PCR"].Should().Be(expectedValue);
        }

        [TestCase(SamplingLocation.Blood, "blood")]
        [TestCase(SamplingLocation.Liquor, "CSF")]
        [TestCase((SamplingLocation)23456, "other")]
        public void DataTable_ContainsSamplingLocation(SamplingLocation location, string expectedValue)
        {
            var sut = CreateExportDefinition();
            Sending.SamplingLocation = location;

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["source"].Should().Be(expectedValue);
        }

        [TestCase(TestResult.Positive, "positive")]
        [TestCase(TestResult.Negative, "negative")]
        [TestCase(TestResult.NotDetermined, "")]
        [TestCase((TestResult)23456, "")]
        public void DataTable_ContainsBetaLactamase(TestResult betaLactamase, string expectedValue)
        {
            var sut = CreateExportDefinition();
            Sending.Isolate.BetaLactamase = betaLactamase;

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["beta_lactamase"].Should().Be(expectedValue);
        }


        [Test]
        public void DataTable_ContainsOtherInvasiveSamplingLocation()
        {
            var sut = CreateExportDefinition();
            Sending.SamplingLocation = SamplingLocation.Other;
            Sending.OtherSamplingLocation = "Anderer Entnahmeort";

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["source"].Should().Be("Anderer Entnahmeort");
        }

        [Test]
        public void DataTable_ContainsEnumStringProperties()
        {
            var sut = CreateExportDefinition();
            Sending.Patient.Gender = Gender.Intersex;
            Sending.Patient.State = State.BY;

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["region"].Should().Be("Bayern");
        }

        [TestCase("CTX", Antibiotic.Cefotaxime, EpsilometerTestResult.Susceptible, 1.234f, "S", 1.234d)]
        [TestCase("CTX", Antibiotic.Cefotaxime, EpsilometerTestResult.Intermediate, 1f, "I", 1d)]
        [TestCase("AMC", Antibiotic.AmoxicillinClavulanate, EpsilometerTestResult.Resistant, 0.25f, "R", 0.25d)]
        [TestCase("AMC", Antibiotic.AmoxicillinClavulanate, EpsilometerTestResult.Intermediate, 1f, "I", 1d)]
        public void DataTable_ContainsAntibioticMeasurements(string prefix, Antibiotic antibiotic, EpsilometerTestResult testResult, float measurement, string expectedTestResult, double expectedMeasurement)
        {
            var sut = CreateExportDefinition();
            Sending.Isolate.EpsilometerTests.Add(new EpsilometerTest
            {
                Result = testResult,
                Measurement = measurement,
                EucastClinicalBreakpoint = new EucastClinicalBreakpoint {Antibiotic = antibiotic}
            });

            var export = sut.ToDataTable(Sendings);

            export.Rows[0][$"{prefix}_SIR"].Should().Be(expectedTestResult);
            export.Rows[0][$"{prefix}_MIC"].Should().Be(expectedMeasurement);
        }

        [Test]
        public void DataTable_ContainsNoAntibiotics()
        {
            var sut = CreateExportDefinition();
            Sending.Isolate.EpsilometerTests.Add(new EpsilometerTest
            {
                Result = EpsilometerTestResult.Resistant,
                Measurement = 1.23f,
                EucastClinicalBreakpoint = new EucastClinicalBreakpoint {Antibiotic = Antibiotic.Amikacin}
            });

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["AMX_SIR"].Should().Be(DBNull.Value);
            export.Rows[0]["AMX_MIC"].Should().Be(DBNull.Value);
            export.Rows[0]["AMC_SIR"].Should().Be(DBNull.Value);
            export.Rows[0]["AMC_MIC"].Should().Be(DBNull.Value);
            export.Rows[0]["CTX_SIR"].Should().Be(DBNull.Value);
            export.Rows[0]["CTX_MIC"].Should().Be(DBNull.Value);
        }

        [Test]
        public void DataTable_ContainsAmxAntibioticSusceptibilityInferredFromAmpicillin()
        {
            var sut = CreateExportDefinition();
            Sending.Isolate.EpsilometerTests.Add(new EpsilometerTest
            {
                Result = EpsilometerTestResult.Resistant,
                Measurement = 1.23f,
                EucastClinicalBreakpoint = new EucastClinicalBreakpoint {Antibiotic = Antibiotic.Ampicillin}
            });

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["AMX_SIR"].Should().Be("R");
            export.Rows[0]["AMX_MIC"].Should().Be(DBNull.Value);
        }

        [TestCase(Gender.Female, "f")]
        [TestCase(Gender.Male, "m")]
        [TestCase(Gender.Intersex, "")]
        [TestCase(Gender.NotStated, "")]
        [TestCase(null, "")]
        public void DataTable_ContainsSexProperties(Gender? gender, string expectedValue)
        {
            var sut = CreateExportDefinition();
            Sending.Patient.Gender = gender;

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["sex"].Should().Be(expectedValue);
        }

        private static HaemophilusPubMlstExport CreateExportDefinition()
        {
            return new HaemophilusPubMlstExport();
        }
    }
}