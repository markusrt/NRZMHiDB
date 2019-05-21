using FluentAssertions;
using HaemophilusWeb.Models;
using NUnit.Framework;

namespace HaemophilusWeb.Utils
{
    public class ReportFormatterTests
    {
        [Test]
        [TestCase(Evaluation.HaemophilusTypeA, "Haemophilus influenzae")]
        [TestCase(Evaluation.HaemophilusTypeC, "Haemophilus influenzae")]
        [TestCase(Evaluation.HaemophilusParainfluenzae, "H. parainfluenzae")]
        [TestCase(Evaluation.NoHaemophilusSpecies, "keine Haemophilus-Spezies")]
        public void ToReportFormat_Evaluation_ReturnsCorrectString(Evaluation evaluation, string expected)
        {
            evaluation.ToReportFormat().Should().Be(expected);
        }

        [Test]
        public void ToLaboratoryNumber_ValidInput_TruncatesYearToDecimal()
        {
            ReportFormatter.ToLaboratoryNumber(10, 2014).Should().Be("010/14");
            ReportFormatter.ToLaboratoryNumber(1, 2008).Should().Be("001/08");
        }

        [TestCase("", DatabaseType.Meningococci, "MZ")]
        [TestCase(null, DatabaseType.Meningococci, "MZ -")]
        [TestCase("01/2019", DatabaseType.Meningococci, "MZ01/2019")]
        [TestCase("01/2019", DatabaseType.Haemophilus, "KL01/2019")]
        [TestCase("-01/2019", DatabaseType.Meningococci, "NR01/2019")]
        public void ToLaboratoryNumberWithPrefix_ValidInput_AddsPrefix(string input, DatabaseType databaseType, string expected)
        {
            input.ToLaboratoryNumberWithPrefix(databaseType).Should().Be(expected);
        }

        [TestCase(0, DatabaseType.Meningococci, "DE0")]
        [TestCase(null, DatabaseType.Meningococci, "DE -")]
        [TestCase(1234, DatabaseType.Meningococci, "DE1234")]
        [TestCase(1234, DatabaseType.Haemophilus, "H1234")]
        public void ToStemNumberWithPrefix_ValidInput_AddsPrefix(int? input, DatabaseType databaseType, string expected)
        {
            input.ToStemNumberWithPrefix(databaseType).Should().Be(expected);
        }
    }
}