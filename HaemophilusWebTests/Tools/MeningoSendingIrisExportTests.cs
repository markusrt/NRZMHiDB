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
    public class MeningoSendingIrisExportTests
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

            export.Columns.Count.Should().Be(26);
        }

        [Test]
        public void DataTable_DatesAreFormattedCorrectly()
        {
            var sut = CreateExportDefinition();

            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["Eingangsdatum"].ToString().Should().Match("??.??.????");
            export.Rows[0]["Entnahmedatum"].ToString().Should().Match("??.??.????");
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

            export.Rows[0]["Patientenalter bei Entnahme"].Should().Be(expectedYearsOld);
            export.Rows[0]["Patientenalter bei Entnahme (Monate)"].Should().Be(expectedMonthsOld);
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
        }

        [Test]
        public void DataTable_ContainsPorAAndFetA()
        {
            var sut = CreateExportDefinition();
            Sending.Isolate.PorAPcr = NativeMaterialTestResult.Positive;
            Sending.Isolate.FetAPcr = NativeMaterialTestResult.Positive;
            Sending.Isolate.PorAVr1 = "17";
            Sending.Isolate.PorAVr2 = "16-3";
            Sending.Isolate.FetAVr = "1-20";
            
            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["PorA-VR1"].Should().Be("17");
            export.Rows[0]["PorA-VR2"].Should().Be("16-3");
            export.Rows[0]["FetA-VR"].Should().Be("1-20");
        }

        [Test]
        public void DataTable_DoesNotContainsEmptyPorAAndFetA()
        {
            var sut = CreateExportDefinition();
            Sending.Isolate.PorAPcr = NativeMaterialTestResult.NotDetermined;
            Sending.Isolate.FetAPcr = NativeMaterialTestResult.NotDetermined;
            
            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["PorA-VR1"].Should().Be(DBNull.Value);
            export.Rows[0]["PorA-VR2"].Should().Be(DBNull.Value);
            export.Rows[0]["FetA-VR"].Should().Be(DBNull.Value);
        }

        [Test]
        public void DataTable_DoesContainsNegativePorAAndFetA()
        {
            var sut = CreateExportDefinition();
            Sending.Isolate.PorAPcr = NativeMaterialTestResult.Negative;
            Sending.Isolate.FetAPcr = NativeMaterialTestResult.Negative;
            
            var export = sut.ToDataTable(Sendings);

            export.Rows[0]["PorA-VR1"].Should().Be("negativ");
            export.Rows[0]["PorA-VR2"].Should().Be("negativ");
            export.Rows[0]["FetA-VR"].Should().Be("negativ");
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

        private static MeningoSendingIrisExport CreateExportDefinition()
        {
            return new MeningoSendingIrisExport();
        }
    }
}