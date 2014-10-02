using System;
using FluentAssertions;
using HaemophilusWeb.Models;
using NUnit.Framework;

namespace HaemophilusWeb.Domain
{
    public class IsolateInterpretationTests
    {
        [Test]
        public void Interpret_EmptyIsolate_IsUndetermined()
        {
            var isolate = new IsolateBase();

            var interpretation = IsolateInterpretation.Interpret(isolate);

            interpretation.Interpretation.Should().Contain("Diskrepante Ergebnisse");
            interpretation.InterpretationDisclaimer.Should().BeEmpty();
        }

        [Test]
        [TestCase("a")]
        [TestCase("b")]
        [TestCase("c")]
        [TestCase("d")]
        [TestCase("e")]
        [TestCase("f")]
        public void Interpret_DistinctSerotypeWithPositiveBexA_IsSpecificTyping(string serotypeString)
        {
            var serotypePcr = (SerotypePcr) Enum.Parse(typeof (SerotypePcr), serotypeString.ToUpper());
            var serotypeAgg = (SerotypeAgg) Enum.Parse(typeof (SerotypeAgg), serotypeString.ToUpper());
            var isolate = new IsolateBase
            {
                SerotypePcr = serotypePcr,
                Agglutination = serotypeAgg,
                BexA = TestResult.Positive
            };

            var interpretation = IsolateInterpretation.Interpret(isolate);

            var expectedInterpretation = string.Format("Infektion mit Haemophilus influenzae des Serotyp {0} (Hi{0}).",
                serotypeString);
            var expectedDisclaimer = string.Format(
                "Meldekategorie dieses Befundes: Haemophilus influenzae, Serotyp {0}", serotypeString);
            interpretation.Interpretation.Should().Contain(expectedInterpretation);
            interpretation.InterpretationDisclaimer.Should().Contain(expectedDisclaimer);
        }

        [Test]
        [TestCase("a")]
        [TestCase("b")]
        [TestCase("c")]
        [TestCase("d")]
        [TestCase("e")]
        [TestCase("f")]
        public void Interpret_NegativeAgglutinationButDistinctSerotypeWithNegativeBexA_IsSpecificTyping(
            string serotypeString)
        {
            var serotypePcr = (SerotypePcr) Enum.Parse(typeof (SerotypePcr), serotypeString.ToUpper());
            var isolate = new IsolateBase
            {
                SerotypePcr = serotypePcr,
                Agglutination = SerotypeAgg.Negative,
                BexA = TestResult.Negative
            };

            var interpretation = IsolateInterpretation.Interpret(isolate);

            var expectedInterpretation = string.Format("Kapsellocus für Polysaccharide des Serotyps {0}", serotypeString);
            interpretation.Interpretation.Should().Contain(expectedInterpretation);
            interpretation.InterpretationDisclaimer.Should()
                .Contain("Meldekategorie dieses Befundes: Haemophilus influenzae, unbekapselt.");
        }

        [Test]
        public void Interpret_OnlyNegativeAgglutination_TypingNotEfficient()
        {
            var isolate = new IsolateBase
            {
                SerotypePcr = SerotypePcr.NotDetermined,
                Agglutination = SerotypeAgg.Negative,
                BexA = TestResult.NotDetermined
            };

            var interpretation = IsolateInterpretation.Interpret(isolate);

            interpretation.Interpretation.Should().Contain("aus epidemiologischen und Kostengründen nicht durchgeführt");
        }
    }
}