using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using HaemophilusWeb.Models;
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
            
            export.Columns.Count.Should().Be(22);
        }

        [Test]
        public void DataTable_ContainsOnlyPayloadColumnsAfterCleanup()
        {
            var duplicatePatientResolver = new DuplicatePatientResolver(new RkiExportColumns());
            var sut = CreateExportDefinition();

            var export = sut.ToDataTable(Sendings);
            
            duplicatePatientResolver.RemovePatientData(export);
            export.Columns.Count.Should().Be(19);
        }

        [Test]
        public void DataTable_ContainsRkiColumns()
        {
            var sut = CreateExportDefinition();
            var county = Counties.First();

            Sending.SamplingLocation = SamplingLocation.Other;
            Sending.OtherSamplingLocation = "Other Location";
            Sending.Patient.BirthDate = new DateTime(2005, 8, 31);
            Sending.Patient.County = county.Name;
            Sending.Patient.State = State.BB;
            Sending.Patient.Gender = Gender.Female;
            Sending.Patient.HibVaccination = VaccinationStatus.Yes;
            Sending.Isolate.Evaluation = Evaluation.HaemophilusTypeA;
            Sending.Isolate.BetaLactamase = TestResult.Negative;

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
        }

        private HaemophilusSendingRkiExport CreateExportDefinition()
        {
            return new HaemophilusSendingRkiExport(Counties);
        }
    }
}