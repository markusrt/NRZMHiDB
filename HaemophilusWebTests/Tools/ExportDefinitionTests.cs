using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace HaemophilusWeb.Tools
{
    public class ExportDefinitionTests
    {
        [Test]
        public void Ctor_DoesNotThrow()
        {
            var sut = CreateExportDefinition();

            sut.Should().NotBeNull();
        }

        [Test]
        public void ToDataTable_IncludesSingleEntryAndField()
        {
            var persons = new List<Person>
            {
                new Person
                {
                    HeightInCentimeters = 185, 
                    BirthDate = new DateTime(1980, 10, 10),
                    Name = "Test"
                }
            };
            var sut = CreateExportDefinition();
            sut.AddField(person => person.HeightInCentimeters);

            var dataTable = sut.ToDataTable(persons);

            dataTable.Rows.Count.Should().Be(1);
            dataTable.Columns.Count.Should().Be(1);
            dataTable.Rows[0][0].ToString().Should().Be("185");
        }

        [Test]
        public void ToDataTable_IncludesMultipleEntriesAndFields()
        {
            var persons = new List<Person>
            {
                new Person
                {
                    HeightInCentimeters = 185,
                    BirthDate = new DateTime(1980, 10, 10),
                    Name = "Test 1"
                },
                new Person
                {
                    HeightInCentimeters = 175,
                    BirthDate = new DateTime(1982, 11, 23),
                    Name = "Test 2"
                }
            };
            var sut = CreateExportDefinition();
            sut.AddField(person => person.HeightInCentimeters);
            sut.AddField(person => person.BirthDate);
            sut.AddField(person => person.Name);

            var dataTable = sut.ToDataTable(persons);

            dataTable.Rows.Count.Should().Be(2);
            dataTable.Columns.Count.Should().Be(3);

            dataTable.Rows[1][0].ToString().Should().Be("175");
            dataTable.Rows[1][1].Should().Be(new DateTime(1982, 11, 23));
            dataTable.Rows[1][2].ToString().Should().Be("Test 2");
        }


        [Test]
        public void ToDataTable_EnumsAreConvertedToStrings()
        {
            var persons = new List<Person>
            {
                new Person
                {
                    AccessLevel = FileAccess.Read
                }
            };
            var sut = CreateExportDefinition();
            sut.AddField(person => person.AccessLevel);

            var dataTable = sut.ToDataTable(persons);

            dataTable.Rows.Count.Should().Be(1);
            dataTable.Columns.Count.Should().Be(1);

            dataTable.Rows[0][0].Should().Be("Read");
        }

        private ExportDefinition<Person> CreateExportDefinition()
        {
            return new ExportDefinition<Person>();
        }
    }

    internal class Person
    {
        public string Name { get; set; }
        public DateTime BirthDate { get; set; }
        public int HeightInCentimeters { get; set; }
        public FileAccess AccessLevel { get; set; }
    }
}