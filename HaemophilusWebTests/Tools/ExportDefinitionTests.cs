using System.Collections.Generic;
using FluentAssertions;
using HaemophilusWeb.Models;
using HaemophilusWeb.Views.Utils;
using NUnit.Framework;
using Isolate = HaemophilusWeb.TestDoubles.Isolate;
using Patient = HaemophilusWeb.TestDoubles.Patient;

namespace HaemophilusWeb.Tools
{
    public class ExportDefinitionTests
    {
        private readonly List<Isolate> testData = new List<Isolate>
        {
            new Isolate
            {
                SamplingLocation = "Blood",
                Invasive = "Yes",
                Patient = new Patient
                {
                    Initials = "J.D."
                }
            },
            new Isolate
            {
                SamplingLocation = "Liquor",
                Invasive = "No",
                Patient = new Patient
                {
                    Initials = "A.B."
                }
            }
        };

        [Test]
        public void Ctor_DoesNotThrow()
        {
            CreateExportDefinition();
        }

        [Test]
        public void ToDataTable_NoFields_CreatesEmptyTable()
        {
            var exportDefinition = CreateExportDefinition();

            var table = exportDefinition.ToDataTable(new List<Isolate>());

            table.Columns.Count.Should().Be(0);
            table.Rows.Count.Should().Be(0);
        }

        [Test]
        public void ToDataTable_SomeFieldsAndSomeRecord_CreatesTable()
        {
            var exportDefinition = CreateExportDefinition();
            exportDefinition.AddField(p => p.SamplingLocation);
            exportDefinition.AddField(p => p.Patient.Initials.ToLower());

            var table = exportDefinition.ToDataTable(testData);

            table.Columns.Count.Should().Be(2);
            table.Rows.Count.Should().Be(2);

            var firstRow = table.Rows[0];
            firstRow["Entnahmeort"].Should().Be("Blood");
            firstRow["Initials"].Should().Be("j.d.");
        }

        [Test]
        public void ToDataTable_FieldsWithCustomTitle_CreatesTable()
        {
            var exportDefinition = CreateExportDefinition();
            var customHeader = "Invasivität";
            exportDefinition.AddField(p => p.Invasive=="Yes" ? "Invasiv" : "Nicht invasiv" , customHeader);

            var table = exportDefinition.ToDataTable(testData);

            var firstRow = table.Rows[0];
            firstRow[customHeader].Should().Be("Invasiv");
        }

        private static ExportDefinition<Isolate> CreateExportDefinition()
        {
            return new ExportDefinition<Isolate>();
        }
    }
}