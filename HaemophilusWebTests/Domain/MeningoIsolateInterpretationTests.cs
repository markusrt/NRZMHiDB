using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Utils;
using NUnit.Framework;

namespace HaemophilusWeb.Domain
{
    public class MeningoIsolateInterpretationTests
    {
        private const MeningoSamplingLocation NonInvasiveSamplingLocation = MeningoSamplingLocation.NasalSwab;
        private const MeningoSamplingLocation InvasiveSamplingLocation = MeningoSamplingLocation.Blood;

        private Random _random = new Random();

        [Test]
        public void EmptyIsolate_ReturnsEmptyInterpretation()
        {
            var isolateInterpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                Sending = new MeningoSending()
            };

            isolateInterpretation.Interpret(isolate);

            isolateInterpretation.Typings.Should().BeEmpty();
            isolateInterpretation.Result.Report.Should().Contain(s => s.Contains("Diskrepante Ergebnisse"));
            isolateInterpretation.Result.Comment.Should().BeNull();
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
            isolateInterpretation.Result.Report.Should().Contain(s => s.Contains("konnte nicht angezüchtet werden"));
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

            isolateInterpretation.Result.Report.Should().Contain(s => s.Contains("Meldekategorie dieses Befundes: Neisseria meningitidis, Serogruppe C."));
            isolateInterpretation.Result.Report.Should().Contain(s => s.Contains("konnte nicht angezüchtet werden"));
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

            isolateInterpretation.Result.Report.Should().Contain(s => s.Contains("Kein Nachweis von Neisseria meningitidis."));
            isolateInterpretation.Typings.Should().NotContain(t => t.Attribute == "Identifikation");
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "Wachstum auf Blutagar" && t.Value == "atypisches Wachstum");
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "Wachstum auf Martin-Lewis-Agar" && t.Value == "Nein");
        }

        [TestCase(TestResult.Negative, TestName = "IsolateMatchingStemRule4_ReturnsCorrespondingInterpretation")]
        [TestCase(TestResult.Positive, TestName = "IsolateMatchingStemRule5_ReturnsCorrespondingInterpretation")]
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

            isolateInterpretation.Result.Report.Should().Contain(s => s.Contains("Kein Nachweis von Neisseria meningitidis."));
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

            isolateInterpretation.Result.Report.Should().BeEmpty();

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

            isolateInterpretation.Result.Report.Should().Contain(s => s.Contains("Meldekategorie dieses Befundes: Neisseria meningitidis, Serogruppe E."));
            isolateInterpretation.Typings.Should().Contain(
                t => t.Attribute == "Identifikation" && t.Value == "Neisseria meningitidis");
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "Serogruppe" && t.Value == "E");
        }

        [TestCase(MeningoSerogroupAgg.Auto, TestName = "IsolateMatchingStemRule8_ReturnsCorrespondingInterpretation")]
        [TestCase(MeningoSerogroupAgg.Poly, TestName = "IsolateMatchingStemRule9_ReturnsCorrespondingInterpretation")]
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

            isolateInterpretation.Result.Report.Should().Contain(s => s.Contains("Meldekategorie dieses Befundes: Neisseria meningitidis, Serogruppe W/Y."));
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

        [TestCase(MeningoSerogroupAgg.Auto, TestName = "IsolateMatchingStemRule10_ReturnsCorrespondingInterpretation")]
        [TestCase(MeningoSerogroupAgg.Poly, TestName = "IsolateMatchingStemRule11_ReturnsCorrespondingInterpretation")]
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

            isolateInterpretation.Result.Report.Should().BeEmpty();
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

        [TestCase(MeningoSerogroupAgg.Auto, TestName = "IsolateMatchingStemRule12_ReturnsCorrespondingInterpretation")]
        [TestCase(MeningoSerogroupAgg.Poly, TestName = "IsolateMatchingStemRule13_ReturnsCorrespondingInterpretation")]
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

            isolateInterpretation.Result.Report.Should().BeEmpty();
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

            isolateInterpretation.Result.Report.Should().Contain(s => s.Contains("Meldekategorie dieses Befundes: Neisseria meningitidis, Serogruppe W/Y."));
            isolateInterpretation.Typings.Should().Contain(
                t => t.Attribute == "Identifikation" && t.Value == "Neisseria meningitidis");
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "Serogruppe" && t.Value == "W/Y");
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "PorA - Sequenztyp" && t.Value == "X, Y");
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "FetA - Sequenztyp" && t.Value == "Z");
        }

        [TestCase(true, NativeMaterialTestResult.NotDetermined, TestName = "IsolateMatchingStemRule15_ReturnsCorrespondingInterpretation")]
        [TestCase(true, NativeMaterialTestResult.Positive, TestName = "IsolateMatchingStemRule16_ReturnsCorrespondingInterpretation")]
        [TestCase(false, NativeMaterialTestResult.NotDetermined, TestName = "IsolateMatchingStemRule17_ReturnsCorrespondingInterpretation")]
        [TestCase(false, NativeMaterialTestResult.Positive, TestName = "IsolateMatchingStemRule18_ReturnsCorrespondingInterpretation")]
        public void IsolateMatchingStemRule15to18_ReturnsCorrespondingInterpretation(bool invasive, NativeMaterialTestResult poraAndFeta)
        {
            var interpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                GrowthOnBloodAgar = Growth.TypicalGrowth,
                GrowthOnMartinLewisAgar = Growth.TypicalGrowth,
                Sending = new MeningoSending { SamplingLocation = invasive ? InvasiveSamplingLocation : NonInvasiveSamplingLocation },
                Oxidase = TestResult.Positive,
                Agglutination = MeningoSerogroupAgg.Negative,
                Onpg = TestResult.Negative,
                GammaGt = TestResult.Positive,
                SerogroupPcr = MeningoSerogroupPcr.Cnl,
                MaldiTof = UnspecificTestResult.NotDetermined,
                PorAPcr = poraAndFeta,
                FetAPcr = poraAndFeta,
                PorAVr1 = "X",
                PorAVr2 = "Y",
                FetAVr = "Z"
            };

            interpretation.Interpret(isolate);

            if (invasive)
            {
                interpretation.Result.Report.Should().Contain(s => s.Contains("Meldekategorie dieses Befundes: Neisseria meningitidis, Serogruppe negativ"));
            }
            else
            {
                interpretation.Result.Report.Should().BeEmpty();
            }
            
            interpretation.Typings.Should().Contain(
                t => t.Attribute == "Identifikation" && t.Value == "Neisseria meningitidis");
            interpretation.Typings.Should().Contain(t =>
                t.Attribute == "Agglutination" && t.Value == "keine Agglutination mit Antikörpern gegen die Serogruppen A, B, C, E, W, X und Y");

            if (poraAndFeta != NativeMaterialTestResult.NotDetermined)
            {
                interpretation.Typings.Should().Contain(t =>
                    t.Attribute == "PorA - Sequenztyp" && t.Value == "X, Y");
                interpretation.Typings.Should().Contain(t =>
                    t.Attribute == "FetA - Sequenztyp" && t.Value == "Z");
                interpretation.Result.Comment.Should()
                    .Contain("Die technische Durchführung der Sequenzierung erfolgte durch den Unterauftragnehmer");
            }
        }

        
        [TestCase(true, NativeMaterialTestResult.NotDetermined, TestName = "IsolateMatchingStemRule19_ReturnsCorrespondingInterpretation")]
        [TestCase(true, NativeMaterialTestResult.Positive, TestName = "IsolateMatchingStemRule20_ReturnsCorrespondingInterpretation")]
        [TestCase(false, NativeMaterialTestResult.NotDetermined, TestName = "IsolateMatchingStemRule21_ReturnsCorrespondingInterpretation")]
        [TestCase(false, NativeMaterialTestResult.Positive, TestName = "IsolateMatchingStemRule22_ReturnsCorrespondingInterpretation")]
        public void IsolateMatchingStemRule19to22_ReturnsCorrespondingInterpretation(bool invasive, NativeMaterialTestResult poraAndFeta)
        {
            var interpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                GrowthOnBloodAgar = Growth.TypicalGrowth,
                GrowthOnMartinLewisAgar = Growth.TypicalGrowth,
                Sending = new MeningoSending { SamplingLocation = invasive ? InvasiveSamplingLocation : NonInvasiveSamplingLocation },
                Oxidase = TestResult.Positive,
                Agglutination = MeningoSerogroupAgg.Poly,
                Onpg = TestResult.Negative,
                GammaGt = TestResult.Positive,
                SerogroupPcr = MeningoSerogroupPcr.Cnl,
                MaldiTof = UnspecificTestResult.NotDetermined,
                PorAPcr = poraAndFeta,
                FetAPcr = poraAndFeta,
                PorAVr1 = "X",
                PorAVr2 = "Y",
                FetAVr = "Z"
            };

            interpretation.Interpret(isolate);

            if (invasive)
            {
                interpretation.Result.Report.Should().Contain(s => s.Contains("Meldekategorie dieses Befundes: Neisseria meningitidis, Serogruppe poly"));
            }
            else
            {
                interpretation.Result.Report.Should().BeEmpty();
            }
            
            interpretation.Typings.Should().Contain(
                t => t.Attribute == "Identifikation" && t.Value == "Neisseria meningitidis");
            interpretation.Typings.Should().Contain(t =>
                t.Attribute == "Agglutination" && t.Value == "Poly-Agglutination mit Antikörpern gegen die Serogruppen B, C, W und Y");

            if (poraAndFeta != NativeMaterialTestResult.NotDetermined)
            {
                interpretation.Typings.Should().Contain(t =>
                    t.Attribute == "PorA - Sequenztyp" && t.Value == "X, Y");
                interpretation.Typings.Should().Contain(t =>
                    t.Attribute == "FetA - Sequenztyp" && t.Value == "Z");
                interpretation.Result.Comment.Should()
                    .Contain("Die technische Durchführung der Sequenzierung erfolgte durch den Unterauftragnehmer");
            }
        }

        [TestCase(true, NativeMaterialTestResult.NotDetermined, TestName = "IsolateMatchingStemRule23_ReturnsCorrespondingInterpretation")]
        [TestCase(true, NativeMaterialTestResult.Positive, TestName = "IsolateMatchingStemRule24_ReturnsCorrespondingInterpretation")]
        [TestCase(false, NativeMaterialTestResult.NotDetermined, TestName = "IsolateMatchingStemRule25_ReturnsCorrespondingInterpretation")]
        [TestCase(false, NativeMaterialTestResult.Positive, TestName = "IsolateMatchingStemRule26_ReturnsCorrespondingInterpretation")]
        public void IsolateMatchingStemRule23to26_ReturnsCorrespondingInterpretation(bool invasive, NativeMaterialTestResult poraAndFeta)
        {
            var interpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                GrowthOnBloodAgar = Growth.TypicalGrowth,
                GrowthOnMartinLewisAgar = Growth.TypicalGrowth,
                Sending = new MeningoSending { SamplingLocation = invasive ? InvasiveSamplingLocation : NonInvasiveSamplingLocation },
                Oxidase = TestResult.Positive,
                Agglutination = MeningoSerogroupAgg.Auto,
                Onpg = TestResult.Negative,
                GammaGt = TestResult.Positive,
                SerogroupPcr = MeningoSerogroupPcr.Cnl,
                MaldiTof = UnspecificTestResult.NotDetermined,
                PorAPcr = poraAndFeta,
                FetAPcr = poraAndFeta,
                PorAVr1 = "X",
                PorAVr2 = "Y",
                FetAVr = "Z"
            };

            interpretation.Interpret(isolate);

            if (invasive)
            {
                interpretation.Result.Report.Should().Contain(s => s.Contains("Meldekategorie dieses Befundes: Neisseria meningitidis, Serogruppe auto"));
            }
            else
            {
                interpretation.Result.Report.Should().BeEmpty();
            }
            
            interpretation.Typings.Should().Contain(
                t => t.Attribute == "Identifikation" && t.Value == "Neisseria meningitidis");
            interpretation.Typings.Should().Contain(t =>
                t.Attribute == "Agglutination" && t.Value == "Auto-Agglutination");

            if (poraAndFeta != NativeMaterialTestResult.NotDetermined)
            {
                interpretation.Typings.Should().Contain(t =>
                    t.Attribute == "PorA - Sequenztyp" && t.Value == "X, Y");
                interpretation.Typings.Should().Contain(t =>
                    t.Attribute == "FetA - Sequenztyp" && t.Value == "Z");
                interpretation.Result.Comment.Should()
                    .Contain("Die technische Durchführung der Sequenzierung erfolgte durch den Unterauftragnehmer");
            }
        }

        [TestCase("B", NativeMaterialTestResult.Negative)]
        [TestCase("B", NativeMaterialTestResult.NotDetermined)]
        [TestCase("B", NativeMaterialTestResult.Inhibitory)]
        [TestCase("C", NativeMaterialTestResult.Negative)]
        [TestCase("C", NativeMaterialTestResult.NotDetermined)]
        [TestCase("C", NativeMaterialTestResult.Inhibitory)]
        public void IsolateMatchingNativeMaterialRule1Or2_ReturnsCorrespondingInterpretation(string serogroup, NativeMaterialTestResult cswyPcr)
        {
            var interpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                Sending = new MeningoSending { Material = MeningoMaterial.NativeMaterial },
                CsbPcr = "B" == serogroup ? NativeMaterialTestResult.Positive : GetRandomNegativeOrInhibitory(),
                CscPcr = "C" == serogroup ? NativeMaterialTestResult.Positive : GetRandomNegativeOrInhibitory(),
                CswyPcr = cswyPcr,
                PorAPcr = NativeMaterialTestResult.Positive,
                FetAPcr = NativeMaterialTestResult.Positive,
                PorAVr1 = "X",
                PorAVr2 = "Y",
                FetAVr = "Z"
            };

            interpretation.Interpret(isolate);

            interpretation.Result.Report.Should().Contain(s => s.Contains($"Meningokokken-spezifische DNA konnte nachgewiesen werden. Die Ergebnisse sprechen für eine invasive Infektion mit Meningokokken der Serogruppe {serogroup}."));
            interpretation.Result.Report.Should().Contain(s => s.Contains("meldepflichtig"));
            interpretation.Result.Report.Should().Contain(s => s.Contains($"Serogruppe {serogroup}"));
            interpretation.Typings.Should().Contain(
                t => t.Attribute == "Molekulare Typisierung" && t.Value == $"Das Serogruppe-{serogroup}-spezifische cs{serogroup.ToLower()}-Gen wurde nachgewiesen.");
            interpretation.Typings.Should().Contain(t =>
                t.Attribute == "PorA - Sequenztyp" && t.Value == "X, Y");
            interpretation.Typings.Should().Contain(t =>
                t.Attribute == "FetA - Sequenztyp" && t.Value == "Z");
            interpretation.Result.Comment.Should().Contain("Microsynth Seqlab");
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
                CsbPcr = GetRandomNegativeOrInhibitory(),
                CscPcr = GetRandomNegativeOrInhibitory(),
                CswyPcr = NativeMaterialTestResult.Positive,
                PorAPcr = NativeMaterialTestResult.Positive,
                FetAPcr = NativeMaterialTestResult.Positive,
                CswyAllele = "W".Equals(serogroup) ? CswyAllel.Allele1 : "Y".Equals(serogroup) ? CswyAllel.Allele2 : CswyAllel.Allele3,
                PorAVr1 = "X",
                PorAVr2 = "Y",
                FetAVr = "Z"
            };


            interpretation.Interpret(isolate);

            interpretation.Result.Report.Should().Contain(s => s.Contains($"Meningokokken-spezifische DNA konnte nachgewiesen werden. Die Ergebnisse sprechen für eine invasive Infektion mit Meningokokken der Serogruppe {serogroup}."));
            interpretation.Result.Report.Should().Contain(s => s.Contains("meldepflichtig"));
            interpretation.Result.Report.Should().Contain(s => s.Contains($"Serogruppe {serogroup}"));
            interpretation.TypingAttribute("Molekulare Typisierung")
                .Should().Be($"Das Serogruppe{plural}-{serogroup}-spezifische {gen}-Gen wurde nachgewiesen.");
            interpretation.TypingAttribute("PorA - Sequenztyp").Should().Be("X, Y");
            interpretation.TypingAttribute("FetA - Sequenztyp").Should().Be("Z");
            interpretation.Result.Comment.Should().Contain("Microsynth Seqlab");
        }

        [TestCase("Rule 06", NativeMaterialTestResult.Inhibitory, false, 0, "Negativ für bekapselte Neisseria meningitidis, bekapselte Haemophilus influenzae und Streptococcus pneumoniae.")]
        [TestCase("Rule 06", NativeMaterialTestResult.Negative, false, 0, "Negativ für bekapselte Neisseria meningitidis, bekapselte Haemophilus influenzae und Streptococcus pneumoniae.")]
        [TestCase("Rule 13", NativeMaterialTestResult.Inhibitory, true,  0, "Negativ für bekapselte Neisseria meningitidis, bekapselte Haemophilus influenzae und Streptococcus pneumoniae.")]
        [TestCase("Rule 13", NativeMaterialTestResult.Negative, true,  0, "Negativ für bekapselte Neisseria meningitidis, bekapselte Haemophilus influenzae und Streptococcus pneumoniae.")]
        [TestCase("Rule 07", NativeMaterialTestResult.Positive, false, RealTimePcrResult.NeisseriaMeningitidis, "Positiv für bekapselte Neisseria meningitidis. Der molekularbiologische Nachweis von Neisseria meningitidis beruht auf dem Nachweis des Kapseltransportgens ctrA mittels spezifischer spezifischer Real-Time-PCR.")]
        [TestCase("Rule 14", NativeMaterialTestResult.Positive, true,  RealTimePcrResult.NeisseriaMeningitidis, "Positiv für bekapselte Neisseria meningitidis. Der molekularbiologische Nachweis von Neisseria meningitidis beruht auf dem Nachweis des Kapseltransportgens ctrA mittels spezifischer spezifischer Real-Time-PCR.")]
        [TestCase("Rule 08", NativeMaterialTestResult.Positive, false, RealTimePcrResult.StreptococcusPneumoniae, "Positiv für Streptococcus pneumoniae. Der molekularbiologische Nachweis von Streptococcus pneumoniae beruht auf dem Nachweis des Pneumolysin-Gens ply mittels spezifischer Real-Time-PCR.")]
        [TestCase("Rule 15", NativeMaterialTestResult.Positive, true,  RealTimePcrResult.StreptococcusPneumoniae, "Positiv für Streptococcus pneumoniae. Der molekularbiologische Nachweis von Streptococcus pneumoniae beruht auf dem Nachweis des Pneumolysin-Gens ply mittels spezifischer Real-Time-PCR.")]
        [TestCase("Rule 09", NativeMaterialTestResult.Positive, false, RealTimePcrResult.HaemophilusInfluenzae, "Positiv für bekapselte Haemophilus influenzae. Der molekularbiologische Nachweis von Haemophilus influenzae beruht auf dem Nachweis des Kapseltransportgens bexA mittels spezifischer Real-Time-PCR.")]
        [TestCase("Rule 16", NativeMaterialTestResult.Positive, true,  RealTimePcrResult.HaemophilusInfluenzae, "Positiv für bekapselte Haemophilus influenzae. Der molekularbiologische Nachweis von Haemophilus influenzae beruht auf dem Nachweis des Kapseltransportgens bexA mittels spezifischer Real-Time-PCR.")]
        public void IsolateMatchingNativeMaterialRule6to9and13to16_ReturnsCorrespondingInterpretation(string rule, NativeMaterialTestResult realTimePcr, bool cswyNegative, RealTimePcrResult realTimePcrResult, string realTimePcrInterpretation)
        {
            Console.WriteLine($"Test {rule}");
            var interpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                Sending = new MeningoSending { Material = MeningoMaterial.NativeMaterial },
                CsbPcr = GetRandomNegativeOrInhibitory(),
                CscPcr = GetRandomNegativeOrInhibitory(),
                CswyPcr = cswyNegative ? GetRandomNegativeOrInhibitory() : NativeMaterialTestResult.NotDetermined,
                PorAPcr = GetRandomNegativeOrInhibitory(),
                FetAPcr = GetRandomNegativeOrInhibitory(),
                RealTimePcr = realTimePcr == NativeMaterialTestResult.Negative ? GetRandomNegativeOrInhibitory() : realTimePcr,
                RealTimePcrResult = realTimePcrResult
            };

            interpretation.Interpret(isolate);

            interpretation.Result.Report.Should().Contain(s => s.Contains(
                realTimePcrResult == RealTimePcrResult.NeisseriaMeningitidis
                    ? "Meningokokken-spezifische DNA konnte nachgewiesen werden."
                    : "Kein Hinweis auf Neisseria meningitidis"));

            if (realTimePcrResult == RealTimePcrResult.NeisseriaMeningitidis)
            {
                interpretation.Result.Report.Should().Contain(s => s.Contains("meldepflichtig"));
            }
            else
            {
                interpretation.Result.Report.Should().NotContain(s => s.Contains("meldepflichtig"));
            }

            interpretation.TypingAttribute("Molekulare Typisierung")
                .Should().Be(
                    cswyNegative
                        ? "Die Serogruppen B, C,  W und Y-spezifischen csb-, csc-, csw- und csy-Gene wurden nicht nachgewiesen."
                        : "Die Serogruppen B und C-spezifischen csb- und csc-Gene wurden nicht nachgewiesen.");
            interpretation.TypingAttribute("Real-Time-PCR (NHS Meningitis Real Tm, Firma Sacace)")
                .Should().Be(realTimePcrInterpretation);
            interpretation.TypingAttribute("PorA - Sequenztyp").Should().Contain("nicht amplifiziert");
            interpretation.TypingAttribute("FetA - Sequenztyp").Should().Contain("nicht amplifiziert");
            interpretation.Result.Comment.Should().BeNull();
        }

        [TestCase("Rule 10", NativeMaterialTestResult.Positive, false, "Neisseria meningitidis", "Meningokokken-spezifische DNA konnte nachgewiesen werden. Das Ergebnis spricht für eine invasive Meningokokkeninfektion.")]
        [TestCase("Rule 17", NativeMaterialTestResult.Positive, true, "Neisseria meningitidis", "Meningokokken-spezifische DNA konnte nachgewiesen werden. Das Ergebnis spricht für eine invasive Meningokokkeninfektion.")]
        [TestCase("Rule 11", NativeMaterialTestResult.Positive, false, "Something else", "Meningokokken- spezifische DNA konnte nicht nachgewiesen werden.")]
        [TestCase("Rule 18", NativeMaterialTestResult.Positive, true, "Something else", "Meningokokken- spezifische DNA konnte nicht nachgewiesen werden.")]
        public void IsolateMatchingNativeMaterialRule10_11_17or18_ReturnsCorrespondingInterpretation(string rule, NativeMaterialTestResult ribosomalRna16S, bool cswyNegative, string ribosomalRna16SBestMatch, string expectedInterpretation)
        {
            Console.WriteLine($"Test {rule}");
            var interpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                Sending = new MeningoSending { Material = MeningoMaterial.NativeMaterial },
                CsbPcr = GetRandomNegativeOrInhibitory(),
                CscPcr = GetRandomNegativeOrInhibitory(),
                CswyPcr = cswyNegative ? GetRandomNegativeOrInhibitory() : NativeMaterialTestResult.NotDetermined,
                PorAPcr = GetRandomNegativeOrInhibitory(),
                FetAPcr = GetRandomNegativeOrInhibitory(),
                RibosomalRna16S = ribosomalRna16S,
                RibosomalRna16SBestMatch = ribosomalRna16SBestMatch
            };

            interpretation.Interpret(isolate);

            interpretation.Result.Report.Should().Contain(s => s.Contains(expectedInterpretation));

            if ("Neisseria meningitidis".Equals(ribosomalRna16SBestMatch))
            {
                interpretation.Result.Report.Should().Contain(s => s.Contains("meldepflichtig"));
            }
            else
            {
                interpretation.Result.Report.Should().NotContain(s => s.Contains("meldepflichtig"));
            }

            interpretation.TypingAttribute("Molekulare Typisierung")
                .Should().Be(
                    cswyNegative
                        ? "Die Serogruppen B, C,  W und Y-spezifischen csb-, csc-, csw- und csy-Gene wurden nicht nachgewiesen."
                        : "Die Serogruppen B und C-spezifischen csb- und csc-Gene wurden nicht nachgewiesen.");
            interpretation.TypingAttribute("16S-rDNA-Nachweis").Should().Be("positiv");
            interpretation.TypingAttribute("Ergebnis der DNA-Sequenzierung").Should().Be(ribosomalRna16SBestMatch);
            interpretation.TypingAttribute("PorA - Sequenztyp").Should().Contain("nicht amplifiziert");
            interpretation.TypingAttribute("FetA - Sequenztyp").Should().Contain("nicht amplifiziert");
            interpretation.Result.Comment.Should().Contain("Microsynth Seqlab");
        }


        [Test]
        public void IsolateMatchingNativeMaterialRule12_ReturnsCorrespondingInterpretation()
        {
            var interpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                Sending = new MeningoSending { Material = MeningoMaterial.NativeMaterial },
                CsbPcr = NativeMaterialTestResult.NotDetermined,
                CscPcr = NativeMaterialTestResult.NotDetermined,
                CswyPcr = NativeMaterialTestResult.NotDetermined,
                PorAPcr = GetRandomNegativeOrInhibitory(),
                FetAPcr = GetRandomNegativeOrInhibitory(),
                RibosomalRna16S = GetRandomNegativeOrInhibitory()
            };

            interpretation.Interpret(isolate);

            interpretation.Result.Report.Should().Contain(s => s.Contains("Meningokokken- spezifische DNA konnte nicht nachgewiesen werden."));
            interpretation.Result.Report.Should().Contain(s => s.Contains("Kein Hinweis auf Neisseria meningitidis."));
            interpretation.Result.Report.Should().NotContain(s => s.Contains("meldepflichtig"));

            interpretation.TypingAttribute("16S-rDNA-Nachweis").Should().Match(s => new List<string> {"negativ", "inhibitorisch" }.Contains(s));
            interpretation.TypingAttribute("PorA - Sequenztyp").Should().Contain("nicht amplifiziert");
            interpretation.TypingAttribute("FetA - Sequenztyp").Should().Contain("nicht amplifiziert");
        }

        [Test]
        public void IsolateMatchingNativeMaterialRule19_ReturnsCorrespondingInterpretation()
        {
            var interpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                Sending = new MeningoSending { Material = MeningoMaterial.NativeMaterial },
                CsbPcr = GetRandomNegativeOrInhibitory(),
                CscPcr = GetRandomNegativeOrInhibitory(),
                CswyPcr = GetRandomNegativeOrInhibitory(),
                PorAPcr = GetRandomNegativeOrInhibitory(),
                FetAPcr = GetRandomNegativeOrInhibitory(),
                RibosomalRna16S = GetRandomNegativeOrInhibitory()
            };

            interpretation.Interpret(isolate);

            interpretation.Result.Report.Should().Contain(s => s.Contains("Meningokokken- spezifische DNA konnte nicht nachgewiesen werden."));
            interpretation.Result.Report.Should().Contain(s => s.Contains("Kein Hinweis auf Neisseria meningitidis."));
            interpretation.Result.Report.Should().NotContain(s => s.Contains("meldepflichtig"));

            interpretation.TypingAttribute("Molekulare Typisierung")
                .Should().Be("Die Serogruppen B, C,  W und Y-spezifischen csb-, csc-, csw- und csy-Gene wurden nicht nachgewiesen.");
            interpretation.TypingAttribute("16S-rDNA-Nachweis").Should().Match(s => new List<string> { "negativ", "inhibitorisch" }.Contains(s));
            interpretation.TypingAttribute("PorA - Sequenztyp").Should().Contain("nicht amplifiziert");
            interpretation.TypingAttribute("FetA - Sequenztyp").Should().Contain("nicht amplifiziert");
        }

        [Test]
        public void IsolateMatchingNativeMaterialRule20_ReturnsCorrespondingInterpretation()
        {
            var interpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                Sending = new MeningoSending { Material = MeningoMaterial.NativeMaterial },
                CsbPcr = GetRandomNegativeOrInhibitory(),
                CscPcr = GetRandomNegativeOrInhibitory(),
                CswyPcr = GetRandomNegativeOrInhibitory(),
                PorAPcr = GetRandomNegativeOrInhibitory(),
                FetAPcr = GetRandomNegativeOrInhibitory(),
                RibosomalRna16S = NativeMaterialTestResult.NotDetermined
            };

            interpretation.Interpret(isolate);

            interpretation.Result.Report.Should().Contain(s => s.Contains("Meningokokken- spezifische DNA konnte nicht nachgewiesen werden."));
            interpretation.Result.Report.Should().Contain(s => s.Contains("Kein Hinweis auf Neisseria meningitidis."));
            interpretation.Result.Report.Should().NotContain(s => s.Contains("meldepflichtig"));

            interpretation.TypingAttribute("Molekulare Typisierung")
                .Should().Be("Die Serogruppen B, C,  W und Y-spezifischen csb-, csc-, csw- und csy-Gene wurden nicht nachgewiesen.");
            interpretation.TypingAttribute("PorA - Sequenztyp").Should().Contain("nicht amplifiziert");
            interpretation.TypingAttribute("FetA - Sequenztyp").Should().Contain("nicht amplifiziert");
        }

        private NativeMaterialTestResult GetRandomNegativeOrInhibitory()
        {
            return _random.Next(2) == 0 ? NativeMaterialTestResult.Negative : NativeMaterialTestResult.Inhibitory;
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