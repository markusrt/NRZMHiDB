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

            table.Rows.Count.Should().Be(2);

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

            table.Rows.Count.Should().Be(4);

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

            table.Rows.Count.Should().Be(3);

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

            table.Rows.Count.Should().Be(4);
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

            table.Rows.Count.Should().Be(2);
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

            table.Rows.Count.Should().Be(1);

            table.Rows[0][Col.StemNumber].Should().Be("H123");
        }

        // specs/012 FR-006, US1.3, FR-004, US1.7: an identical strain received a few days later is dropped (under
        // both readings of the window, DEC-009); the single remaining entry gets no duplicate group.
        [Test]
        public void WhenIdenticalStrainReceivedTenDaysLater_ThenLaterEntryIsDroppedAndNoGroupIsMarked()
        {
            var first = CreateFixedRecord("H201", "2020-03-02");
            var second = CreateFixedRecord("H202", "2020-03-12");
            var table = ToTable(first, second);

            _sut.CleanOrMarkDuplicates(table);

            table.Rows.Count.Should().Be(1);
            table.Rows[0][Col.StemNumber].Should().Be("H201");
            table.Columns.Contains(DuplicatePatientResolver.ColDuplicateGroup).Should().BeFalse();
        }

        // specs/012 FR-006, US1.5, US1.6: every strain characteristic, including the sampling location, takes part
        // in the comparison; a difference in one of them keeps both entries, which then form one duplicate group.
        [TestCase("beta_lactamase")]
        [TestCase("AMX_SIR")]
        [TestCase("serotype")]
        [TestCase("source")]
        public void WhenOneStrainCharacteristicDiffers_ThenBothEntriesAreKeptAndGrouped(string characteristic)
        {
            var first = CreateFixedRecord("H211", "2020-03-02");
            var second = CreateFixedRecord("H212", "2020-03-02");
            typeof(DataTableRecord).GetProperty(characteristic).SetValue(second, "different");
            var table = ToTable(first, second);

            _sut.CleanOrMarkDuplicates(table);

            table.Rows.Count.Should().Be(2);
            table.Rows[0][DuplicatePatientResolver.ColDuplicateGroup].Should().Be("Group 1");
            table.Rows[1][DuplicatePatientResolver.ColDuplicateGroup].Should().Be("Group 1");
        }

        // specs/012 FR-007: a kept identical strain becomes the new reference. The second entry (seven months later)
        // is kept; the third (ten days after the second, more than seven months after the first) is dropped, so it
        // was compared with the second entry, not with the first.
        [Test]
        public void WhenIdenticalStrainIsKept_ThenItBecomesTheReferenceForLaterEntries()
        {
            var first = CreateFixedRecord("H221", "2020-01-10");
            var second = CreateFixedRecord("H222", "2020-08-10");
            var third = CreateFixedRecord("H223", "2020-08-20");
            var table = ToTable(first, second, third);

            _sut.CleanOrMarkDuplicates(table);

            table.Rows.Count.Should().Be(2);
            table.Rows[0][Col.StemNumber].Should().Be("H221");
            table.Rows[1][Col.StemNumber].Should().Be("H222");
        }

        // specs/012 FR-003, Edge Cases (exact text match): initials that differ only in case are different patients.
        [Test]
        public void WhenInitialsDifferOnlyInCase_ThenEntriesBelongToDifferentPatients()
        {
            var first = CreateFixedRecord("H231", "2020-03-02");
            var second = CreateFixedRecord("H232", "2020-03-02");
            second.initials = "e.m.";
            first.initials = "E.M.";
            var table = ToTable(first, second);

            _sut.CleanOrMarkDuplicates(table);

            table.Rows.Count.Should().Be(2);
            table.Columns.Contains(DuplicatePatientResolver.ColDuplicateGroup).Should().BeFalse();
        }

        // specs/012 FR-008, US1.6: groups are numbered from 1 in order of the patients' first rows; rows of patients
        // with a single remaining entry stay empty in the group column.
        [Test]
        public void WhenOnlyOnePatientHasSeveralEntries_ThenOtherRowsHaveAnEmptyGroupCell()
        {
            var single = CreateFixedRecord("H241", "2020-03-02");
            single.initials = "S.I.";
            var first = CreateFixedRecord("H242", "2020-03-02");
            var second = CreateFixedRecord("H243", "2020-03-02");
            second.serotype = "different";
            var table = ToTable(single, first, second);

            _sut.CleanOrMarkDuplicates(table);

            table.Rows.Count.Should().Be(3);
            table.Columns[table.Columns.Count - 1].ColumnName.Should().Be(DuplicatePatientResolver.ColDuplicateGroup);
            table.Rows[0][DuplicatePatientResolver.ColDuplicateGroup].Should().Be(DBNull.Value);
            table.Rows[1][DuplicatePatientResolver.ColDuplicateGroup].Should().Be("Group 1");
            table.Rows[2][DuplicatePatientResolver.ColDuplicateGroup].Should().Be("Group 1");
        }

        // specs/012 FR-002, FR-009, US2.1-US2.3: the unadjusted mode removes the three identifier columns only;
        // every row stays and no group column is added.
        [Test]
        public void RemovePatientData_KeepsAllRowsAndAddsNoGroupColumn()
        {
            var first = CreateFixedRecord("H251", "2020-03-02");
            var second = CreateFixedRecord("H252", "2020-03-02");
            var table = ToTable(first, second);
            var columnsBefore = table.Columns.Count;

            _sut.RemovePatientData(table);

            table.Rows.Count.Should().Be(2);
            table.Columns.Count.Should().Be(columnsBefore - 3);
            table.Columns.Contains(Col.Initials).Should().BeFalse();
            table.Columns.Contains(Col.DateOfBirth).Should().BeFalse();
            table.Columns.Contains(Col.PostalCode).Should().BeFalse();
            table.Columns.Contains(DuplicatePatientResolver.ColDuplicateGroup).Should().BeFalse();
        }

        // specs/012 Edge Cases (empty selection): header only, identifier columns removed, no group column.
        [Test]
        public void WhenSelectionIsEmpty_ThenOnlyTheReducedHeaderRemains()
        {
            var table = ToTable();

            _sut.CleanOrMarkDuplicates(table);

            table.Rows.Count.Should().Be(0);
            table.Columns.Contains(Col.Initials).Should().BeFalse();
            table.Columns.Contains(Col.DateOfBirth).Should().BeFalse();
            table.Columns.Contains(Col.PostalCode).Should().BeFalse();
            table.Columns.Contains(DuplicatePatientResolver.ColDuplicateGroup).Should().BeFalse();
        }

        private static DataTableRecord CreateFixedRecord(string isolate, string dateReceived)
        {
            return new DataTableRecord
            {
                isolate = isolate,
                initials = "X.Y.",
                date_of_birth = new DateTime(2004, 3, 2),
                sex = "f",
                postal_code = "97000",
                date_received = dateReceived,
                source = "blood",
                serotype = "NT",
                beta_lactamase = "negative",
                AMX_SIR = "S"
            };
        }

        private static System.Data.DataTable ToTable(params DataTableRecord[] records)
        {
            return new DataTableRecordExportDefinition().ToDataTable(new List<DataTableRecord>(records));
        }
    }
}