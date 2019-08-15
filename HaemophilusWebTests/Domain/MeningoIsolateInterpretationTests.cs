using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Collections;
using FluentAssertions.Primitives;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Utils;
using HaemophilusWeb.ViewModels;
using NUnit.Framework;

namespace HaemophilusWeb.Domain
{
    public class MeningoIsolateInterpretationTests
    {
        private const MeningoSamplingLocation NonInvasiveSamplingLocation = MeningoSamplingLocation.NasalSwab;
        private const MeningoSamplingLocation InvasiveSamplingLocation = MeningoSamplingLocation.Blood;

        [Test]
        public void EmptyIsolate_ReturnsEmptyInterpretation()
        {
            var isolateInterpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate()
            {
                Sending = new MeningoSending()
            };

            isolateInterpretation.Interpret(isolate);

            isolateInterpretation.Typings.Should().BeEmpty();
            isolateInterpretation.Result.Interpretation.Should().Contain("Diskrepante Ergebnisse");
        }

        [Test]
        public void IsolateMatchingStemRule1_ReturnsCorrespondingInterpretation()
        {
            var isolateInterpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                GrowthOnBloodAgar = Growth.No,
                GrowthOnMartinLewisAgar = Growth.No,
                Sending = new MeningoSending {SamplingLocation = NonInvasiveSamplingLocation},
                Oxidase = TestResult.NotDetermined,
                Agglutination = MeningoSerogroupAgg.NotDetermined,
                Onpg = TestResult.NotDetermined,
                GammaGt = TestResult.NotDetermined,
                SerogroupPcr = MeningoSerogroupPcr.NotDetermined,
                MaldiTof = UnspecificTestResult.NotDetermined,
                PorAPcr = NativeMaterialTestResult.NotDetermined,
                FetAPcr = NativeMaterialTestResult.NotDetermined
            };

            isolateInterpretation.Interpret(isolate);

            isolateInterpretation.Typings.Should().BeEmpty();
            isolateInterpretation.Result.Interpretation.Should().Contain("konnte nicht angezüchtet werden");
        }

        [Test]
        public void IsolateMatchingStemRule2_ReturnsCorrespondingInterpretation()
        {
            var isolateInterpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                GrowthOnBloodAgar = Growth.No,
                GrowthOnMartinLewisAgar = Growth.No,
                Sending = new MeningoSending { SamplingLocation = InvasiveSamplingLocation },
                Oxidase = TestResult.NotDetermined,
                Agglutination = MeningoSerogroupAgg.NotDetermined,
                Onpg = TestResult.NotDetermined,
                GammaGt = TestResult.NotDetermined,
                SerogroupPcr = MeningoSerogroupPcr.C,
                MaldiTof = UnspecificTestResult.NotDetermined,
                PorAPcr = NativeMaterialTestResult.Positive,
                FetAPcr = NativeMaterialTestResult.Positive,
                PorAVr1 = "X",
                PorAVr2 = "Y",
                FetAVr = "Z"
            };

            isolateInterpretation.Interpret(isolate);


            isolateInterpretation.Result.Interpretation.Should().Contain("Meldekategorie dieses Befundes: Neisseria meningitidis, Serogruppe C.");
            isolateInterpretation.Result.Interpretation.Should().Contain("konnte nicht angezüchtet werden");
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "Identifikation" && t.Value == "Neisseria meningitidis");
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "Serogenogruppe" && t.Value == "C");
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "PorA - Sequenztyp" && t.Value == "X, Y");
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "FetA - Sequenztyp" && t.Value == "Z");
        }

        [Test]
        public void IsolateMatchingStemRule3_ReturnsCorrespondingInterpretation()
        {
            var isolateInterpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                GrowthOnBloodAgar = Growth.ATypicalGrowth,
                GrowthOnMartinLewisAgar = Growth.No,
                Sending = new MeningoSending { SamplingLocation = NonInvasiveSamplingLocation },
                Agglutination = MeningoSerogroupAgg.NotDetermined,
                Onpg = TestResult.Negative,
                GammaGt = TestResult.Positive,
                SerogroupPcr = MeningoSerogroupPcr.NotDetermined,
                MaldiTof = UnspecificTestResult.NotDetermined,
                PorAPcr = NativeMaterialTestResult.NotDetermined,
                FetAPcr = NativeMaterialTestResult.NotDetermined,
            };

            isolateInterpretation.Interpret(isolate);

            isolateInterpretation.Result.Interpretation.Should().Contain("Kein Nachweis von Neisseria meningitidis.");
            isolateInterpretation.Typings.Should().NotContain(t => t.Attribute == "Identifikation");
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "Wachstum auf Blutagar" && t.Value == "atypisches Wachstum");
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "Wachstum auf Martin-Lewis-Agar" && t.Value == "Nein");
        }

        [TestCase(TestResult.Negative)]
        [TestCase(TestResult.Positive)]
        public void IsolateMatchingStemRule4And5_ReturnsCorrespondingInterpretation(TestResult gammaGtTestResult)
        {
            var isolateInterpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                GrowthOnBloodAgar = Growth.ATypicalGrowth,
                GrowthOnMartinLewisAgar = Growth.No,
                Sending = new MeningoSending { SamplingLocation = InvasiveSamplingLocation },
                Oxidase = TestResult.Negative,
                Agglutination = MeningoSerogroupAgg.NotDetermined,
                Onpg = TestResult.Negative,
                GammaGt = gammaGtTestResult,
                SerogroupPcr = MeningoSerogroupPcr.NotDetermined,
                MaldiTof = UnspecificTestResult.Determined,
                MaldiTofBestMatch = "N. gonorrhoeae",
                PorAPcr = NativeMaterialTestResult.NotDetermined,
                FetAPcr = NativeMaterialTestResult.NotDetermined
            };

            isolateInterpretation.Interpret(isolate);

            isolateInterpretation.Result.Interpretation.Should().Contain("Kein Nachweis von Neisseria meningitidis.");
            isolateInterpretation.Typings.Should().NotContain(t => t.Attribute == "Identifikation");
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "β-Galaktosidase" && t.Value == "negativ");
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "γ-Glutamyltransferase" && t.Value == EnumUtils.GetEnumDescription(typeof(TestResult), gammaGtTestResult));
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "MALDI-TOF (VITEK MS)" && t.Value == "N. gonorrhoeae");
        }

        [Test]
        public void IsolateMatchingStemRule6_ReturnsCorrespondingInterpretation()
        {
            var isolateInterpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                GrowthOnBloodAgar = Growth.TypicalGrowth,
                GrowthOnMartinLewisAgar = Growth.TypicalGrowth,
                Sending = new MeningoSending { SamplingLocation = NonInvasiveSamplingLocation },
                Oxidase = TestResult.Positive,
                Agglutination = MeningoSerogroupAgg.X,
                Onpg = TestResult.Negative,
                GammaGt = TestResult.Positive,
                SerogroupPcr = MeningoSerogroupPcr.NotDetermined,
                MaldiTof = UnspecificTestResult.NotDetermined,
                PorAPcr = NativeMaterialTestResult.NotDetermined,
                FetAPcr = NativeMaterialTestResult.NotDetermined
            };

            isolateInterpretation.Interpret(isolate);

            isolateInterpretation.Result.Interpretation.Should().BeEmpty();
            isolateInterpretation.Typings.Should().Contain(
                t => t.Attribute == "Identifikation" && t.Value == "Neisseria meningitidis");
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "Serogruppe" && t.Value == "X");
        }

        [Test]
        public void IsolateMatchingStemRule7_ReturnsCorrespondingInterpretation()
        {
            var isolateInterpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                GrowthOnBloodAgar = Growth.TypicalGrowth,
                GrowthOnMartinLewisAgar = Growth.TypicalGrowth,
                Sending = new MeningoSending { SamplingLocation = InvasiveSamplingLocation },
                Oxidase = TestResult.Positive,
                Agglutination = MeningoSerogroupAgg.E,
                Onpg = TestResult.Negative,
                GammaGt = TestResult.Positive,
                SerogroupPcr = MeningoSerogroupPcr.NotDetermined,
                MaldiTof = UnspecificTestResult.NotDetermined,
                PorAPcr = NativeMaterialTestResult.NotDetermined,
                FetAPcr = NativeMaterialTestResult.NotDetermined
            };

            isolateInterpretation.Interpret(isolate);

            isolateInterpretation.Result.Interpretation.Should().Contain("Meldekategorie dieses Befundes: Neisseria meningitidis, Serogruppe E.");
            isolateInterpretation.Typings.Should().Contain(
                t => t.Attribute == "Identifikation" && t.Value == "Neisseria meningitidis");
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "Serogruppe" && t.Value == "E");
        }

        [TestCase(MeningoSerogroupAgg.Auto)]
        [TestCase(MeningoSerogroupAgg.Poly)]
        public void IsolateMatchingStemRule8And9_ReturnsCorrespondingInterpretation(MeningoSerogroupAgg agglutination)
        {
            var isolateInterpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                GrowthOnBloodAgar = Growth.TypicalGrowth,
                GrowthOnMartinLewisAgar = Growth.TypicalGrowth,
                Sending = new MeningoSending { SamplingLocation = InvasiveSamplingLocation },
                Oxidase = TestResult.Positive,
                Agglutination = agglutination,
                Onpg = TestResult.Negative,
                GammaGt = TestResult.Positive,
                SerogroupPcr = MeningoSerogroupPcr.WY,
                MaldiTof = UnspecificTestResult.NotDetermined,
                PorAPcr = NativeMaterialTestResult.Positive,
                FetAPcr = NativeMaterialTestResult.Positive,
                PorAVr1 = "X",
                PorAVr2 = "Y",
                FetAVr = "Z"
            };

            isolateInterpretation.Interpret(isolate);

            isolateInterpretation.Result.Interpretation.Should().Contain("Meldekategorie dieses Befundes: Neisseria meningitidis, Serogruppe W/Y.");
            isolateInterpretation.Typings.Should().Contain(
                t => t.Attribute == "Identifikation" && t.Value == "Neisseria meningitidis");
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "Serogruppe" && t.Value.Contains(agglutination.ToString()) && !t.Value.Contains("Nicht-invasiv"));
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "Serogenogruppe" && t.Value.Contains("W/Y") && t.Value.Contains("molekularbiologisch bestimmt"));
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "PorA - Sequenztyp" && t.Value == "X, Y");
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "FetA - Sequenztyp" && t.Value == "Z");
        }

        [TestCase(MeningoSerogroupAgg.Auto)]
        [TestCase(MeningoSerogroupAgg.Poly)]
        public void IsolateMatchingStemRule10And11_ReturnsCorrespondingInterpretation(MeningoSerogroupAgg agglutination)
        {
            var isolateInterpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                GrowthOnBloodAgar = Growth.TypicalGrowth,
                GrowthOnMartinLewisAgar = Growth.TypicalGrowth,
                Sending = new MeningoSending { SamplingLocation = NonInvasiveSamplingLocation },
                Oxidase = TestResult.Positive,
                Agglutination = agglutination,
                Onpg = TestResult.Negative,
                GammaGt = TestResult.Positive,
                SerogroupPcr = MeningoSerogroupPcr.WY,
                MaldiTof = UnspecificTestResult.NotDetermined,
                PorAPcr = NativeMaterialTestResult.Positive,
                FetAPcr = NativeMaterialTestResult.Positive,
                PorAVr1 = "X",
                PorAVr2 = "Y",
                FetAVr = "Z"
            };

            isolateInterpretation.Interpret(isolate);

            isolateInterpretation.Result.Interpretation.Should().BeEmpty();
            isolateInterpretation.Typings.Should().Contain(
                t => t.Attribute == "Identifikation" && t.Value == "Neisseria meningitidis");
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "Serogruppe" && t.Value.Contains(agglutination.ToString()) && t.Value.Contains("Nicht-invasiv"));
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "Serogenogruppe" && t.Value.Contains("W/Y") && t.Value.Contains("molekularbiologisch"));
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "PorA - Sequenztyp" && t.Value == "X, Y");
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "FetA - Sequenztyp" && t.Value == "Z");
        }

        [TestCase(MeningoSerogroupAgg.Auto)]
        [TestCase(MeningoSerogroupAgg.Poly)]
        public void IsolateMatchingStemRule12And13_ReturnsCorrespondingInterpretation(MeningoSerogroupAgg agglutination)
        {
            var isolateInterpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                GrowthOnBloodAgar = Growth.TypicalGrowth,
                GrowthOnMartinLewisAgar = Growth.TypicalGrowth,
                Sending = new MeningoSending { SamplingLocation = NonInvasiveSamplingLocation },
                Oxidase = TestResult.Positive,
                Agglutination = agglutination,
                Onpg = TestResult.Negative,
                GammaGt = TestResult.Positive,
                SerogroupPcr = MeningoSerogroupPcr.NotDetermined,
                MaldiTof = UnspecificTestResult.NotDetermined,
                PorAPcr = NativeMaterialTestResult.NotDetermined,
                FetAPcr = NativeMaterialTestResult.NotDetermined
            };

            isolateInterpretation.Interpret(isolate);

            isolateInterpretation.Result.Interpretation.Should().BeEmpty();
            isolateInterpretation.Typings.Should().Contain(
                t => t.Attribute == "Identifikation" && t.Value == "Neisseria meningitidis");
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "Serogruppe" && t.Value.Contains(agglutination.ToString()) && t.Value.Contains("Nicht-invasiv"));
            isolateInterpretation.Typings.Should().NotContain(t => t.Attribute == "Serogenogruppe");
        }

        [Test]
        public void IsolateMatchingStemRule14_ReturnsCorrespondingInterpretation()
        {
            var isolateInterpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                GrowthOnBloodAgar = Growth.TypicalGrowth,
                GrowthOnMartinLewisAgar = Growth.TypicalGrowth,
                Sending = new MeningoSending { SamplingLocation = InvasiveSamplingLocation },
                Oxidase = TestResult.Positive,
                Agglutination = MeningoSerogroupAgg.WY,
                Onpg = TestResult.Negative,
                GammaGt = TestResult.Positive,
                SerogroupPcr = MeningoSerogroupPcr.NotDetermined,
                MaldiTof = UnspecificTestResult.NotDetermined,
                PorAPcr = NativeMaterialTestResult.Positive,
                FetAPcr = NativeMaterialTestResult.Positive,
                PorAVr1 = "X",
                PorAVr2 = "Y",
                FetAVr = "Z"
            };

            isolateInterpretation.Interpret(isolate);

            isolateInterpretation.Result.Interpretation.Should().Contain("Meldekategorie dieses Befundes: Neisseria meningitidis, Serogruppe W/Y.");
            isolateInterpretation.Typings.Should().Contain(
                t => t.Attribute == "Identifikation" && t.Value == "Neisseria meningitidis");
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "Serogruppe" && t.Value == "W/Y");
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "PorA - Sequenztyp" && t.Value == "X, Y");
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "FetA - Sequenztyp" && t.Value == "Z");
        }

        [TestCase("B")]
        [TestCase("C")]
        public void IsolateMatchingNativeMaterialRule1Or2_ReturnsCorrespondingInterpretation(string serogroup)
        {
            var interpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                Sending = new MeningoSending { Material = MeningoMaterial.NativeMaterial },
                CsbPcr = "B" == serogroup ? NativeMaterialTestResult.Positive : NativeMaterialTestResult.Negative,
                CscPcr = "C" == serogroup ? NativeMaterialTestResult.Positive : NativeMaterialTestResult.Negative,
                CswyPcr = NativeMaterialTestResult.Negative,
                PorAPcr = NativeMaterialTestResult.Positive,
                FetAPcr = NativeMaterialTestResult.Positive,
                PorAVr1 = "X",
                PorAVr2 = "Y",
                FetAVr = "Z"
            };

            interpretation.Interpret(isolate);

            interpretation.Result.Interpretation.Should().Contain($"Interpretation: Meningokokken-spezifische DNA konnte nachgewiesen werden. Die Ergebnisse sprechen für eine invasive Infektion mit Meningokokken der Serogruppe {serogroup}.");
            interpretation.Result.InterpretationDisclaimer.Should().NotBeEmpty();
            interpretation.Result.InterpretationDisclaimer.Should().Contain($"Serogruppe {serogroup}");
            interpretation.Typings.Should().Contain(
                t => t.Attribute == "Molekulare Typisierung" && t.Value == $"Das Serogruppe-{serogroup}-spezifische cs{serogroup.ToLower()}-Gen wurde nachgewiesen.");
            interpretation.Typings.Should().Contain(t =>
                t.Attribute == "PorA - Sequenztyp" && t.Value == "X, Y");
            interpretation.Typings.Should().Contain(t =>
                t.Attribute == "FetA - Sequenztyp" && t.Value == "Z");
        }

        [TestCase("W", "csw", "")]
        [TestCase("Y", "csy", "")]
        [TestCase("W/Y", "csw/csy", "n")]
        public void IsolateMatchingNativeMaterialRule34Or5_ReturnsCorrespondingInterpretation(string serogroup, string gen, string plural)
        {
            var interpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                Sending = new MeningoSending { Material = MeningoMaterial.NativeMaterial },
                CsbPcr = NativeMaterialTestResult.Negative,
                CscPcr = NativeMaterialTestResult.Negative,
                CswyPcr = NativeMaterialTestResult.Positive,
                PorAPcr = NativeMaterialTestResult.Positive,
                FetAPcr = NativeMaterialTestResult.Positive,
                CswyAllele = "W".Equals(serogroup) ? CswyAllel.Allele1 : "Y".Equals(serogroup) ? CswyAllel.Allele2 : CswyAllel.Allele3,
                PorAVr1 = "X",
                PorAVr2 = "Y",
                FetAVr = "Z"
            };


            interpretation.Interpret(isolate);

            interpretation.Result.Interpretation.Should().Contain($"Interpretation: Meningokokken-spezifische DNA konnte nachgewiesen werden. Die Ergebnisse sprechen für eine invasive Infektion mit Meningokokken der Serogruppe {serogroup}.");
            interpretation.Result.InterpretationDisclaimer.Should().NotBeEmpty();
            interpretation.Result.InterpretationDisclaimer.Should().Contain($"Serogruppe {serogroup}");
            interpretation.TypingAttribute("Molekulare Typisierung")
                .Should().Be($"Das Serogruppe{plural}-{serogroup}-spezifische {gen}-Gen wurde nachgewiesen.");
            interpretation.TypingAttribute("PorA - Sequenztyp").Should().Be("X, Y");
            interpretation.TypingAttribute("FetA - Sequenztyp").Should().Be("Z");
        }

        [TestCase(NativeMaterialTestResult.Negative, 0, "Negativ für bekapselte Neisseria meningitidis, bekapselte Haemophilus influenzae und Streptococcus pneumoniae.")]
        [TestCase(NativeMaterialTestResult.Positive, RealTimePcrResult.NeisseriaMeningitidis, "Positiv für bekapselte Neisseria meningitidis. Der molekularbiologische Nachweis von Neisseria meningitidis beruht auf dem Nachweis des Kapseltransportgens ctrA mittels spezifischer spezifischer Real-Time-PCR.")]
        [TestCase(NativeMaterialTestResult.Positive, RealTimePcrResult.StreptococcusPneumoniae, "Positiv für Streptococcus pneumoniae. Der molekularbiologische Nachweis von Streptococcus pneumoniae beruht auf dem Nachweis des Pneumolysin-Gens ply mittels spezifischer Real-Time-PCR.")]
        [TestCase(NativeMaterialTestResult.Positive, RealTimePcrResult.HaemophilusInfluenzae, "Positiv für bekapselte Haemophilus influenzae. Der molekularbiologische Nachweis von Haemophilus influenzae beruht auf dem Nachweis des Kapseltransportgens bexA mittels spezifischer Real-Time-PCR.")]
        public void IsolateMatchingNativeMaterialRule678Or9_ReturnsCorrespondingInterpretation(NativeMaterialTestResult realTimePcr, RealTimePcrResult realTimePcrResult, string realTimePcrInterpretation)
        {
            var interpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                Sending = new MeningoSending { Material = MeningoMaterial.NativeMaterial },
                CsbPcr = NativeMaterialTestResult.Negative,
                CscPcr = NativeMaterialTestResult.Negative,
                CswyPcr = NativeMaterialTestResult.NotDetermined,
                PorAPcr = NativeMaterialTestResult.Negative,
                FetAPcr = NativeMaterialTestResult.Negative,
                RealTimePcr = realTimePcr,
                RealTimePcrResult = realTimePcrResult
            };

            interpretation.Interpret(isolate);

            interpretation.Result.Interpretation.Should().Contain(
                realTimePcrResult == RealTimePcrResult.NeisseriaMeningitidis
                    ? "Meningokokken-spezifische DNA konnte nachgewiesen werden."
                    : "Kein Hinweis auf Neisseria meningitidis");

            if (realTimePcrResult == RealTimePcrResult.NeisseriaMeningitidis)
            {
                interpretation.Result.InterpretationDisclaimer.Should().NotBeEmpty();
            }
            else
            {
                interpretation.Result.InterpretationDisclaimer.Should().BeEmpty();
            }

            interpretation.TypingAttribute("Molekulare Typisierung")
                .Should().Be("Die Serogruppen B und C-spezifischen csb- und csc-Gene wurden nicht nachgewiesen.");
            interpretation.TypingAttribute("Real-Time-PCR (NHS Meningitis Real Tm, Firma Sacace)")
                .Should().Be(realTimePcrInterpretation);
            interpretation.TypingAttribute("PorA - Sequenztyp").Should().Contain("nicht amplifiziert");
            interpretation.TypingAttribute("FetA - Sequenztyp").Should().Contain("nicht amplifiziert");
        }

        [TestCase(NativeMaterialTestResult.Positive, "Neisseria meningitidis", "Meningokokken-spezifische DNA konnte nachgewiesen werden. Das Ergebnis spricht für eine invasive Meningokokkeninfektion.")]
        [TestCase(NativeMaterialTestResult.Positive, "Something else", "Meningokokken- spezifische DNA konnte nicht nachgewiesen werden. Kein Hinweis auf Neisseria meningitidis.")]
        public void IsolateMatchingNativeMaterialRule10or11_ReturnsCorrespondingInterpretation(NativeMaterialTestResult ribosomalRna16S, string ribosomalRna16SBestMatch, string expectedInterpretation)
        {
            var interpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                Sending = new MeningoSending { Material = MeningoMaterial.NativeMaterial },
                CsbPcr = NativeMaterialTestResult.Negative,
                CscPcr = NativeMaterialTestResult.Negative,
                CswyPcr = NativeMaterialTestResult.NotDetermined,
                PorAPcr = NativeMaterialTestResult.Negative,
                FetAPcr = NativeMaterialTestResult.Negative,
                RibosomalRna16S = ribosomalRna16S,
                RibosomalRna16SBestMatch = ribosomalRna16SBestMatch
            };

            interpretation.Interpret(isolate);

            interpretation.Result.Interpretation.Should().Contain(expectedInterpretation);

            if ("Neisseria meningitidis".Equals(ribosomalRna16SBestMatch))
            {
                interpretation.Result.InterpretationDisclaimer.Should().NotBeEmpty();
            }
            else
            {
                interpretation.Result.InterpretationDisclaimer.Should().BeEmpty();
            }

            interpretation.TypingAttribute("Molekulare Typisierung")
                .Should().Be("Die Serogruppen B und C-spezifischen csb- und csc-Gene wurden nicht nachgewiesen.");
            interpretation.TypingAttribute("16S-rDNA-Nachweis").Should().Be("positiv");
            interpretation.TypingAttribute("Ergebnis der DNA-Sequenzierung").Should().Be(ribosomalRna16SBestMatch);
            interpretation.TypingAttribute("PorA - Sequenztyp").Should().Contain("nicht amplifiziert");
            interpretation.TypingAttribute("FetA - Sequenztyp").Should().Contain("nicht amplifiziert");
        }

        [Test]
        public void IsolateMatchingNativeMaterialRule12_ReturnsCorrespondingInterpretation()
        {
            var interpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                Sending = new MeningoSending { Material = MeningoMaterial.NativeMaterial },
                CsbPcr = NativeMaterialTestResult.Negative,
                CscPcr = NativeMaterialTestResult.Negative,
                CswyPcr = NativeMaterialTestResult.NotDetermined,
                PorAPcr = NativeMaterialTestResult.Negative,
                FetAPcr = NativeMaterialTestResult.Negative,
                RibosomalRna16S = NativeMaterialTestResult.Negative
            };

            interpretation.Interpret(isolate);

            interpretation.Result.Interpretation.Should().Contain("Meningokokken- spezifische DNA konnte nicht nachgewiesen werden. Kein Hinweis auf Neisseria meningitidis.");
            interpretation.Result.InterpretationDisclaimer.Should().BeEmpty();

            interpretation.TypingAttribute("16S-rDNA-Nachweis").Should().Be("negativ");
            interpretation.TypingAttribute("PorA - Sequenztyp").Should().Contain("nicht amplifiziert");
            interpretation.TypingAttribute("FetA - Sequenztyp").Should().Contain("nicht amplifiziert");
        }
    }



    static class AssertionExtensions {
        public static string TypingAttribute(
            this MeningoIsolateInterpretation interpretation, string attributeName)
        {
            var attribute = interpretation.Typings.FirstOrDefault(t => t.Attribute == attributeName);
            attribute.Should().NotBeNull($"attribute '{attributeName}' should exist");
            return attribute.Value;
        }
    }
}