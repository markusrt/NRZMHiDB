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
        [TestCase(Evaluation.HaemophilusParainfluenzae, "H.parainfluenzae")]
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
    }
}