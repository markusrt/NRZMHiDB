using System;
using FluentAssertions;
using HaemophilusWeb.Models;
using NUnit.Framework;

namespace HaemophilusWeb.Domain
{
    public class IsolateInterpretationTests
    {
        private readonly IsolateInterpretation isolateInterpretation = new IsolateInterpretation();

        [Test]
        public void Interpret_EmptyIsolate_IsUndetermined()
        {
            var isolate = new Isolate();

            var interpretation = isolateInterpretation.Interpret(isolate);

            interpretation.Interpretation.Should().Contain("Diskrepante Ergebnisse");
            interpretation.InterpretationDisclaimer.Should().BeEmpty();
        }

        [Test]
        [TestCase(SerotypeAgg.A, SerotypePcr.A)]
        [TestCase(SerotypeAgg.B, SerotypePcr.B)]
        [TestCase(SerotypeAgg.C, SerotypePcr.C)]
        [TestCase(SerotypeAgg.D, SerotypePcr.D)]
        [TestCase(SerotypeAgg.E, SerotypePcr.E)]
        [TestCase(SerotypeAgg.F, SerotypePcr.F)]
        [TestCase(SerotypeAgg.A, SerotypePcr.NotDetermined)]
        [TestCase(SerotypeAgg.B, SerotypePcr.NotDetermined)]
        [TestCase(SerotypeAgg.C, SerotypePcr.NotDetermined)]
        [TestCase(SerotypeAgg.D, SerotypePcr.NotDetermined)]
        [TestCase(SerotypeAgg.E, SerotypePcr.NotDetermined)]
        [TestCase(SerotypeAgg.F, SerotypePcr.NotDetermined)]
        public void
            Interpret_DistinctAgglutinationWithPositiveBexA_EitherMatchingOrNotDeterminedSerotype_IsSpecificTyping(
            SerotypeAgg serotypeAgg,
            SerotypePcr serotypePcr)
        {
            var serotypeString = serotypeAgg.ToString().ToLower();
            var isolate = new Isolate
            {
                SerotypePcr = serotypePcr,
                Agglutination = serotypeAgg,
                BexA = TestResult.Positive,
                Sending = new Sending {SamplingLocation = SamplingLocation.Blood}
            };

            var interpretation = isolateInterpretation.Interpret(isolate);

            var expectedInterpretation = string.Format("Die Ergebnisse sprechen für eine Infektion mit Haemophilus influenzae des Serotyp {0} (Hi{0}).",
                serotypeString);
            interpretation.Interpretation.Should().Be(expectedInterpretation);
            interpretation.InterpretationPreliminary.Should().Contain("Diskrepante");
            interpretation.InterpretationDisclaimer.Should().Contain($"Meldekategorie dieses Befundes: Haemophilus influenzae, Serotyp {serotypeString}");
            interpretation.InterpretationDisclaimer.Should().Contain("Der direkte Nachweis von Haemophilus influenzae aus Blut oder Liquor ist nach §7 IfSG meldepflichtig.");
        }

        [Test]
        [TestCase(SerotypeAgg.A, SerotypePcr.NotDetermined)]
        [TestCase(SerotypeAgg.B, SerotypePcr.NotDetermined)]
        [TestCase(SerotypeAgg.C, SerotypePcr.NotDetermined)]
        [TestCase(SerotypeAgg.D, SerotypePcr.NotDetermined)]
        [TestCase(SerotypeAgg.E, SerotypePcr.NotDetermined)]
        [TestCase(SerotypeAgg.F, SerotypePcr.NotDetermined)]
        public void
            Interpret_DistinctAgglutinationWithNotDeterminedBexA_NotDeterminedSerotype_IsSpecificPreliminaryTyping(
                SerotypeAgg serotypeAgg,
                SerotypePcr serotypePcr)
        {
            var serotypeString = serotypeAgg.ToString().ToLower();
            var isolate = new Isolate
            {
                SerotypePcr = serotypePcr,
                Agglutination = serotypeAgg,
                BexA = TestResult.NotDetermined,
                Sending = new Sending {SamplingLocation = SamplingLocation.Blood}
            };

            var interpretation = isolateInterpretation.Interpret(isolate);

            var expectedInterpretationPreliminary = string.Format("Das Ergebnis spricht für eine Infektion mit Haemophilus influenzae des Serotyp {0} (Hi{0}).",
                serotypeString);
            interpretation.Interpretation.Should().Contain("Diskrepante");
            interpretation.InterpretationPreliminary.Should().Be(expectedInterpretationPreliminary);
            interpretation.InterpretationDisclaimer.Should().Contain($"Meldekategorie dieses Befundes: Haemophilus influenzae, Serotyp {serotypeString}");
            interpretation.InterpretationDisclaimer.Should().Contain("Der direkte Nachweis von Haemophilus influenzae aus Blut oder Liquor ist nach §7 IfSG meldepflichtig.");
        }

        [Test]
        public void
            Interpret_DistinctAgglutinationWithNotDeterminedBexA_NotDeterminedSerotype_NonInvasive_IsSpecificPreliminaryTypingWithEmptyDisclaimer()
        {
            var serotypeString = SerotypeAgg.E.ToString().ToLower();
            var isolate = new Isolate
            {
                SerotypePcr = SerotypePcr.NotDetermined,
                Agglutination = SerotypeAgg.E,
                BexA = TestResult.NotDetermined,
                Sending = new Sending {SamplingLocation = SamplingLocation.OtherNonInvasive}
            };

            var interpretation = isolateInterpretation.Interpret(isolate);

            var expectedInterpretationPreliminary = string.Format("Das Ergebnis spricht für eine Infektion mit Haemophilus influenzae des Serotyp {0} (Hi{0}).",
                serotypeString);
            interpretation.Interpretation.Should().Contain("Diskrepante");
            interpretation.InterpretationPreliminary.Should().Be(expectedInterpretationPreliminary);
            interpretation.InterpretationDisclaimer.Should().Be("Eine molekularbiologische Typisierung und Resistenztestungen werden bei nicht-invasiven Isolaten aus epidemiologischen und Kostengründen nicht durchgeführt.");
            interpretation.InterpretationDisclaimer.Should().NotContain("Der direkte Nachweis von Haemophilus influenzae aus Blut oder Liquor ist nach §7 IfSG meldepflichtig.");
        }

        [Test]
        [TestCase(SerotypePcr.A)]
        [TestCase(SerotypePcr.B)]
        [TestCase(SerotypePcr.C)]
        [TestCase(SerotypePcr.D)]
        [TestCase(SerotypePcr.E)]
        [TestCase(SerotypePcr.F)]
        public void Interpret_NegativeAgglutinationButDistinctSerotypeWithNegativeBexA_IsSpecificTyping(
            SerotypePcr serotypePcr)
        {
            var serotypeString = serotypePcr.ToString().ToLower();
            var isolate = new Isolate
            {
                SerotypePcr = serotypePcr,
                Agglutination = SerotypeAgg.Negative,
                BexA = TestResult.Negative,
                Sending = new Sending {SamplingLocation = SamplingLocation.Blood}
            };

            var interpretation = isolateInterpretation.Interpret(isolate);
            var expectedInterpretation = $"Die Ergebnisse sprechen für einen phänotpischen nicht-typisierbaren Haemophilus influenzae (NTHi). Ein vorhandener genetischer Kapsellocus für Polysaccharide des Serotyps {serotypeString} wird nicht exprimiert.";
            interpretation.Interpretation.Should().Be(expectedInterpretation);
            interpretation.InterpretationPreliminary.Should().Contain("Diskrepante");
            interpretation.InterpretationDisclaimer.Should().Contain("Meldekategorie dieses Befundes: Haemophilus influenzae, unbekapselt.");
            interpretation.InterpretationDisclaimer.Should().Contain("Der direkte Nachweis von Haemophilus influenzae aus Blut oder Liquor ist nach §7 IfSG meldepflichtig.");
        }

        [Test]
        public void Interpret_OnlyNegativeAgglutination_TypingNotEfficient()
        {
            var isolate = new Isolate
            {
                SerotypePcr = SerotypePcr.NotDetermined,
                Agglutination = SerotypeAgg.Negative,
                BexA = TestResult.NotDetermined,
                Sending = new Sending {SamplingLocation = SamplingLocation.Blood}
            };

            var interpretation = isolateInterpretation.Interpret(isolate);

            var expectedInterpretation = "Das Ergebnis spricht für einen unbekapselten Haemophilus influenzae (sog. \"nicht-typisierbarer\" H. influenzae, NTHi).";
            interpretation.Interpretation.Should().Be(expectedInterpretation + " Eine molekularbiologische Typisierung wurde aus epidemiologischen und Kostengründen nicht durchgeführt.");
            interpretation.InterpretationPreliminary.Should().Be(expectedInterpretation);
        }

        [Test]
        [TestCase(SerotypePcr.Negative)]
        [TestCase(SerotypePcr.NotDetermined)]
        public void Interpret_BexaAndAgglutinationIsNegative_WithRespectiveSeroType_NotTypable(SerotypePcr serotype)
        {
            var isolate = new Isolate
            {
                SerotypePcr = serotype,
                Agglutination = SerotypeAgg.Negative,
                BexA = TestResult.Negative,
                Sending = new Sending {SamplingLocation = SamplingLocation.Blood}
            };

            var interpretation = isolateInterpretation.Interpret(isolate);

            interpretation.Interpretation.Should().Contain("nicht-typisierbar");
            interpretation.InterpretationPreliminary.Should().Contain("Diskrepante");
            interpretation.InterpretationDisclaimer.Should()
                .Contain("Meldekategorie dieses Befundes: Haemophilus influenzae, unbekapselt.");
            interpretation.InterpretationDisclaimer.Should().Contain("Der direkte Nachweis von Haemophilus influenzae aus Blut oder Liquor ist nach §7 IfSG meldepflichtig.");

        }

        [TestCase(SerotypePcr.Negative)]
        [TestCase(SerotypePcr.NotDetermined)]
        public void Interpret_BexaAndAgglutinationIsNegative_WithRespectiveSeroType_NonInvasive_NotTypable(SerotypePcr serotype)
        {
            var isolate = new Isolate
            {
                SerotypePcr = serotype,
                Agglutination = SerotypeAgg.Negative,
                BexA = TestResult.Negative,
                Sending = new Sending {SamplingLocation = SamplingLocation.OtherNonInvasive}
            };

            var interpretation = isolateInterpretation.Interpret(isolate);

            interpretation.Interpretation.Should().Contain("nicht-typisierbar");
            interpretation.InterpretationPreliminary.Should().Contain("Diskrepante");
            interpretation.InterpretationDisclaimer.Should().Be("Eine molekularbiologische Typisierung und Resistenztestungen werden bei nicht-invasiven Isolaten aus epidemiologischen und Kostengründen nicht durchgeführt.");

        }

        [Test]
        public void Interpret_AgglutinationBexAAndSerotypAreNotDeterminedButGrowthIsYes_WithRespectiveSeroType_RequestForResend()
        {
            var isolate = new Isolate
            {
                SerotypePcr = SerotypePcr.NotDetermined,
                Agglutination = SerotypeAgg.NotDetermined,
                BexA = TestResult.NotDetermined,
                Growth = YesNoOptional.Yes
            };

            var interpretation = isolateInterpretation.Interpret(isolate);

            interpretation.Interpretation.Should().Contain("Um Wiedereinsendung wird gebeten");
            interpretation.InterpretationPreliminary.Should().Contain("Diskrepante");
            interpretation.InterpretationDisclaimer.Should()
                .Contain("Eine telefonische Vorabmitteilung ist erfolgt.");
        }

        [Test]
        public void Interpret_AgglutinationBexAAndSerotypAreNotDetermined_ButGrowthIsNo_ReportEvaluation()
        {
            var isolate = new Isolate
            {
                SerotypePcr = SerotypePcr.NotDetermined,
                Agglutination = SerotypeAgg.NotDetermined,
                BexA = TestResult.NotDetermined,
                Growth = YesNoOptional.No,
                Evaluation = Evaluation.HaemophilusSpeciesNoHaemophilusInfluenzae
            };

            var interpretation = isolateInterpretation.Interpret(isolate);

            interpretation.Interpretation.Should().Contain("Kein Nachweis von Haemophilus influenzae.");
            interpretation.InterpretationPreliminary.Should().Contain("Diskrepante");
            interpretation.InterpretationDisclaimer.Should()
                .Contain("Beim eingesendeten Isolat handelt es sich am ehesten um Haemophilus sp., nicht H. influenzae.");
        }

        private const string Discrepant = "Diskrepante Ergebnisse, bitte Datenbankeinträge kontrollieren.";

        private const string NonInvasiveDisclaimer =
            "Eine molekularbiologische Typisierung und Resistenztestungen werden bei nicht-invasiven Isolaten aus epidemiologischen und Kostengründen nicht durchgeführt.";

        private static string InvasiveDisclaimer(string category) =>
            $"Der direkte Nachweis von Haemophilus influenzae aus Blut oder Liquor ist nach §7 IfSG meldepflichtig. Meldekategorie dieses Befundes: {category}.";

        private static Isolate CreateIsolate(SerotypeAgg agglutination, TestResult bexA, SerotypePcr serotypePcr,
            SamplingLocation samplingLocation = SamplingLocation.Blood, YesNoOptional growth = YesNoOptional.NotStated)
        {
            return new Isolate
            {
                Agglutination = agglutination,
                BexA = bexA,
                SerotypePcr = serotypePcr,
                Growth = growth,
                Sending = new Sending { SamplingLocation = samplingLocation }
            };
        }

        // specs/006 FR-003: a new isolate starts with typing "not determined" and growth "not stated"
        [Test]
        public void NewIsolate_StartsWithTypingNotDeterminedAndGrowthNotStated()
        {
            var isolate = new Isolate();

            isolate.Agglutination.Should().Be(SerotypeAgg.NotDetermined);
            isolate.BexA.Should().Be(TestResult.NotDetermined);
            isolate.SerotypePcr.Should().Be(SerotypePcr.NotDetermined);
            isolate.Growth.Should().Be(YesNoOptional.NotStated);
        }

        // specs/006 FR-002, FR-003: fields other than agglutination, bexA, serotype PCR, growth, evaluation and
        // sampling location do not influence the result
        [TestCase(SerotypeAgg.Negative, TestResult.Negative, SerotypePcr.B, YesNoOptional.Yes)]
        [TestCase(SerotypeAgg.C, TestResult.Positive, SerotypePcr.C, YesNoOptional.No)]
        [TestCase(SerotypeAgg.D, TestResult.NotDetermined, SerotypePcr.NotDetermined, YesNoOptional.Yes)]
        [TestCase(SerotypeAgg.NotDetermined, TestResult.NotDetermined, SerotypePcr.NotDetermined, YesNoOptional.No)]
        [TestCase(SerotypeAgg.Poly, TestResult.Positive, SerotypePcr.A, YesNoOptional.Yes)]
        public void Interpret_FieldsOtherThanTheSixInputs_DoNotChangeTheResult(SerotypeAgg agglutination, TestResult bexA,
            SerotypePcr serotypePcr, YesNoOptional growth)
        {
            var randomIsolate = new TestDataGenerator.Catalog().CreateInstance<Isolate>();
            randomIsolate.Agglutination = agglutination;
            randomIsolate.BexA = bexA;
            randomIsolate.SerotypePcr = serotypePcr;
            randomIsolate.Growth = growth;
            randomIsolate.Evaluation = Evaluation.HaemophilusTypeC;
            randomIsolate.Sending.SamplingLocation = SamplingLocation.Liquor;
            var plainIsolate = CreateIsolate(agglutination, bexA, serotypePcr, SamplingLocation.Liquor, growth);
            plainIsolate.Evaluation = Evaluation.HaemophilusTypeC;

            var fromRandom = isolateInterpretation.Interpret(randomIsolate);
            var fromPlain = isolateInterpretation.Interpret(plainIsolate);

            fromRandom.Interpretation.Should().Be(fromPlain.Interpretation);
            fromRandom.InterpretationPreliminary.Should().Be(fromPlain.InterpretationPreliminary);
            fromRandom.InterpretationDisclaimer.Should().Be(fromPlain.InterpretationDisclaimer);
        }

        // specs/006 FR-005, Decision Table row 1: exact final text, preliminary stays discrepant
        [TestCase(SerotypePcr.Negative)]
        [TestCase(SerotypePcr.NotDetermined)]
        public void Interpret_NegativeAgglutinationAndBexA_NegativeOrNotDeterminedPcr_IsExactNotTypableText(SerotypePcr serotypePcr)
        {
            var interpretation = isolateInterpretation.Interpret(CreateIsolate(SerotypeAgg.Negative, TestResult.Negative, serotypePcr));

            interpretation.Interpretation.Should().Be(
                "Die Ergebnisse sprechen für einen unbekapselten Haemophilus influenzae (sog. \"nicht-typisierbarer\" H. influenzae, NTHi).");
            interpretation.InterpretationPreliminary.Should().Be(Discrepant);
        }

        // specs/006 FR-008, Edge Cases 1 and 8: every negative-agglutination combination gets the unencapsulated
        // reporting disclaimer (blood, liquor, other invasive) or the non-invasive note (other non-invasive)
        [Test]
        public void Interpret_NegativeAgglutination_AnyBexAAndPcr_AssignsDisclaimerBySamplingLocation(
            [Values] TestResult bexA, [Values] SerotypePcr serotypePcr, [Values] SamplingLocation samplingLocation)
        {
            var interpretation = isolateInterpretation.Interpret(
                CreateIsolate(SerotypeAgg.Negative, bexA, serotypePcr, samplingLocation));

            var expected = samplingLocation == SamplingLocation.OtherNonInvasive
                ? NonInvasiveDisclaimer
                : InvasiveDisclaimer("Haemophilus influenzae, unbekapselt");
            interpretation.InterpretationDisclaimer.Should().Be(expected);
        }

        // specs/006 Decision Table row 4, Edge Case 1: negative agglutination with positive bexA, or with
        // not-determined bexA and a determined PCR value, stays discrepant but gets the disclaimer
        [TestCase(TestResult.Positive, SerotypePcr.NotDetermined)]
        [TestCase(TestResult.Positive, SerotypePcr.A)]
        [TestCase(TestResult.Positive, SerotypePcr.Negative)]
        [TestCase(TestResult.NotDetermined, SerotypePcr.B)]
        [TestCase(TestResult.NotDetermined, SerotypePcr.F)]
        [TestCase(TestResult.NotDetermined, SerotypePcr.Negative)]
        public void Interpret_NegativeAgglutination_PositiveBexAOrUndeterminedBexAWithPcrValue_IsDiscrepant(TestResult bexA,
            SerotypePcr serotypePcr)
        {
            var interpretation = isolateInterpretation.Interpret(CreateIsolate(SerotypeAgg.Negative, bexA, serotypePcr));

            interpretation.Interpretation.Should().Be(Discrepant);
            interpretation.InterpretationPreliminary.Should().Be(Discrepant);
            interpretation.InterpretationDisclaimer.Should().Be(InvasiveDisclaimer("Haemophilus influenzae, unbekapselt"));
        }

        // specs/006 Decision Table row 7, Edge Case 2: specific agglutination, bexA not determined and matching PCR
        // enters the specific branch (serotype disclaimer) without replacing either interpretation
        [TestCase(SerotypeAgg.A, SerotypePcr.A)]
        [TestCase(SerotypeAgg.B, SerotypePcr.B)]
        [TestCase(SerotypeAgg.C, SerotypePcr.C)]
        [TestCase(SerotypeAgg.D, SerotypePcr.D)]
        [TestCase(SerotypeAgg.E, SerotypePcr.E)]
        [TestCase(SerotypeAgg.F, SerotypePcr.F)]
        public void Interpret_SpecificAgglutinationUndeterminedBexA_MatchingPcr_IsDiscrepantWithSerotypeDisclaimer(
            SerotypeAgg agglutination, SerotypePcr serotypePcr)
        {
            var interpretation = isolateInterpretation.Interpret(
                CreateIsolate(agglutination, TestResult.NotDetermined, serotypePcr));

            interpretation.Interpretation.Should().Be(Discrepant);
            interpretation.InterpretationPreliminary.Should().Be(Discrepant);
            interpretation.InterpretationDisclaimer.Should().Be(
                InvasiveDisclaimer($"Haemophilus influenzae, Serotyp {agglutination.ToString().ToLower()}"));
        }

        // specs/006 Edge Case 3, FR-014: specific agglutination with negative bexA, or with a conflicting determined
        // PCR value, is unmatched: both texts discrepant, disclaimer empty
        [TestCase(SerotypeAgg.A, TestResult.Negative, SerotypePcr.NotDetermined)]
        [TestCase(SerotypeAgg.B, TestResult.Negative, SerotypePcr.B)]
        [TestCase(SerotypeAgg.C, TestResult.Negative, SerotypePcr.Negative)]
        [TestCase(SerotypeAgg.D, TestResult.Positive, SerotypePcr.E)]
        [TestCase(SerotypeAgg.E, TestResult.Positive, SerotypePcr.Negative)]
        [TestCase(SerotypeAgg.F, TestResult.NotDetermined, SerotypePcr.A)]
        [TestCase(SerotypeAgg.A, TestResult.NotDetermined, SerotypePcr.Negative)]
        public void Interpret_SpecificAgglutination_UnmatchedCombination_IsDiscrepantWithEmptyDisclaimer(
            SerotypeAgg agglutination, TestResult bexA, SerotypePcr serotypePcr)
        {
            var interpretation = isolateInterpretation.Interpret(CreateIsolate(agglutination, bexA, serotypePcr));

            interpretation.Interpretation.Should().Be(Discrepant);
            interpretation.InterpretationPreliminary.Should().Be(Discrepant);
            interpretation.InterpretationDisclaimer.Should().BeEmpty();
        }

        // specs/006 Edge Case 5, FR-004, FR-014: auto, poly and non-evaluable agglutination have no outcome
        [Test]
        public void Interpret_AutoPolyOrNotEvaluableAgglutination_IsDiscrepantWithEmptyDisclaimer(
            [Values(SerotypeAgg.Auto, SerotypeAgg.Poly, SerotypeAgg.NotEvaluable)] SerotypeAgg agglutination,
            [Values] TestResult bexA, [Values] SerotypePcr serotypePcr)
        {
            var interpretation = isolateInterpretation.Interpret(
                CreateIsolate(agglutination, bexA, serotypePcr, growth: YesNoOptional.Yes));

            interpretation.Interpretation.Should().Be(Discrepant);
            interpretation.InterpretationPreliminary.Should().Be(Discrepant);
            interpretation.InterpretationDisclaimer.Should().BeEmpty();
        }

        // specs/006 Edge Case 6, FR-004: all typing not determined with growth "not stated" stays discrepant
        [Test]
        public void Interpret_AllTypingNotDetermined_GrowthNotStated_IsDiscrepantWithEmptyDisclaimer()
        {
            var isolate = CreateIsolate(SerotypeAgg.NotDetermined, TestResult.NotDetermined, SerotypePcr.NotDetermined);
            isolate.Evaluation = Evaluation.HaemophilusParainfluenzae;

            var interpretation = isolateInterpretation.Interpret(isolate);

            interpretation.Interpretation.Should().Be(Discrepant);
            interpretation.InterpretationPreliminary.Should().Be(Discrepant);
            interpretation.InterpretationDisclaimer.Should().BeEmpty();
        }

        // specs/006 Edge Case 7: outside the all-not-determined/growth "no" branch the evaluation has no influence
        [TestCase(SerotypeAgg.Negative, TestResult.Negative, SerotypePcr.Negative, YesNoOptional.No)]
        [TestCase(SerotypeAgg.Negative, TestResult.NotDetermined, SerotypePcr.NotDetermined, YesNoOptional.No)]
        [TestCase(SerotypeAgg.A, TestResult.Positive, SerotypePcr.A, YesNoOptional.No)]
        [TestCase(SerotypeAgg.B, TestResult.NotDetermined, SerotypePcr.NotDetermined, YesNoOptional.No)]
        [TestCase(SerotypeAgg.Auto, TestResult.Positive, SerotypePcr.A, YesNoOptional.No)]
        [TestCase(SerotypeAgg.NotDetermined, TestResult.NotDetermined, SerotypePcr.NotDetermined, YesNoOptional.Yes)]
        [TestCase(SerotypeAgg.NotDetermined, TestResult.NotDetermined, SerotypePcr.NotDetermined, YesNoOptional.NotStated)]
        public void Interpret_OutsideNoGrowthBranch_EvaluationDoesNotChangeTheResult(SerotypeAgg agglutination,
            TestResult bexA, SerotypePcr serotypePcr, YesNoOptional growth)
        {
            var reference = isolateInterpretation.Interpret(CreateIsolate(agglutination, bexA, serotypePcr, growth: growth));

            foreach (Evaluation evaluation in Enum.GetValues(typeof(Evaluation)))
            {
                var isolate = CreateIsolate(agglutination, bexA, serotypePcr, growth: growth);
                isolate.Evaluation = evaluation;

                var interpretation = isolateInterpretation.Interpret(isolate);

                interpretation.Interpretation.Should().Be(reference.Interpretation);
                interpretation.InterpretationPreliminary.Should().Be(reference.InterpretationPreliminary);
                interpretation.InterpretationDisclaimer.Should().Be(reference.InterpretationDisclaimer);
            }
        }

        // specs/006 US3, FR-011, SC-002, SC-003, Edge Case 8: the sampling location changes only the disclaimer
        [TestCase(SerotypeAgg.Negative, TestResult.Negative, SerotypePcr.Negative, "Haemophilus influenzae, unbekapselt")]
        [TestCase(SerotypeAgg.Negative, TestResult.Negative, SerotypePcr.C, "Haemophilus influenzae, unbekapselt")]
        [TestCase(SerotypeAgg.Negative, TestResult.NotDetermined, SerotypePcr.NotDetermined, "Haemophilus influenzae, unbekapselt")]
        [TestCase(SerotypeAgg.A, TestResult.Positive, SerotypePcr.A, "Haemophilus influenzae, Serotyp a")]
        [TestCase(SerotypeAgg.D, TestResult.Positive, SerotypePcr.NotDetermined, "Haemophilus influenzae, Serotyp d")]
        [TestCase(SerotypeAgg.F, TestResult.NotDetermined, SerotypePcr.NotDetermined, "Haemophilus influenzae, Serotyp f")]
        public void Interpret_SamplingLocation_ChangesOnlyTheDisclaimer(SerotypeAgg agglutination, TestResult bexA,
            SerotypePcr serotypePcr, string category)
        {
            var blood = isolateInterpretation.Interpret(CreateIsolate(agglutination, bexA, serotypePcr));

            foreach (SamplingLocation samplingLocation in Enum.GetValues(typeof(SamplingLocation)))
            {
                var interpretation = isolateInterpretation.Interpret(
                    CreateIsolate(agglutination, bexA, serotypePcr, samplingLocation));

                interpretation.Interpretation.Should().Be(blood.Interpretation);
                interpretation.InterpretationPreliminary.Should().Be(blood.InterpretationPreliminary);
                interpretation.InterpretationDisclaimer.Should().Be(samplingLocation == SamplingLocation.OtherNonInvasive
                    ? NonInvasiveDisclaimer
                    : InvasiveDisclaimer(category));
            }
        }

        // specs/006 FR-001, FR-015: three report fields only (no report array, no comment); the isolate is not changed
        [TestCase(SerotypeAgg.Negative, TestResult.Negative, SerotypePcr.Negative, YesNoOptional.NotStated)]
        [TestCase(SerotypeAgg.B, TestResult.Positive, SerotypePcr.B, YesNoOptional.Yes)]
        [TestCase(SerotypeAgg.NotDetermined, TestResult.NotDetermined, SerotypePcr.NotDetermined, YesNoOptional.No)]
        [TestCase(SerotypeAgg.Poly, TestResult.Negative, SerotypePcr.A, YesNoOptional.NotStated)]
        public void Interpret_ProducesNoReportArrayNorComment_AndLeavesIsolateUnchanged(SerotypeAgg agglutination,
            TestResult bexA, SerotypePcr serotypePcr, YesNoOptional growth)
        {
            var isolate = CreateIsolate(agglutination, bexA, serotypePcr, SamplingLocation.OtherInvasive, growth);
            isolate.Evaluation = Evaluation.NoGrowth;

            var interpretation = isolateInterpretation.Interpret(isolate);

            interpretation.Report.Should().BeNull();
            interpretation.Comment.Should().BeNull();
            isolate.Agglutination.Should().Be(agglutination);
            isolate.BexA.Should().Be(bexA);
            isolate.SerotypePcr.Should().Be(serotypePcr);
            isolate.Growth.Should().Be(growth);
            isolate.Evaluation.Should().Be(Evaluation.NoGrowth);
            isolate.Sending.SamplingLocation.Should().Be(SamplingLocation.OtherInvasive);
        }

        // specs/006 Edge Case 9: the negative and specific branches need the submission to derive invasive status
        [TestCase(SerotypeAgg.Negative, TestResult.Negative, SerotypePcr.Negative)]
        [TestCase(SerotypeAgg.B, TestResult.Positive, SerotypePcr.B)]
        public void Interpret_NegativeOrSpecificBranchWithoutSending_FailsWithAnError(SerotypeAgg agglutination,
            TestResult bexA, SerotypePcr serotypePcr)
        {
            var isolate = new Isolate { Agglutination = agglutination, BexA = bexA, SerotypePcr = serotypePcr };

            Action interpret = () => isolateInterpretation.Interpret(isolate);

            interpret.Should().Throw<Exception>();
        }
    }
}