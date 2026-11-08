using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace HaemophilusWeb.Tools
{
    public class CreateExcelFileTests
    {
        // specs/012 Edge Cases (error inside duplicate handling): an exception thrown while the table is
        // post-processed is not caught, so the export action fails and no spreadsheet is written.
        [Test]
        public void CreateExcelDocument_WhenPostProcessingThrows_ExceptionPropagatesAndNoFileIsWritten()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xlsx");
            var records = new List<DataTableRecord> {new DataTableRecord {isolate = "H1"}};

            try
            {
                Action act = () => CreateExcelFile.CreateExcelDocument(records, new DataTableRecordExportDefinition(),
                    path, table => throw new InvalidOperationException("post-processing failed"));

                act.Should().Throw<InvalidOperationException>();
                File.Exists(path).Should().BeFalse();
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}
