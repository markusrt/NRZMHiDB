using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Principal;
using FluentAssertions;
using HaemophilusWeb.TestUtils;
using NUnit.Framework;
using static HaemophilusWeb.Tools.PubMlstColumns;

namespace HaemophilusWeb.Tools
{
    class DataTableRecord
    {
        public string isolate { get; set; }
        public string initials { get; set; }
        public DateTime date_of_birth { get; set; }
        public string sex { get; set; }
        public string postal_code { get; set; }

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
            AddField(r => r.serotype);
            AddField(r => r.beta_lactamase);
            AddField(r => r.AMX_SIR);
        }
    }

    public class PubMlstDuplicateResolverTests
    {
        [Test]
        public void WhenDuplicatePatients_ThenOlderEntriesAreRemoved()
        {
            var record1 = MockData.CreateInstance<DataTableRecord>();
            record1.isolate = "H123";
            var record2 = MockData.CreateInstance<DataTableRecord>();
            record2.isolate = "H135";
            record1.initials = record2.initials;
            record1.date_of_birth = record2.date_of_birth;
            record1.sex = record2.sex;
            record1.postal_code = record2.postal_code;
            var table = new DataTableRecordExportDefinition().ToDataTable(new List<DataTableRecord> {record1, record2});

            PubMlstDuplicateResolver.CleanOrMarkDuplicates(table);

            table.Rows.Should().HaveCount(1);

            table.Rows[0][ColIsolate].Should().Be("H123");
        }

        

        [Test]
        public void WhenNonDuplicateEntries_ThenNoRowsAreRemoved()
        {
            var record1 = MockData.CreateInstance<DataTableRecord>();
            var record2 = MockData.CreateInstance<DataTableRecord>();
            record2.sex = record1.sex;
            record2.initials = record1.initials;
            record2.date_of_birth = record1.date_of_birth;
            var record3 = MockData.CreateInstance<DataTableRecord>();
            record3.date_of_birth = record3.date_of_birth.AddDays(10);
            record3.postal_code = null;
            record3.initials = null;
            var record4 = MockData.CreateInstance<DataTableRecord>();
            record4.sex = record3.sex;
            record4.postal_code = null;
            record4.initials = null;
            var table = new DataTableRecordExportDefinition().ToDataTable(new List<DataTableRecord> {record1, record2, record3, record4});

            PubMlstDuplicateResolver.CleanOrMarkDuplicates(table);

            table.Rows.Should().HaveCount(4);
        }



        [Test]
        public void WhenEmptyRecords_ThenEntriesAreRemoved()
        {
            var record1 = MockData.CreateInstance<DataTableRecord>();
            record1.isolate = "H123";
            record1.beta_lactamase = "";
            record1.AMX_SIR = "";
            var record2 = MockData.CreateInstance<DataTableRecord>();
            record2.beta_lactamase = "";
            record2.AMX_SIR = null;
            record2.serotype = "";
            var record3 = MockData.CreateInstance<DataTableRecord>();
            record3.beta_lactamase = "";
            record3.AMX_SIR = "";
            record3.serotype = "";

            var table = new DataTableRecordExportDefinition().ToDataTable(new List<DataTableRecord> {record1, record2, record3});

            PubMlstDuplicateResolver.CleanOrMarkDuplicates(table);

            table.Rows.Should().HaveCount(1);

            table.Rows[0][ColIsolate].Should().Be("H123");
        }
    }
}