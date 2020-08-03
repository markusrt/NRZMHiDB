using System;
using System.Collections.Generic;
using FluentAssertions;
using HaemophilusWeb.TestUtils;
using Newtonsoft.Json;
using NUnit.Framework;

namespace HaemophilusWeb.Tools
{
    class DataTableRecord
    {
        public string isolate { get; set; }
        public string initials { get; set; }
        public DateTime? date_of_birth { get; set; }
        public string sex { get; set; }
        public string postal_code { get; set; }
        public string date_received { get; set; }

        public string source { get; set; }
        public string serotype { get; set; }
        public string beta_lactamase { get; set; }
        public string AMX_SIR { get; set; }
    }

    class DataTableRecordExportDefinition : ExportDefinition<DataTableRecord>
    {
        public DataTableRecordExportDefinition()
        {
            AddField(r => r.isolate);
            AddField(r => r.initials);
            AddField(r => r.date_of_birth);
            AddField(r => r.sex);
            AddField(r => r.postal_code);
            AddField(r => r.date_received);
            AddField(r => r.source);
            AddField(r => r.serotype);
            AddField(r => r.beta_lactamase);
            AddField(r => r.AMX_SIR);
        }
    }

    public class DuplicatePatientResolverTests
    {
        private static readonly PubMlstColumns Col = new PubMlstColumns();
        private DuplicatePatientResolver _sut;

        [SetUp]
        public void Setup()
        {
            _sut = new DuplicatePatientResolver(Col);
        }

        [Test]
        public void WhenDuplicatePatients_ThenIdenticalOlderEntriesStrainsAreRemoved()
        {
            var record1 = CreateRecord();
            var record1BluePrint = JsonConvert.SerializeObject(record1);
            record1.isolate = "H123";
            var record2 = JsonConvert.DeserializeObject<DataTableRecord>(record1BluePrint);
            record2.isolate = "H135";
            var record3 = JsonConvert.DeserializeObject<DataTableRecord>(record1BluePrint);
            record3.isolate = "H137";
            record3.serotype += "different";
            var table = new DataTableRecordExportDefinition().ToDataTable(new List<DataTableRecord> {record1, record2, record3});

            _sut.CleanOrMarkDuplicates(table);

            table.Rows.Should().HaveCount(2);

            table.Rows[0][Col.StemNumber].Should().Be("H123");
            table.Rows[1][Col.StemNumber].Should().Be("H137");
        }

        [Test]
        public void WhenTwoPatientsEachHavingTwoEntries_ThenDuplicateGroupsAreMarked()
        {
            var record1 = CreateRecord();
            var record1BluePrint = JsonConvert.SerializeObject(record1);
            var record3 = JsonConvert.DeserializeObject<DataTableRecord>(record1BluePrint);
            record3.serotype += "different";
            var record2 = JsonConvert.DeserializeObject<DataTableRecord>(record1BluePrint);
            record2.initials = "A.B.";
            var record4 = JsonConvert.DeserializeObject<DataTableRecord>(record1BluePrint);
            record4.initials = "A.B.";
            record4.serotype += "different";
            var table = new DataTableRecordExportDefinition().ToDataTable(new List<DataTableRecord> {record1, record2, record3, record4});

            _sut.CleanOrMarkDuplicates(table);

            table.Rows.Should().HaveCount(4);

            table.Rows[0][DuplicatePatientResolver.ColDuplicateGroup].Should().Be("Group 1");
            table.Rows[2][DuplicatePatientResolver.ColDuplicateGroup].Should().Be("Group 1");
            table.Rows[1][DuplicatePatientResolver.ColDuplicateGroup].Should().Be("Group 2");
            table.Rows[3][DuplicatePatientResolver.ColDuplicateGroup].Should().Be("Group 2");
        }

        [Test]
        public void WhenDuplicatePatients_ThenIdenticalEntriesMoreThenSixMonthApartStrainsAreKept()
        {
            var record1 = CreateRecord();
            var record1BluePrint = JsonConvert.SerializeObject(record1);
            record1.isolate = "H123";
            var record2 = JsonConvert.DeserializeObject<DataTableRecord>(record1BluePrint);
            record2.isolate = "H135";
            record2.date_received = DateTime.Parse(record2.date_received).AddMonths(6).ToString("yyyy-MM-dd");
            var record3 = JsonConvert.DeserializeObject<DataTableRecord>(record1BluePrint);
            record3.isolate = "H137";
            record3.serotype += "different";
            var table = new DataTableRecordExportDefinition().ToDataTable(new List<DataTableRecord> {record1, record2, record3});

            _sut.CleanOrMarkDuplicates(table);

            table.Rows.Should().HaveCount(3);

            table.Rows[0][Col.StemNumber].Should().Be("H123");
            table.Rows[1][Col.StemNumber].Should().Be("H135");
            table.Rows[2][Col.StemNumber].Should().Be("H137");
        }

        [Test]
        public void WhenNonDuplicateEntries_ThenNoRowsAreRemoved()
        {
            var record1 = CreateRecord();
            var record2 = CreateRecord();
            record2.sex = record1.sex;
            record2.initials = record1.initials;
            record2.date_of_birth = record1.date_of_birth;
            var record3 = CreateRecord();
            record3.postal_code = null;
            record3.date_of_birth = null;
            var record4 = CreateRecord();
            record4.sex = record3.sex;
            record4.postal_code = null;
            record4.date_of_birth = null;
            var table = new DataTableRecordExportDefinition().ToDataTable(new List<DataTableRecord> {record1, record2, record3, record4});

            _sut.CleanOrMarkDuplicates(table);

            table.Rows.Should().HaveCount(4);
            table.Columns.Contains(DuplicatePatientResolver.ColDuplicateGroup).Should().BeFalse();
        }

        private DataTableRecord CreateRecord()
        {
            var record = MockData.CreateInstance<DataTableRecord>();
            record.date_received = DateTime.Now.ToString("yyyy-MM-dd");
            return record;
        }


        [Test]
        public void WhenCleanupFinished_PatientColumnsAreRemoved()
        {
            var record1 = CreateRecord();
            var record2 = CreateRecord();
            var table = new DataTableRecordExportDefinition().ToDataTable(new List<DataTableRecord> {record1, record2});

            _sut.CleanOrMarkDuplicates(table);

            table.Rows.Should().HaveCount(2);
            table.Columns.Contains(Col.Initials).Should().BeFalse();
            table.Columns.Contains(Col.DateOfBirth).Should().BeFalse();
            table.Columns.Contains(Col.PostalCode).Should().BeFalse();
        }


        [Test]
        public void WhenEmptyRecords_ThenEntriesAreRemoved()
        {
            var record1 = CreateRecord();
            record1.isolate = "H123";
            record1.beta_lactamase = "";
            record1.AMX_SIR = "";
            var record2 = CreateRecord();
            record2.beta_lactamase = "";
            record2.AMX_SIR = null;
            record2.serotype = "";
            var record3 = CreateRecord();
            record3.beta_lactamase = "";
            record3.AMX_SIR = "";
            record3.serotype = "";

            var table = new DataTableRecordExportDefinition().ToDataTable(new List<DataTableRecord> {record1, record2, record3});

            _sut.CleanOrMarkDuplicates(table);

            table.Rows.Should().HaveCount(1);

            table.Rows[0][Col.StemNumber].Should().Be("H123");
        }
    }
}