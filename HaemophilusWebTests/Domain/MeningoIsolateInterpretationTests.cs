using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using FluentAssertions;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Utils;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Ag = HaemophilusWeb.Models.Meningo.MeningoSerogroupAgg;
using Al = HaemophilusWeb.Models.Meningo.CswyAllel;
using Gr = HaemophilusWeb.Models.Meningo.Growth;
using Nm = HaemophilusWeb.Models.NativeMaterialTestResult;
using Pc = HaemophilusWeb.Models.Meningo.MeningoSerogroupPcr;
using Rt = HaemophilusWeb.Models.Meningo.RealTimePcrResult;
using Tr = HaemophilusWeb.Models.TestResult;
using Ut = HaemophilusWeb.Models.UnspecificTestResult;

namespace HaemophilusWeb.Domain
{
    public class MeningoIsolateInterpretationTests
    {
        private const MeningoSamplingLocation NonInvasiveSamplingLocation = MeningoSamplingLocation.NasalSwab;
        private const MeningoSamplingLocation InvasiveSamplingLocation = MeningoSamplingLocation.Blood;

        private List<string> RulesWithNoMeningococci = new List<string>()
        {
            "StemInterpretation_01", "StemInterpretation_03", "StemInterpretation_04", "StemInterpretation_05", "StemInterpretation_27", "StemInterpretation_34",
            "NativeMaterialInterpretation_06", "NativeMaterialInterpretation_08", "NativeMaterialInterpretation_09", "NativeMaterialInterpretation_11", 
            "NativeMaterialInterpretation_12", "NativeMaterialInterpretation_13", "NativeMaterialInterpretation_15", "NativeMaterialInterpretation_16", 
            "NativeMaterialInterpretation_18", "NativeMaterialInterpretation_19", "NativeMaterialInterpretation_20", "NativeMaterialInterpretation_23",
            "NativeMaterialInterpretation_24", "StemInterpretation_NoNM_01", "StemInterpretation_NoNM_02"
        };
        
        private readonly Random _random = new Random();

        [Test]
        public void EmptyIsolate_ReturnsNoGrowth()
        {
            var isolateInterpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                Sending = new MeningoSending()
            };

            isolateInterpretation.Interpret(isolate);

            isolateInterpretation.Typings.Should().BeEmpty();
            isolateInterpretation.Result.Report.Should().Contain(s => s.Contains("konnte nicht angezüchtet werden"));
            isolateInterpretation.Result.Report.Should().Contain(s => s.Contains("sollte unmittelbar vor dem Versand in Transportmedien"));
            isolateInterpretation.Serogroup.Should().BeNull();
            isolateInterpretation.Rule.Should().Be("StemInterpretation_27");
            AssertNoMeningococciFlagIsValid(isolateInterpretation);
        }


        [Test]
        public void FirstNonEmptyThenEmptyIsolate_ClearsSerogroup()
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
                MaldiTofVitek = UnspecificTestResult.NotDetermined,
                PorAPcr = NativeMaterialTestResult.Negative,
                FetAPcr = NativeMaterialTestResult.Positive,
            };
            var emptyIsolate = new MeningoIsolate
            {
                Sending = new MeningoSending { SamplingLocation = NonInvasiveSamplingLocation },
                PorAPcr = NativeMaterialTestResult.Positive,
            };

            isolateInterpretation.Interpret(isolate);
            isolateInterpretation.Serogroup.Should().Be("C");
            isolateInterpretation.Rule.Should().Be("StemInterpretation_29");

            isolateInterpretation.Interpret(emptyIsolate);
            isolateInterpretation.Serogroup.Should().BeNull();
            isolateInterpretation.Rule.Should().BeNull();
        }

        [Test]
        public void FirstMatchThenNoMatch_ClearsNoMeningococci()
        {
            var isolateInterpretation = new MeningoIsolateInterpretation();
            var emptyIsolate = new MeningoIsolate
            {
                Sending = new MeningoSending { SamplingLocation = NonInvasiveSamplingLocation },
                PorAPcr = NativeMaterialTestResult.Positive,
            };
            var noMeningococciIsolate = new MeningoIsolate
            {
                Sending = new MeningoSending()
            };

            isolateInterpretation.Interpret(noMeningococciIsolate);
            isolateInterpretation.Serogroup.Should().BeNull();
            isolateInterpretation.Rule.Should().Be("StemInterpretation_27");
            isolateInterpretation.NoMeningococci.Should().BeTrue();

            isolateInterpretation.Interpret(emptyIsolate);
            isolateInterpretation.Serogroup.Should().BeNull();
            isolateInterpretation.Rule.Should().BeNull();
            isolateInterpretation.NoMeningococci.Should().BeFalse();
        }

        [TestCase(false, TestName = "IsolateMatchingStemRule1_ReturnsCorrespondingInterpretation")]
        [TestCase(true, TestName = "IsolateMatchingStemRule27_ReturnsCorrespondingInterpretation")]
        public void IsolateMatchingStemRule1or27_ReturnsCorrespondingInterpretation(bool invasive)
        {
            var isolateInterpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                GrowthOnBloodAgar = Growth.No,
                GrowthOnMartinLewisAgar = Growth.No,
                Sending = new MeningoSending {SamplingLocation = invasive ? InvasiveSamplingLocation : NonInvasiveSamplingLocation},
                Oxidase = TestResult.NotDetermined,
                Agglutination = MeningoSerogroupAgg.NotDetermined,
                Onpg = TestResult.NotDetermined,
                GammaGt = TestResult.NotDetermined,
                SerogroupPcr = MeningoSerogroupPcr.NotDetermined,
                MaldiTofVitek = UnspecificTestResult.NotDetermined,
                PorAPcr = NativeMaterialTestResult.NotDetermined,
                FetAPcr = NativeMaterialTestResult.NotDetermined
            };

            isolateInterpretation.Interpret(isolate);

            isolateInterpretation.Typings.Should().BeEmpty();
            isolateInterpretation.Result.Report.Should().Contain(s => s.Contains("konnte nicht angezüchtet werden"));
            isolateInterpretation.Serogroup.Should().BeNull();
            AssertNoMeningococciFlagIsValid(isolateInterpretation);
        }

        [TestCase(MeningoSerogroupPcr.C, NativeMaterialTestResult.Positive, NativeMaterialTestResult.Positive, TestName = "IsolateMatchingStemRule2_ReturnsCorrespondingInterpretation")]
        [TestCase(MeningoSerogroupPcr.C, NativeMaterialTestResult.Positive, NativeMaterialTestResult.Negative, TestName = "IsolateMatchingStemRule28_ReturnsCorrespondingInterpretation")]
        [TestCase(MeningoSerogroupPcr.C, NativeMaterialTestResult.Negative, NativeMaterialTestResult.Positive, TestName = "IsolateMatchingStemRule29_ReturnsCorrespondingInterpretation")]
        [TestCase(MeningoSerogroupPcr.Negative, NativeMaterialTestResult.Positive, NativeMaterialTestResult.Positive, TestName = "IsolateMatchingStemRule30_ReturnsCorrespondingInterpretation")]
        [TestCase(MeningoSerogroupPcr.Negative, NativeMaterialTestResult.Positive, NativeMaterialTestResult.Negative, TestName = "IsolateMatchingStemRule31_ReturnsCorrespondingInterpretation")]
        [TestCase(MeningoSerogroupPcr.Negative, NativeMaterialTestResult.Negative, NativeMaterialTestResult.Positive, TestName = "IsolateMatchingStemRule32_ReturnsCorrespondingInterpretation")]
        public void IsolateMatchingStemRule2_28to32_ReturnsCorrespondingInterpretation(MeningoSerogroupPcr serogroupPcr, NativeMaterialTestResult porAPcr, NativeMaterialTestResult fetAPcr)
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
                SerogroupPcr = serogroupPcr,
                MaldiTofVitek = UnspecificTestResult.NotDetermined,
                PorAPcr = porAPcr,
                FetAPcr = fetAPcr,
                PorAVr1 = "X",
                PorAVr2 = "Y",
                FetAVr = "Z"
            };

            isolateInterpretation.Interpret(isolate);
           
            isolateInterpretation.Result.Report.Should().Contain(s => s.Contains("konnte nicht angezüchtet werden"));
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "Identifikation" && t.Value == "Neisseria meningitidis");
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "Serogenogruppe" && t.Value == 
                (serogroupPcr != MeningoSerogroupPcr.Negative ? "C" : "nicht gruppierbar"));
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "PorA - Sequenztyp"
                && t.Value == (porAPcr == NativeMaterialTestResult.Positive ? "X, Y" : "das porA-Gen konnte nicht amplifiziert werden"));
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "FetA - Sequenztyp"
                && t.Value == (fetAPcr == NativeMaterialTestResult.Positive ? "Z" : "das fetA-Gen konnte nicht amplifiziert werden"));
            isolateInterpretation.Result.Comment.Should().NotBeEmpty();

            if (serogroupPcr == MeningoSerogroupPcr.Negative)
            {
                isolateInterpretation.Result.Report.Should().Contain(s => s.Contains(
                    "Meldekategorie dieses Befundes: Neisseria meningitidis, NG (keine Serogruppe bestimmbar)."));
                isolateInterpretation.Serogroup.Should().Be("NG");
            }
            else
            {
                isolateInterpretation.Result.Report.Should().Contain(s => s.Contains(
                    "Meldekategorie dieses Befundes: Neisseria meningitidis, Serogruppe C."));
                isolateInterpretation.Serogroup.Should().Be("C");
            }
            AssertNoMeningococciFlagIsValid(isolateInterpretation);
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
                MaldiTofVitek = UnspecificTestResult.NotDetermined,
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
            isolateInterpretation.Serogroup.Should().BeNull();
            isolateInterpretation.Rule.Should().Be("StemInterpretation_03");
            AssertNoMeningococciFlagIsValid(isolateInterpretation);
        }

        [TestCase(TestResult.Negative, true, TestName = "IsolateMatchingStemRule4_ReturnsCorrespondingInterpretation")]
        [TestCase(TestResult.Negative, false, TestName = "IsolateMatchingStemRule4_ReturnsCorrespondingInterpretation")]
        [TestCase(TestResult.Positive, true, TestName = "IsolateMatchingStemRule5_ReturnsCorrespondingInterpretation")]
        public void IsolateMatchingStemRule4And5Vitek_ReturnsCorrespondingInterpretation(TestResult gammaGtTestResult, bool invasive)
        {
            var isolateInterpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                GrowthOnBloodAgar = Growth.ATypicalGrowth,
                GrowthOnMartinLewisAgar = Growth.No,
                Sending = new MeningoSending { SamplingLocation = invasive ? InvasiveSamplingLocation : NonInvasiveSamplingLocation },
                Oxidase = TestResult.Negative,
                Agglutination = MeningoSerogroupAgg.NotDetermined,
                Onpg = TestResult.Negative,
                GammaGt = gammaGtTestResult,
                SerogroupPcr = MeningoSerogroupPcr.NotDetermined,
                MaldiTofBiotyper = UnspecificTestResult.NotDetermined,
                MaldiTofVitek = UnspecificTestResult.Determined,
                MaldiTofVitekBestMatch = "N. gonorrhoeae",
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
            isolateInterpretation.Serogroup.Should().BeNull();
            AssertNoMeningococciFlagIsValid(isolateInterpretation);
        }

        [TestCase(TestResult.Negative, true, TestName = "IsolateMatchingStemRule4_ReturnsCorrespondingInterpretation")]
        [TestCase(TestResult.Negative, false, TestName = "IsolateMatchingStemRule4_ReturnsCorrespondingInterpretation")]
        [TestCase(TestResult.Positive, true, TestName = "IsolateMatchingStemRule5_ReturnsCorrespondingInterpretation")]
        public void IsolateMatchingStemRule4And5Biotyper_ReturnsCorrespondingInterpretation(TestResult gammaGtTestResult, bool invasive)
        {
            var isolateInterpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                GrowthOnBloodAgar = Growth.ATypicalGrowth,
                GrowthOnMartinLewisAgar = Growth.No,
                Sending = new MeningoSending { SamplingLocation = invasive ? InvasiveSamplingLocation : NonInvasiveSamplingLocation },
                Oxidase = TestResult.Negative,
                Agglutination = MeningoSerogroupAgg.NotDetermined,
                Onpg = TestResult.Negative,
                GammaGt = gammaGtTestResult,
                SerogroupPcr = MeningoSerogroupPcr.NotDetermined,
                MaldiTofVitek = UnspecificTestResult.NotDetermined,
                MaldiTofBiotyper = UnspecificTestResult.Determined,
                MaldiTofBiotyperBestMatch = "N. gonorrhoeae",
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
                t.Attribute == "MALDI-TOF (Biotyper)" && t.Value == "N. gonorrhoeae");
            isolateInterpretation.Serogroup.Should().BeNull();
            AssertNoMeningococciFlagIsValid(isolateInterpretation);
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
                MaldiTofVitek = UnspecificTestResult.NotDetermined,
                PorAPcr = NativeMaterialTestResult.NotDetermined,
                FetAPcr = NativeMaterialTestResult.NotDetermined
            };

            isolateInterpretation.Interpret(isolate);

            isolateInterpretation.Result.Report.Should().Contain(
                s => s.Contains("Anmerkung: Eine Resistenztestung wurde aus epidemiologischen und  Kostengründen nicht durchgeführt."));

            isolateInterpretation.Typings.Should().Contain(
                t => t.Attribute == "Identifikation" && t.Value == "Neisseria meningitidis");
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "Serogruppe" && t.Value == "X");
            isolateInterpretation.Serogroup.Should().Be("X");
            AssertNoMeningococciFlagIsValid(isolateInterpretation);
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
                MaldiTofVitek = UnspecificTestResult.NotDetermined,
                PorAPcr = NativeMaterialTestResult.NotDetermined,
                FetAPcr = NativeMaterialTestResult.NotDetermined
            };

            isolateInterpretation.Interpret(isolate);

            isolateInterpretation.Result.Report.Should().Contain(s => s.Contains("Meldekategorie dieses Befundes: Neisseria meningitidis, Serogruppe E."));
            isolateInterpretation.Typings.Should().Contain(
                t => t.Attribute == "Identifikation" && t.Value == "Neisseria meningitidis");
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "Serogruppe" && t.Value == "E");
            isolateInterpretation.Serogroup.Should().Be("E");
            AssertNoMeningococciFlagIsValid(isolateInterpretation);
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
                MaldiTofVitek = UnspecificTestResult.NotDetermined,
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
            isolateInterpretation.Serogroup.Should().Be("W/Y");
            AssertNoMeningococciFlagIsValid(isolateInterpretation);
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
                MaldiTofVitek = UnspecificTestResult.NotDetermined,
                PorAPcr = NativeMaterialTestResult.Positive,
                FetAPcr = NativeMaterialTestResult.Positive,
                PorAVr1 = "X",
                PorAVr2 = "Y",
                FetAVr = "Z"
            };

            isolateInterpretation.Interpret(isolate);

            isolateInterpretation.Result.Report.Should().Contain(
                s => s.Contains("Anmerkung: Eine Resistenztestung wurde aus epidemiologischen und  Kostengründen nicht durchgeführt."));

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
            isolateInterpretation.Serogroup.Should().Be("W/Y");
            AssertNoMeningococciFlagIsValid(isolateInterpretation);
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
                MaldiTofVitek = UnspecificTestResult.NotDetermined,
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
            isolateInterpretation.Serogroup.Should().Be("NG");
            AssertNoMeningococciFlagIsValid(isolateInterpretation);
        }

        [TestCase(Growth.TypicalGrowth, TestResult.Positive)]
        [TestCase(Growth.ATypicalGrowth, TestResult.Positive)]
        [TestCase(Growth.ATypicalGrowth, TestResult.Negative)]
        [TestCase(Growth.TypicalGrowth, TestResult.Negative)]
        public void IsolateMatchingStemRule14_ReturnsCorrespondingInterpretation(
            Growth growthOnMartinLewisAgar,
            TestResult gammaGt)
        {
            var isolateInterpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                GrowthOnBloodAgar = Growth.TypicalGrowth,
                GrowthOnMartinLewisAgar = growthOnMartinLewisAgar,
                Sending = new MeningoSending { SamplingLocation = InvasiveSamplingLocation },
                Oxidase = TestResult.Positive,
                Agglutination = MeningoSerogroupAgg.WY,
                Onpg = TestResult.Negative,
                GammaGt = gammaGt,
                SerogroupPcr = MeningoSerogroupPcr.NotDetermined,
                MaldiTofVitek = UnspecificTestResult.NotDetermined,
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
            isolateInterpretation.Serogroup.Should().Be("W/Y");
            AssertNoMeningococciFlagIsValid(isolateInterpretation);
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
                MaldiTofVitek = UnspecificTestResult.NotDetermined,
                PorAPcr = poraAndFeta,
                FetAPcr = poraAndFeta,
                PorAVr1 = "X",
                PorAVr2 = "Y",
                FetAVr = "Z"
            };

            interpretation.Interpret(isolate);

            if (invasive)
            {
                interpretation.Result.Report.Should().Contain(s => s.Contains("Meldekategorie dieses Befundes: Neisseria meningitidis, cnl (capsule null locus)."));
            }
            else
            {
                interpretation.Result.Report.Should().Contain(
                    s => s.Contains("Anmerkung: Eine Resistenztestung wurde aus epidemiologischen und  Kostengründen nicht durchgeführt."));
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
            interpretation.Serogroup.Should().Be("cnl");
            AssertNoMeningococciFlagIsValid(interpretation);
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
                MaldiTofVitek = UnspecificTestResult.NotDetermined,
                PorAPcr = poraAndFeta,
                FetAPcr = poraAndFeta,
                PorAVr1 = "X",
                PorAVr2 = "Y",
                FetAVr = "Z"
            };

            interpretation.Interpret(isolate);

            if (invasive)
            {
                interpretation.Result.Report.Should().Contain(s => s.Contains("Meldekategorie dieses Befundes: Neisseria meningitidis, cnl (capsule null locus)."));
            }
            else
            {
                interpretation.Result.Report.Should().Contain(
                    s => s.Contains("Anmerkung: Eine Resistenztestung wurde aus epidemiologischen und  Kostengründen nicht durchgeführt."));
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
            interpretation.Serogroup.Should().Be("cnl");
            AssertNoMeningococciFlagIsValid(interpretation);
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
                MaldiTofVitek = UnspecificTestResult.NotDetermined,
                PorAPcr = poraAndFeta,
                FetAPcr = poraAndFeta,
                PorAVr1 = "X",
                PorAVr2 = "Y",
                FetAVr = "Z"
            };

            interpretation.Interpret(isolate);

            if (invasive)
            {
                interpretation.Result.Report.Should().Contain(s => s.Contains("Meldekategorie dieses Befundes: Neisseria meningitidis, cnl (capsule null locus)."));
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

            interpretation.Serogroup.Should().Be("cnl");
            AssertNoMeningococciFlagIsValid(interpretation);
        }

        [Test]
        public void IsolateMatchingStemRule33_ReturnsCorrespondingInterpretation()
        {
            var interpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                GrowthOnBloodAgar = Growth.TypicalGrowth,
                GrowthOnMartinLewisAgar = Growth.TypicalGrowth,
                Sending = new MeningoSending { SamplingLocation = NonInvasiveSamplingLocation },
                Oxidase = TestResult.Positive,
                Agglutination = MeningoSerogroupAgg.Negative,
                Onpg = TestResult.Negative,
                GammaGt = TestResult.Positive,
                SerogroupPcr = MeningoSerogroupPcr.NotDetermined,
                MaldiTofVitek = UnspecificTestResult.NotDetermined,
                PorAPcr = NativeMaterialTestResult.NotDetermined,
                FetAPcr = NativeMaterialTestResult.NotDetermined,
            };

            interpretation.Interpret(isolate);

            interpretation.Result.Report.Should().Contain(s => s.Contains("Anmerkung: Eine Resistenztestung wurde aus epidemiologischen und  Kostengründen nicht durchgeführt."));
            interpretation.Typings.Should().Contain(
                t => t.Attribute == "Identifikation" && t.Value == "Neisseria meningitidis");
            interpretation.Typings.Should().Contain(t =>
                t.Attribute == "Agglutination" && t.Value == "keine Agglutination mit Antikörpern gegen die Serogruppen A, B, C, E, W, X und Y (Nicht-invasive Meningokokken sind oft unbekapselt.)");
            interpretation.Serogroup.Should().Be("NG");
            AssertNoMeningococciFlagIsValid(interpretation);
        }

        [Test]
        public void IsolateMatchingStemRule34Vitek_ReturnsCorrespondingInterpretation()
        {
            var isolateInterpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                GrowthOnBloodAgar = Growth.ATypicalGrowth,
                GrowthOnMartinLewisAgar = Growth.ATypicalGrowth,
                Sending = new MeningoSending { SamplingLocation = InvasiveSamplingLocation },
                Oxidase = TestResult.Negative,
                Agglutination = MeningoSerogroupAgg.NotDetermined,
                Onpg = TestResult.Negative,
                GammaGt = TestResult.Negative,
                SerogroupPcr = MeningoSerogroupPcr.NotDetermined,
                MaldiTofBiotyper = UnspecificTestResult.NotDetermined,
                MaldiTofVitek = UnspecificTestResult.Determined,
                MaldiTofVitekBestMatch = "N. gonorrhoeae",
                PorAPcr = NativeMaterialTestResult.NotDetermined,
                FetAPcr = NativeMaterialTestResult.NotDetermined
            };

            isolateInterpretation.Interpret(isolate);

            isolateInterpretation.Result.Report.Should().Contain(s => s.Contains("Kein Nachweis von Neisseria meningitidis."));
            isolateInterpretation.Typings.Should().NotContain(t => t.Attribute == "Identifikation");
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "β-Galaktosidase" && t.Value == "negativ");
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "γ-Glutamyltransferase" && t.Value == "negativ");
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "MALDI-TOF (VITEK MS)" && t.Value == "N. gonorrhoeae");
            isolateInterpretation.Serogroup.Should().BeNull();
            AssertNoMeningococciFlagIsValid(isolateInterpretation);
        }

        [Test]
        public void IsolateMatchingStemRule34Biotyper_ReturnsCorrespondingInterpretation()
        {
            var isolateInterpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                GrowthOnBloodAgar = Growth.ATypicalGrowth,
                GrowthOnMartinLewisAgar = Growth.ATypicalGrowth,
                Sending = new MeningoSending { SamplingLocation = InvasiveSamplingLocation },
                Oxidase = TestResult.Negative,
                Agglutination = MeningoSerogroupAgg.NotDetermined,
                Onpg = TestResult.Negative,
                GammaGt = TestResult.Negative,
                SerogroupPcr = MeningoSerogroupPcr.NotDetermined,
                MaldiTofVitek = UnspecificTestResult.NotDetermined,
                MaldiTofBiotyper = UnspecificTestResult.Determined,
                MaldiTofBiotyperBestMatch = "N. gonorrhoeae",
                PorAPcr = NativeMaterialTestResult.NotDetermined,
                FetAPcr = NativeMaterialTestResult.NotDetermined
            };

            isolateInterpretation.Interpret(isolate);

            isolateInterpretation.Result.Report.Should().Contain(s => s.Contains("Kein Nachweis von Neisseria meningitidis."));
            isolateInterpretation.Typings.Should().NotContain(t => t.Attribute == "Identifikation");
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "β-Galaktosidase" && t.Value == "negativ");
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "γ-Glutamyltransferase" && t.Value == "negativ");
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "MALDI-TOF (Biotyper)" && t.Value == "N. gonorrhoeae");
            isolateInterpretation.Serogroup.Should().BeNull();
            AssertNoMeningococciFlagIsValid(isolateInterpretation);
        }

        [Test]
        public void IsolateMatchingStemRule35_ReturnsCorrespondingInterpretation()
        {
            var interpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                GrowthOnBloodAgar = Growth.No,
                GrowthOnMartinLewisAgar = Growth.No,
                Sending = new MeningoSending { SamplingLocation = InvasiveSamplingLocation },
                Oxidase = TestResult.NotDetermined,
                Agglutination = MeningoSerogroupAgg.NotDetermined,
                Onpg = TestResult.NotDetermined,
                GammaGt = TestResult.NotDetermined,
                SerogroupPcr = MeningoSerogroupPcr.WY,
                MaldiTofVitek = UnspecificTestResult.NotDetermined,
                PorAPcr = NativeMaterialTestResult.Negative,
                FetAPcr = NativeMaterialTestResult.Negative,
            };

            interpretation.Interpret(isolate);
           
            interpretation.Result.Report.Should().Contain(s => s.Contains("meldepflichtig"));
            interpretation.Result.Report.Should().Contain(s => s.Contains("Meldekategorie dieses Befundes: Neisseria meningitidis, Serogruppe W/Y"));
            interpretation.Result.Report.Should().Contain(s => s.Contains("konnte nicht angezüchtet werden"));
            interpretation.TypingAttribute("Serogenogruppe").Should().Be("W/Y");
            interpretation.TypingAttribute("PorA - Sequenztyp").Should().Contain("nicht amplifiziert");
            interpretation.TypingAttribute("FetA - Sequenztyp").Should().Contain("nicht amplifiziert");
            interpretation.Serogroup.Should().Be("W/Y");
            AssertNoMeningococciFlagIsValid(interpretation);
        }

        [TestCase(MeningoSerogroupAgg.C, MeningoSerogroupPcr.NotDetermined, TestName = "IsolateMatchingStemRule36_ReturnsCorrespondingInterpretation")]
        [TestCase(MeningoSerogroupAgg.Poly, MeningoSerogroupPcr.Negative, TestName = "IsolateMatchingStemRule37_ReturnsCorrespondingInterpretation")]
        [TestCase(MeningoSerogroupAgg.Auto, MeningoSerogroupPcr.NotDetermined, TestName = "IsolateMatchingStemRule38_ReturnsCorrespondingInterpretation")]
        public void IsolateMatchingStemRule36To38_ReturnsCorrespondingInterpretation(
            MeningoSerogroupAgg agglutination,
            MeningoSerogroupPcr serogroupPcr )
        {
            var interpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                GrowthOnBloodAgar = Growth.TypicalGrowth,
                GrowthOnMartinLewisAgar = Growth.TypicalGrowth,
                Sending = new MeningoSending { SamplingLocation = NonInvasiveSamplingLocation },
                Oxidase = TestResult.Positive,
                Agglutination = agglutination,
                Onpg = TestResult.Negative,
                GammaGt = TestResult.Positive,
                SerogroupPcr = serogroupPcr,
                MaldiTofVitek = UnspecificTestResult.NotDetermined,
                PorAPcr = NativeMaterialTestResult.Positive,
                FetAPcr = NativeMaterialTestResult.Positive,
                PorAVr1 = "X",
                PorAVr2 = "Y",
                FetAVr = "Z"
            };

            interpretation.Interpret(isolate);

            var agglutinationText = agglutination.ToString();
            interpretation.Result.Report.Should().Contain(
                s => s.Contains("Anmerkung: Eine Resistenztestung wurde aus epidemiologischen und  Kostengründen nicht durchgeführt."));

            interpretation.TypingAttribute("Identifikation").Should().Be("Neisseria meningitidis");
            interpretation.TypingAttribute("PorA - Sequenztyp").Should().Be("X, Y");
            interpretation.TypingAttribute("FetA - Sequenztyp").Should().Be("Z");
            interpretation.TypingAttribute("Serogruppe").Should().Contain(agglutinationText);
            interpretation.Result.Comment.Should().Contain("Microsynth Seqlab");
            
            if (agglutination == MeningoSerogroupAgg.Poly || agglutination == MeningoSerogroupAgg.Auto)
            {
                interpretation.TypingAttribute("Serogruppe").Should().Contain("Nicht-invasiv");
                interpretation.Serogroup.Should().Be("NG");
            }
            else
            {
                interpretation.Serogroup.Should().Be(agglutinationText);
            }
           
            AssertNoMeningococciFlagIsValid(interpretation);
        }

        [Test]
        public void IsolateMatchingStemRule39_ReturnsInconclusiveAsRuleWasRemoved()
        {
            var interpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                GrowthOnBloodAgar = Growth.TypicalGrowth,
                GrowthOnMartinLewisAgar = Growth.TypicalGrowth,
                Sending = new MeningoSending { SamplingLocation = NonInvasiveSamplingLocation },
                Oxidase = TestResult.Positive,
                Agglutination = MeningoSerogroupAgg.C,
                Onpg = TestResult.Negative,
                GammaGt = TestResult.Positive,
                SerogroupPcr = MeningoSerogroupPcr.C,
                MaldiTofVitek = UnspecificTestResult.NotDetermined,
                PorAPcr = NativeMaterialTestResult.NotDetermined,
                FetAPcr = NativeMaterialTestResult.NotDetermined
            };

            interpretation.Interpret(isolate);

            interpretation.Result.Report.Should().Contain(
                s => s.Contains("Diskrepante Ergebnisse"));
        }

        [TestCase(MeningoSerogroupAgg.Negative, MeningoSerogroupPcr.B)]
        [TestCase(MeningoSerogroupAgg.Poly, MeningoSerogroupPcr.X)]
        public void IsolateMatchingStemRule40_ReturnsCorrespondingInterpretation(
            MeningoSerogroupAgg agglutination,
            MeningoSerogroupPcr serogroupPcr )
        {
            var interpretation = new MeningoIsolateInterpretation();
            var expectedSerogroup = serogroupPcr.ToString();
            var isolate = new MeningoIsolate
            {
                GrowthOnBloodAgar = Growth.TypicalGrowth,
                GrowthOnMartinLewisAgar = Growth.TypicalGrowth,
                Sending = new MeningoSending { SamplingLocation = NonInvasiveSamplingLocation },
                Oxidase = TestResult.Positive,
                Agglutination = agglutination,
                Onpg = TestResult.Negative,
                GammaGt = TestResult.Positive,
                SerogroupPcr = serogroupPcr,
                MaldiTofVitek = UnspecificTestResult.NotDetermined,
                PorAPcr = NativeMaterialTestResult.NotDetermined,
                FetAPcr = NativeMaterialTestResult.NotDetermined
            };

            interpretation.Interpret(isolate);

            interpretation.Result.Report.Should().Contain(
                s => s.Contains("Anmerkung: Eine Resistenztestung wurde aus epidemiologischen und  Kostengründen nicht durchgeführt."));

            interpretation.TypingAttribute("Identifikation").Should().Be("Neisseria meningitidis");
            interpretation.TypingAttribute("Serogenogruppe").Should().Contain(expectedSerogroup);
            interpretation.TypingAttribute("Serogenogruppe").Should().Contain("molekularbiologisch bestimmt");
            interpretation.Serogroup.Should().Be(expectedSerogroup);

            AssertNoMeningococciFlagIsValid(interpretation);
            interpretation.Rule.Should().Be("StemInterpretation_40");
        }
        
        [Test]
        public void IsolateMatchingStemRule41_ReturnsCorrespondingInterpretation()
        {
            var interpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                GrowthOnBloodAgar = Growth.TypicalGrowth,
                GrowthOnMartinLewisAgar = Growth.TypicalGrowth,
                Sending = new MeningoSending { SamplingLocation = NonInvasiveSamplingLocation },
                Oxidase = TestResult.Positive,
                Agglutination = MeningoSerogroupAgg.Negative,
                Onpg = TestResult.Negative,
                GammaGt = TestResult.Positive,
                SerogroupPcr = MeningoSerogroupPcr.NotDetermined,
                MaldiTofVitek = UnspecificTestResult.NotDetermined,
                PorAPcr = NativeMaterialTestResult.Positive,
                FetAPcr = NativeMaterialTestResult.Positive,
                PorAVr1 = "X",
                PorAVr2 = "Y",
                FetAVr = "Z"
            };

            interpretation.Interpret(isolate);

            interpretation.Result.Report.Should().Contain(
                s => s.Contains("Anmerkung: Eine Resistenztestung wurde aus epidemiologischen und  Kostengründen nicht durchgeführt."));

            interpretation.TypingAttribute("Identifikation").Should().Be("Neisseria meningitidis");
            interpretation.TypingAttribute("Agglutination").Should().Be("keine Agglutination mit Antikörpern gegen die Serogruppen  B, C, W und Y");
            interpretation.TypingAttribute("PorA - Sequenztyp").Should().Be("X, Y");
            interpretation.TypingAttribute("FetA - Sequenztyp").Should().Be("Z");
            interpretation.Serogroup.Should().Be("NG");
            
            AssertNoMeningococciFlagIsValid(interpretation);
            interpretation.Rule.Should().Be("StemInterpretation_41");
        }

        [Test]
        public void IsolateMatchingStemRule43_ReturnsCorrespondingInterpretation()
        {
            var interpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                GrowthOnBloodAgar = Growth.TypicalGrowth,
                GrowthOnMartinLewisAgar = Growth.TypicalGrowth,
                Sending = new MeningoSending { SamplingLocation = NonInvasiveSamplingLocation },
                Oxidase = TestResult.Positive,
                Agglutination = MeningoSerogroupAgg.NotDetermined,
                Onpg = TestResult.Negative,
                GammaGt = TestResult.Positive,
                SerogroupPcr = MeningoSerogroupPcr.NotDetermined,
                MaldiTofVitek = UnspecificTestResult.NotDetermined,
                PorAPcr = NativeMaterialTestResult.NotDetermined,
                FetAPcr = NativeMaterialTestResult.NotDetermined
            };

            interpretation.Interpret(isolate);

            interpretation.Result.Report.Should().Contain(
                s => s.Contains("Anmerkung: Aus Kostengründen werden bei nicht-invasiven Isolaten keine Serogruppenbestimmungen und Resistenztestungen durchgeführt."));

            interpretation.TypingAttribute("Identifikation").Should().Be("Neisseria meningitidis");

            AssertNoMeningococciFlagIsValid(interpretation);
            interpretation.Rule.Should().Be("StemInterpretation_43");
        }

        [TestCase(Growth.No, MeningoSamplingLocation.Blood)]
        [TestCase(Growth.No, MeningoSamplingLocation.NasalSwab)]
        [TestCase(Growth.ATypicalGrowth, MeningoSamplingLocation.Blood)]
        [TestCase(Growth.ATypicalGrowth, MeningoSamplingLocation.NasalSwab)]
        public void IsolateMatchingStemRuleNoNM_01Vitek_ReturnsCorrespondingInterpretation(
            Growth growthOnMartinLewisAgar,
            MeningoSamplingLocation samplingLocation)
        {
            var interpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                GrowthOnBloodAgar = Growth.ATypicalGrowth,
                GrowthOnMartinLewisAgar = growthOnMartinLewisAgar,
                Sending = new MeningoSending { SamplingLocation =  samplingLocation},
                Oxidase = TestResult.Positive,
                Agglutination = MeningoSerogroupAgg.NotDetermined,
                Onpg = TestResult.Negative,
                GammaGt = TestResult.NotDetermined,
                SerogroupPcr = MeningoSerogroupPcr.NotDetermined,
                MaldiTofBiotyper = UnspecificTestResult.NotDetermined,
                MaldiTofVitek = UnspecificTestResult.Determined,
                MaldiTofVitekBestMatch = "N. gonorrhoeae",
                PorAPcr = NativeMaterialTestResult.NotDetermined,
                FetAPcr = NativeMaterialTestResult.NotDetermined
            };

            interpretation.Interpret(isolate);

            interpretation.Result.Report.Should().Contain(s => s.Contains("Kein Nachweis von Neisseria meningitidis."));
            interpretation.Typings.Should().NotContain(t => t.Attribute == "Identifikation");
            interpretation.TypingAttribute("β-Galaktosidase").Should().Be("negativ");
            interpretation.TypingAttribute("γ-Glutamyltransferase").Should().Be("n.d.");
            interpretation.TypingAttribute("MALDI-TOF (VITEK MS)").Should().Be("N. gonorrhoeae");
            interpretation.Serogroup.Should().BeNull();

            AssertNoMeningococciFlagIsValid(interpretation);
            interpretation.Rule.Should().Be("StemInterpretation_NoNM_01");
        }

        [TestCase(Growth.No, MeningoSamplingLocation.Blood)]
        [TestCase(Growth.No, MeningoSamplingLocation.NasalSwab)]
        [TestCase(Growth.ATypicalGrowth, MeningoSamplingLocation.Blood)]
        [TestCase(Growth.ATypicalGrowth, MeningoSamplingLocation.NasalSwab)]
        public void IsolateMatchingStemRuleNoNM_01Biotyper_ReturnsCorrespondingInterpretation(
            Growth growthOnMartinLewisAgar,
            MeningoSamplingLocation samplingLocation)
        {
            var interpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                GrowthOnBloodAgar = Growth.ATypicalGrowth,
                GrowthOnMartinLewisAgar = growthOnMartinLewisAgar,
                Sending = new MeningoSending { SamplingLocation =  samplingLocation},
                Oxidase = TestResult.Positive,
                Agglutination = MeningoSerogroupAgg.NotDetermined,
                Onpg = TestResult.Negative,
                GammaGt = TestResult.NotDetermined,
                SerogroupPcr = MeningoSerogroupPcr.NotDetermined,
                MaldiTofVitek = UnspecificTestResult.NotDetermined,
                MaldiTofBiotyper = UnspecificTestResult.Determined,
                MaldiTofBiotyperBestMatch = "N. gonorrhoeae",
                PorAPcr = NativeMaterialTestResult.NotDetermined,
                FetAPcr = NativeMaterialTestResult.NotDetermined
            };

            interpretation.Interpret(isolate);

            interpretation.Result.Report.Should().Contain(s => s.Contains("Kein Nachweis von Neisseria meningitidis."));
            interpretation.Typings.Should().NotContain(t => t.Attribute == "Identifikation");
            interpretation.TypingAttribute("β-Galaktosidase").Should().Be("negativ");
            interpretation.TypingAttribute("γ-Glutamyltransferase").Should().Be("n.d.");
            interpretation.TypingAttribute("MALDI-TOF (Biotyper)").Should().Be("N. gonorrhoeae");
            interpretation.Serogroup.Should().BeNull();

            AssertNoMeningococciFlagIsValid(interpretation);
            interpretation.Rule.Should().Be("StemInterpretation_NoNM_01");
        }

        [TestCase(Growth.No, MeningoSamplingLocation.Blood)]
        [TestCase(Growth.ATypicalGrowth, MeningoSamplingLocation.Blood)]
        [TestCase(Growth.ATypicalGrowth, MeningoSamplingLocation.NasalSwab)]
        public void IsolateMatchingStemRuleNoNM_02_ReturnsCorrespondingInterpretation(
            Growth growthOnMartinLewisAgar,
            MeningoSamplingLocation samplingLocation)
        {
            var interpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                GrowthOnBloodAgar = Growth.ATypicalGrowth,
                GrowthOnMartinLewisAgar = growthOnMartinLewisAgar,
                Sending = new MeningoSending { SamplingLocation =  samplingLocation},
                Oxidase = TestResult.NotDetermined,
                Agglutination = MeningoSerogroupAgg.NotDetermined,
                Onpg = TestResult.NotDetermined,
                GammaGt = TestResult.Negative,
                SerogroupPcr = MeningoSerogroupPcr.NotDetermined,
                PorAPcr = NativeMaterialTestResult.NotDetermined,
                FetAPcr = NativeMaterialTestResult.NotDetermined
            };

            interpretation.Interpret(isolate);

            interpretation.Result.Report.Should().Contain(s => s.Contains("Kein Nachweis von Neisseria meningitidis."));
            interpretation.Typings.Should().NotContain(t => t.Attribute == "Identifikation");
            interpretation.Typings.Should().NotContain(t => t.Attribute == "MALDI-TOF (VITEK MS)");
            interpretation.TypingAttribute("β-Galaktosidase").Should().Be("n.d.");
            interpretation.TypingAttribute("γ-Glutamyltransferase").Should().Be("negativ");
            interpretation.Serogroup.Should().BeNull();

            AssertNoMeningococciFlagIsValid(interpretation);
            interpretation.Rule.Should().Be("StemInterpretation_NoNM_02");
        }
        [Test]
        public void IsolateMatchingStemRulePartialReport_ReturnsCorrespondingInterpretation()
        {
            var interpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                GrowthOnBloodAgar = Growth.TypicalGrowth,
                GrowthOnMartinLewisAgar = Growth.TypicalGrowth,
                Sending = new MeningoSending { SamplingLocation = InvasiveSamplingLocation },
                Oxidase = TestResult.Positive,
                Agglutination = MeningoSerogroupAgg.B,
                Onpg = TestResult.NotDetermined,
                GammaGt = TestResult.NotDetermined,
                SerogroupPcr = MeningoSerogroupPcr.NotDetermined,
                MaldiTofVitek = UnspecificTestResult.NotDetermined,
                PorAPcr = NativeMaterialTestResult.NotDetermined,
                FetAPcr = NativeMaterialTestResult.NotDetermined
            };

            interpretation.Interpret(isolate);

            interpretation.Result.Report.Should().Contain(s => s.Contains("meldepflichtig"));
            interpretation.Result.Report.Should().Contain(s => s.Contains("Meldekategorie dieses Befundes: Neisseria meningitidis, Serogruppe B"));

            interpretation.TypingAttribute("Identifikation").Should().Be("Neisseria meningitidis");
            interpretation.TypingAttribute("Serogruppe").Should().Be("B");
            interpretation.TypingAttribute("Empfindlichkeitstestung (Etest)").Should().Be("folgt");
            interpretation.Serogroup.Should().Be("B");
            
            AssertNoMeningococciFlagIsValid(interpretation);
            interpretation.Rule.Should().Be("StemInterpretation_PartialReport");
        }

        [TestCase("B", NativeMaterialTestResult.Negative, NativeMaterialTestResult.NotDetermined)]
        [TestCase("B", NativeMaterialTestResult.NotDetermined, NativeMaterialTestResult.NotDetermined)]
        [TestCase("B", NativeMaterialTestResult.Inhibitory, NativeMaterialTestResult.NotDetermined)]
        [TestCase("C", NativeMaterialTestResult.Negative, NativeMaterialTestResult.NotDetermined)]
        [TestCase("C", NativeMaterialTestResult.NotDetermined, NativeMaterialTestResult.NotDetermined)]
        [TestCase("C", NativeMaterialTestResult.Inhibitory, NativeMaterialTestResult.NotDetermined)]
        public void IsolateMatchingNativeMaterialRule1aOr2_ReturnsCorrespondingInterpretation(
            string serogroup,
            NativeMaterialTestResult cswyPcr,
            NativeMaterialTestResult realTimePcr)
        {
            var interpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                Sending = new MeningoSending { Material = MeningoMaterial.NativeMaterial },
                CsbPcr = "B" == serogroup ? NativeMaterialTestResult.Positive : GetRandomNegativeInhibitoryOrNotDetermined(),
                CscPcr = "C" == serogroup ? NativeMaterialTestResult.Positive : GetRandomNegativeInhibitoryOrNotDetermined(),
                CswyPcr = cswyPcr,
                PorAPcr = NativeMaterialTestResult.Positive,
                FetAPcr = NativeMaterialTestResult.Positive,
                PorAVr1 = "X",
                PorAVr2 = "Y",
                FetAVr = "Z",
                RealTimePcr = realTimePcr,
                RealTimePcrDevice = RealTimePcrDevice.NhsSacace
            };
            if (isolate.RealTimePcr == NativeMaterialTestResult.Positive)
            {
                isolate.RealTimePcrResult = RealTimePcrResult.NeisseriaMeningitidis;
            }

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
            interpretation.Serogroup.Should().Be(serogroup);

            if (realTimePcr == NativeMaterialTestResult.Positive)
            {
                interpretation.TypingAttribute("NHS Real-Time-PCR (Sacace)")
                    .Should().Be("Positiv für bekapselte Neisseria meningitidis. Der molekularbiologische Nachweis von Neisseria meningitidis beruht auf dem Nachweis des Kapseltransportgens ctrA mittels spezifischer Real-Time-PCR.");
            }
            else
            {
                interpretation.Typings.Should().NotContain(t =>
                    t.Attribute == "NHS Real-Time-PCR (Sacace)");
            }

            AssertNoMeningococciFlagIsValid(interpretation);
        }

        [TestCase("B", NativeMaterialTestResult.NotDetermined, TestName = "IsolateMatchingNativeMaterialRule1a_ReturnsCorrespondingInterpretation")]
        [TestCase("B", NativeMaterialTestResult.Positive, TestName = "IsolateMatchingNativeMaterialRule1b_ReturnsCorrespondingInterpretation")]
        [TestCase("C", NativeMaterialTestResult.NotDetermined, TestName = "IsolateMatchingNativeMaterialRule2_ReturnsCorrespondingInterpretation")]
        public void IsolateMatchingNativeMaterialRule1Or2_ReturnsCorrespondingInterpretation(
            string serogroup,
            NativeMaterialTestResult realTimePcr)
        {
            var interpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                Sending = new MeningoSending { Material = MeningoMaterial.NativeMaterial },
                CsbPcr = "B" == serogroup ? NativeMaterialTestResult.Positive : GetRandomNegativeInhibitoryOrNotDetermined(),
                CscPcr = "C" == serogroup ? NativeMaterialTestResult.Positive : GetRandomNegativeInhibitoryOrNotDetermined(),
                CswyPcr = GetRandomNegativeInhibitoryOrNotDetermined(),
                PorAPcr = NativeMaterialTestResult.Positive,
                FetAPcr = NativeMaterialTestResult.Positive,
                PorAVr1 = "X",
                PorAVr2 = "Y",
                FetAVr = "Z",
                RealTimePcr = realTimePcr,
                RealTimePcrDevice = RealTimePcrDevice.NshBdMax
            };
            if (isolate.RealTimePcr == NativeMaterialTestResult.Positive)
            {
                isolate.RealTimePcrResult = RealTimePcrResult.NeisseriaMeningitidis;
            }

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
            interpretation.Serogroup.Should().Be(serogroup);

            if (realTimePcr == NativeMaterialTestResult.Positive)
            {
                interpretation.TypingAttribute("NSH Real-Time-PCR (BD-MAX)")
                    .Should().Be("Positiv für bekapselte Neisseria meningitidis. Der molekularbiologische Nachweis von Neisseria meningitidis beruht auf dem Nachweis des Kapseltransportgens ctrA mittels spezifischer Real-Time-PCR.");
            }
            else
            {
                interpretation.Typings.Should().NotContain(t =>
                    t.Attribute == "NSH Real-Time-PCR (BD-MAX)");
            }

            AssertNoMeningococciFlagIsValid(interpretation);
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
                CsbPcr = GetRandomNegativeInhibitoryOrNotDetermined(),
                CscPcr = GetRandomNegativeInhibitoryOrNotDetermined(),
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
            interpretation.Serogroup.Should().Be(serogroup);
            AssertNoMeningococciFlagIsValid(interpretation);
        }

        [TestCase("Rule 06", NativeMaterialTestResult.Inhibitory, false, 0, "Negativ für bekapselte Neisseria meningitidis, bekapselte Haemophilus influenzae und Streptococcus pneumoniae.")]
        [TestCase("Rule 06", NativeMaterialTestResult.Negative, false, 0, "Negativ für bekapselte Neisseria meningitidis, bekapselte Haemophilus influenzae und Streptococcus pneumoniae.")]
        [TestCase("Rule 13", NativeMaterialTestResult.Inhibitory, true,  0, "Negativ für bekapselte Neisseria meningitidis, bekapselte Haemophilus influenzae und Streptococcus pneumoniae.")]
        [TestCase("Rule 13", NativeMaterialTestResult.Negative, true,  0, "Negativ für bekapselte Neisseria meningitidis, bekapselte Haemophilus influenzae und Streptococcus pneumoniae.")]
        [TestCase("Rule 07", NativeMaterialTestResult.Positive, false, RealTimePcrResult.NeisseriaMeningitidis, "Positiv für bekapselte Neisseria meningitidis. Der molekularbiologische Nachweis von Neisseria meningitidis beruht auf dem Nachweis des Kapseltransportgens ctrA mittels spezifischer Real-Time-PCR.")]
        [TestCase("Rule 14", NativeMaterialTestResult.Positive, true,  RealTimePcrResult.NeisseriaMeningitidis, "Positiv für bekapselte Neisseria meningitidis. Der molekularbiologische Nachweis von Neisseria meningitidis beruht auf dem Nachweis des Kapseltransportgens ctrA mittels spezifischer Real-Time-PCR.")]
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
                RealTimePcrResult = realTimePcrResult,
                RealTimePcrDevice = RealTimePcrDevice.NhsSacace
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
            interpretation.TypingAttribute("NHS Real-Time-PCR (Sacace)")
                .Should().Be(realTimePcrInterpretation);
            interpretation.TypingAttribute("PorA - Sequenztyp").Should().Contain("nicht amplifiziert");
            interpretation.TypingAttribute("FetA - Sequenztyp").Should().Contain("nicht amplifiziert");
            interpretation.Result.Comment.Should().BeNull();
            interpretation.Serogroup.Should().BeNull();
            AssertNoMeningococciFlagIsValid(interpretation);
        }

        [Test]
        public void IsolateMatchingNativeMaterialRule7ButWithMultipleRealTimePcrResults_ReturnsInconclusive()
        {
            var interpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                Sending = new MeningoSending { Material = MeningoMaterial.NativeMaterial },
                CsbPcr = GetRandomNegativeOrInhibitory(),
                CscPcr = GetRandomNegativeOrInhibitory(),
                CswyPcr = NativeMaterialTestResult.NotDetermined,
                PorAPcr = GetRandomNegativeOrInhibitory(),
                FetAPcr = GetRandomNegativeOrInhibitory(),
                RealTimePcr = NativeMaterialTestResult.Positive,
                RealTimePcrResult = RealTimePcrResult.NeisseriaMeningitidis | RealTimePcrResult.HaemophilusInfluenzae
            };

            interpretation.Interpret(isolate);

            interpretation.Result.Report.Should().Contain(s => s.Contains("Diskrepante Ergebnisse, bitte Datenbankeinträge kontrollieren."));
            interpretation.Typings.Should().BeEmpty();
            interpretation.Result.Comment.Should().BeNull();
            interpretation.Serogroup.Should().BeNull();
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
            interpretation.Serogroup.Should().BeNull();
            AssertNoMeningococciFlagIsValid(interpretation);
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

            interpretation.Typings.Should().NotContain(t => t.Attribute == "Molekulare Typisierung");

            interpretation.TypingAttribute("16S-rDNA-Nachweis").Should().Match(s => new List<string> {"negativ", "inhibitorisch" }.Contains(s));
            interpretation.TypingAttribute("PorA - Sequenztyp").Should().Contain("nicht amplifiziert");
            interpretation.TypingAttribute("FetA - Sequenztyp").Should().Contain("nicht amplifiziert");
            interpretation.Serogroup.Should().BeNull();
            AssertNoMeningococciFlagIsValid(interpretation);
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
            interpretation.Serogroup.Should().BeNull();
            interpretation.Rule.Should().Be("NativeMaterialInterpretation_19");
            AssertNoMeningococciFlagIsValid(interpretation);
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
            interpretation.Serogroup.Should().BeNull();
            AssertNoMeningococciFlagIsValid(interpretation);
        }

        [Test]
        public void IsolateMatchingNativeMaterialRule21_ReturnsCorrespondingInterpretation()
        {
            var interpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                Sending = new MeningoSending { Material = MeningoMaterial.NativeMaterial },
                CsbPcr = GetRandomNegativeOrInhibitory(),
                CscPcr = GetRandomNegativeOrInhibitory(),
                CswyPcr = GetRandomNegativeOrInhibitory(),
                PorAPcr = NativeMaterialTestResult.Positive,
                FetAPcr = NativeMaterialTestResult.Positive,
                PorAVr1 = "X",
                PorAVr2 = "Y",
                FetAVr = "Z",
                RibosomalRna16S = NativeMaterialTestResult.NotDetermined
            };

            interpretation.Interpret(isolate);

            
            interpretation.Result.Report.Should().Contain(s => s.Contains("Meningokokken-spezifische DNA konnte nachgewiesen werden."));
            interpretation.Result.Report.Should().Contain(s => s.Contains("meldepflichtig"));
            interpretation.Result.Report.Should().Contain(s => s.Contains("Meldekategorie dieses Befundes: Neisseria meningitidis."));
            interpretation.TypingAttribute("Molekulare Typisierung")
                .Should().Be("Die Serogruppen B, C,  W und Y-spezifischen csb-, csc-, csw- und csy-Gene wurden nicht nachgewiesen.");
            interpretation.TypingAttribute("PorA - Sequenztyp").Should().Be("X, Y");
            interpretation.TypingAttribute("FetA - Sequenztyp").Should().Be("Z");
            interpretation.Result.Comment.Should().Contain("Microsynth Seqlab");
            interpretation.Serogroup.Should().BeNull();
            AssertNoMeningococciFlagIsValid(interpretation);
        }

        [Test]
        public void IsolateMatchingNativeMaterialRule22_ReturnsCorrespondingInterpretation()
        {
            var interpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                Sending = new MeningoSending { Material = MeningoMaterial.NativeMaterial },
                CsbPcr = NativeMaterialTestResult.Positive,
                CscPcr = GetRandomNegativeInhibitoryOrNotDetermined(),
                CswyPcr = GetRandomNegativeInhibitoryOrNotDetermined(),
                PorAPcr = NativeMaterialTestResult.Negative,
                FetAPcr = NativeMaterialTestResult.Negative,
                RealTimePcr = NativeMaterialTestResult.Positive,
                RealTimePcrResult = RealTimePcrResult.NeisseriaMeningitidis,
                RealTimePcrDevice = RealTimePcrDevice.NhsSacace
            };

            interpretation.Interpret(isolate);

            interpretation.Result.Report.Should().Contain(s => s.Contains("Meningokokken-spezifische DNA konnte nachgewiesen werden."));
            interpretation.Result.Report.Should().Contain(s => s.Contains("meldepflichtig"));
            interpretation.Result.Report.Should().Contain(s => s.Contains("Meldekategorie dieses Befundes: Neisseria meningitidis, Serogruppe B"));
            interpretation.TypingAttribute("Molekulare Typisierung")
                .Should().Be("Das Serogruppe-B-spezifische csb-Gen wurde nachgewiesen.");
            interpretation.TypingAttribute("PorA - Sequenztyp").Should().Contain("nicht amplifiziert");
            interpretation.TypingAttribute("FetA - Sequenztyp").Should().Contain("nicht amplifiziert");
            interpretation.TypingAttribute("NHS Real-Time-PCR (Sacace)")
                .Should().Be("Positiv für bekapselte Neisseria meningitidis. Der molekularbiologische Nachweis von Neisseria meningitidis beruht auf dem Nachweis des Kapseltransportgens ctrA mittels spezifischer Real-Time-PCR.");
            interpretation.Serogroup.Should().Be("B");
            AssertNoMeningococciFlagIsValid(interpretation);
        }

        [Test]
        public void IsolateMatchingNativeMaterialRule23_ReturnsCorrespondingInterpretation()
        {
            var interpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                Sending = new MeningoSending { Material = MeningoMaterial.NativeMaterial },
                CsbPcr = NativeMaterialTestResult.NotDetermined,
                CscPcr = NativeMaterialTestResult.NotDetermined,
                CswyPcr = NativeMaterialTestResult.NotDetermined,
                PorAPcr = NativeMaterialTestResult.Negative,
                FetAPcr = NativeMaterialTestResult.Negative,
                RibosomalRna16S = NativeMaterialTestResult.NotDetermined,
                RealTimePcr = NativeMaterialTestResult.Negative,
                RealTimePcrDevice = RealTimePcrDevice.NhsSacace
            };

            interpretation.Interpret(isolate);

            interpretation.Result.Report.Should().Contain(s => s.Contains("Meningokokken- spezifische DNA konnte nicht nachgewiesen werden."));
            interpretation.Result.Report.Should().Contain(s => s.Contains("Kein Hinweis auf Neisseria meningitidis."));
            interpretation.Result.Report.Should().NotContain(s => s.Contains("meldepflichtig"));

            interpretation.Typings.Should().NotContain(t => t.Attribute == "Molekulare Typisierung");
            interpretation.TypingAttribute("PorA - Sequenztyp").Should().Contain("nicht amplifiziert");
            interpretation.TypingAttribute("FetA - Sequenztyp").Should().Contain("nicht amplifiziert");
            interpretation.TypingAttribute("NHS Real-Time-PCR (Sacace)")
                .Should().Be("Negativ für bekapselte Neisseria meningitidis, bekapselte Haemophilus influenzae und Streptococcus pneumoniae.");
            interpretation.Serogroup.Should().BeNull();
            AssertNoMeningococciFlagIsValid(interpretation);
        }

        [Test]
        public void IsolateMatchingNativeMaterialRule24_ReturnsCorrespondingInterpretation()
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
                RibosomalRna16S = NativeMaterialTestResult.NotDetermined,
                RealTimePcr = NativeMaterialTestResult.NotDetermined,
                RealTimePcrDevice = RealTimePcrDevice.NhsSacace
            };

            interpretation.Interpret(isolate);

            interpretation.Result.Report.Should().Contain(s => s.Contains("Meningokokken- spezifische DNA konnte nicht nachgewiesen werden."));
            interpretation.Result.Report.Should().Contain(s => s.Contains("Kein Hinweis auf Neisseria meningitidis."));
            interpretation.Result.Report.Should().NotContain(s => s.Contains("meldepflichtig"));

            interpretation.TypingAttribute("Molekulare Typisierung")
                .Should().Be("Die Serogruppen B und C-spezifischen csb- und csc-Gene wurden nicht nachgewiesen.");
            interpretation.Typings.Should().NotContain(t => t.Attribute == "NHS Real-Time-PCR (Sacace)");
            interpretation.TypingAttribute("PorA - Sequenztyp").Should().Contain("nicht amplifiziert");
            interpretation.TypingAttribute("FetA - Sequenztyp").Should().Contain("nicht amplifiziert");
            interpretation.Serogroup.Should().BeNull();
             AssertNoMeningococciFlagIsValid(interpretation);
        }

        [Test]
        public void IsolateMatchingNativeMaterialRule25_ReturnsCorrespondingInterpretation()
        {
            var interpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                Sending = new MeningoSending { Material = MeningoMaterial.NativeMaterial },
                CsbPcr = GetRandomNegativeInhibitoryOrNotDetermined(),
                CscPcr = NativeMaterialTestResult.Positive,
                CswyPcr = GetRandomNegativeInhibitoryOrNotDetermined(),
                PorAPcr = NativeMaterialTestResult.Negative,
                FetAPcr = NativeMaterialTestResult.Negative,
                RealTimePcr = NativeMaterialTestResult.Positive,
                RealTimePcrResult = RealTimePcrResult.NeisseriaMeningitidis,
                RealTimePcrDevice = RealTimePcrDevice.NhsSacace
            };

            interpretation.Interpret(isolate);

            interpretation.Result.Report.Should().Contain(s => s.Contains("Meningokokken-spezifische DNA konnte nachgewiesen werden."));
            interpretation.Result.Report.Should().Contain(s => s.Contains("meldepflichtig"));
            interpretation.Result.Report.Should().Contain(s => s.Contains("Meldekategorie dieses Befundes: Neisseria meningitidis, Serogruppe C"));
            interpretation.TypingAttribute("Molekulare Typisierung")
                .Should().Be("Das Serogruppe-C-spezifische csc-Gen wurde nachgewiesen.");
            interpretation.TypingAttribute("PorA - Sequenztyp").Should().Contain("nicht amplifiziert");
            interpretation.TypingAttribute("FetA - Sequenztyp").Should().Contain("nicht amplifiziert");
            interpretation.TypingAttribute("NHS Real-Time-PCR (Sacace)")
                .Should().Be("Positiv für bekapselte Neisseria meningitidis. Der molekularbiologische Nachweis von Neisseria meningitidis beruht auf dem Nachweis des Kapseltransportgens ctrA mittels spezifischer Real-Time-PCR.");
            interpretation.Serogroup.Should().Be("C");
            AssertNoMeningococciFlagIsValid(interpretation);
        }

        [Test]
        public void IsolateMatchingNativeMaterialRule26_ReturnsCorrespondingInterpretation()
        {
            var interpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                Sending = new MeningoSending { Material = MeningoMaterial.NativeMaterial },
                CsbPcr = GetRandomNegativeInhibitoryOrNotDetermined(),
                CscPcr = GetRandomNegativeInhibitoryOrNotDetermined(),
                CswyPcr = NativeMaterialTestResult.Positive,
                CswyAllele = CswyAllel.Allele1,
                PorAPcr = NativeMaterialTestResult.Negative,
                FetAPcr = NativeMaterialTestResult.Negative,
                RealTimePcr = NativeMaterialTestResult.Positive,
                RealTimePcrResult = RealTimePcrResult.NeisseriaMeningitidis,
                RealTimePcrDevice = RealTimePcrDevice.NhsSacace
            };

            interpretation.Interpret(isolate);

            interpretation.Result.Report.Should().Contain(s => s.Contains("Meningokokken-spezifische DNA konnte nachgewiesen werden."));
            interpretation.Result.Report.Should().Contain(s => s.Contains("meldepflichtig"));
            interpretation.Result.Report.Should().Contain(s => s.Contains("Meldekategorie dieses Befundes: Neisseria meningitidis, Serogruppe W"));
            interpretation.TypingAttribute("Molekulare Typisierung")
                .Should().Be("Das Serogruppe-W-spezifische csw-Gen wurde nachgewiesen.");
            interpretation.TypingAttribute("PorA - Sequenztyp").Should().Contain("nicht amplifiziert");
            interpretation.TypingAttribute("FetA - Sequenztyp").Should().Contain("nicht amplifiziert");
            interpretation.TypingAttribute("NHS Real-Time-PCR (Sacace)")
                .Should().Be("Positiv für bekapselte Neisseria meningitidis. Der molekularbiologische Nachweis von Neisseria meningitidis beruht auf dem Nachweis des Kapseltransportgens ctrA mittels spezifischer Real-Time-PCR.");
            interpretation.Serogroup.Should().Be("W");
            AssertNoMeningococciFlagIsValid(interpretation);
        }

        [Test]
        public void IsolateMatchingNativeMaterialRule27_ReturnsCorrespondingInterpretation()
        {
            var interpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                Sending = new MeningoSending { Material = MeningoMaterial.NativeMaterial },
                CsbPcr = GetRandomNegativeInhibitoryOrNotDetermined(),
                CscPcr = GetRandomNegativeInhibitoryOrNotDetermined(),
                CswyPcr = NativeMaterialTestResult.Positive,
                CswyAllele = CswyAllel.Allele2,
                PorAPcr = NativeMaterialTestResult.Negative,
                FetAPcr = NativeMaterialTestResult.Negative,
                RealTimePcr = NativeMaterialTestResult.Positive,
                RealTimePcrResult = RealTimePcrResult.NeisseriaMeningitidis,
                RealTimePcrDevice = RealTimePcrDevice.NhsSacace
            };

            interpretation.Interpret(isolate);

            interpretation.Result.Report.Should().Contain(s => s.Contains("Meningokokken-spezifische DNA konnte nachgewiesen werden."));
            interpretation.Result.Report.Should().Contain(s => s.Contains("meldepflichtig"));
            interpretation.Result.Report.Should().Contain(s => s.Contains("Meldekategorie dieses Befundes: Neisseria meningitidis, Serogruppe Y"));
            interpretation.TypingAttribute("Molekulare Typisierung")
                .Should().Be("Das Serogruppe-Y-spezifische csy-Gen wurde nachgewiesen.");
            interpretation.TypingAttribute("PorA - Sequenztyp").Should().Contain("nicht amplifiziert");
            interpretation.TypingAttribute("FetA - Sequenztyp").Should().Contain("nicht amplifiziert");
            interpretation.TypingAttribute("NHS Real-Time-PCR (Sacace)")
                .Should().Be("Positiv für bekapselte Neisseria meningitidis. Der molekularbiologische Nachweis von Neisseria meningitidis beruht auf dem Nachweis des Kapseltransportgens ctrA mittels spezifischer Real-Time-PCR.");
            interpretation.Serogroup.Should().Be("Y");
            AssertNoMeningococciFlagIsValid(interpretation);
        }

        [Test]
        public void IsolateMatchingNativeMaterialRule28_ReturnsCorrespondingInterpretation()
        {
            var interpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                Sending = new MeningoSending { Material = MeningoMaterial.NativeMaterial },
                CsbPcr = GetRandomNegativeInhibitoryOrNotDetermined(),
                CscPcr = GetRandomNegativeInhibitoryOrNotDetermined(),
                CswyPcr = NativeMaterialTestResult.Positive,
                CswyAllele = CswyAllel.Allele3,
                PorAPcr = NativeMaterialTestResult.Negative,
                FetAPcr = NativeMaterialTestResult.Negative,
                RealTimePcr = NativeMaterialTestResult.Positive,
                RealTimePcrResult = RealTimePcrResult.NeisseriaMeningitidis,
                RealTimePcrDevice = RealTimePcrDevice.NhsSacace
            };

            interpretation.Interpret(isolate);

            interpretation.Result.Report.Should().Contain(s => s.Contains("Meningokokken-spezifische DNA konnte nachgewiesen werden."));
            interpretation.Result.Report.Should().Contain(s => s.Contains("meldepflichtig"));
            interpretation.Result.Report.Should().Contain(s => s.Contains("Meldekategorie dieses Befundes: Neisseria meningitidis, Serogruppe W/Y"));
            interpretation.TypingAttribute("Molekulare Typisierung")
                .Should().Be("Das Serogruppen-W/Y-spezifische csw/csy-Gen wurde nachgewiesen.");
            interpretation.TypingAttribute("PorA - Sequenztyp").Should().Contain("nicht amplifiziert");
            interpretation.TypingAttribute("FetA - Sequenztyp").Should().Contain("nicht amplifiziert");
            interpretation.TypingAttribute("NHS Real-Time-PCR (Sacace)")
                .Should().Be("Positiv für bekapselte Neisseria meningitidis. Der molekularbiologische Nachweis von Neisseria meningitidis beruht auf dem Nachweis des Kapseltransportgens ctrA mittels spezifischer Real-Time-PCR.");
            interpretation.Serogroup.Should().Be("W/Y");
            interpretation.Rule.Should().Be("NativeMaterialInterpretation_28");
            AssertNoMeningococciFlagIsValid(interpretation);
        }

        [TestCase(NativeMaterialTestResult.Negative, NativeMaterialTestResult.Positive, NativeMaterialTestResult.Positive, TestName = "IsolateMatchingNativeMaterialRule29a_ReturnsCorrespondingInterpretation")]
        [TestCase(NativeMaterialTestResult.Negative, NativeMaterialTestResult.Positive, NativeMaterialTestResult.NotDetermined, TestName = "IsolateMatchingNativeMaterialRule29b_ReturnsCorrespondingInterpretation")]
        [TestCase(NativeMaterialTestResult.Positive, NativeMaterialTestResult.Positive, NativeMaterialTestResult.Positive, TestName = "IsolateMatchingNativeMaterialRule30a_ReturnsCorrespondingInterpretation")]
        [TestCase(NativeMaterialTestResult.Positive, NativeMaterialTestResult.Positive, NativeMaterialTestResult.Positive, TestName = "IsolateMatchingNativeMaterialRule30b_ReturnsCorrespondingInterpretation")]
        [TestCase(NativeMaterialTestResult.Positive, NativeMaterialTestResult.Negative, NativeMaterialTestResult.Positive, TestName = "IsolateMatchingNativeMaterialRule31a_ReturnsCorrespondingInterpretation")]
        [TestCase(NativeMaterialTestResult.Positive, NativeMaterialTestResult.Negative, NativeMaterialTestResult.Positive, TestName = "IsolateMatchingNativeMaterialRule31b_ReturnsCorrespondingInterpretation")]
        public void IsolateMatchingNativeMaterialRule29To31_ReturnsCorrespondingInterpretation(
            NativeMaterialTestResult porA,
            NativeMaterialTestResult fetA,
            NativeMaterialTestResult realTimePcr)
        {
            var interpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                Sending = new MeningoSending { Material = MeningoMaterial.NativeMaterial },
                CsbPcr = NativeMaterialTestResult.Positive,
                CscPcr = GetRandomNegativeInhibitoryOrNotDetermined(),
                CswyPcr = GetRandomNegativeInhibitoryOrNotDetermined(),
                PorAPcr = porA,
                FetAPcr = fetA,
                RealTimePcr = realTimePcr,
                RealTimePcrDevice = RealTimePcrDevice.NhsSacace,
                PorAVr1 = "X",
                PorAVr2 = "Y",
                FetAVr = "Z",
            };
            if (isolate.RealTimePcr == NativeMaterialTestResult.Positive)
            {
                isolate.RealTimePcrResult = RealTimePcrResult.NeisseriaMeningitidis;
            }

            interpretation.Interpret(isolate);

            interpretation.Result.Report.Should().Contain(s => s.Contains("Meningokokken-spezifische DNA konnte nachgewiesen werden."));
            interpretation.Result.Report.Should().Contain(s => s.Contains("meldepflichtig"));
            interpretation.Result.Report.Should().Contain(s => s.Contains("Meldekategorie dieses Befundes: Neisseria meningitidis, Serogruppe B"));
            interpretation.TypingAttribute("Molekulare Typisierung").Should().Be("Das Serogruppe-B-spezifische csb-Gen wurde nachgewiesen.");

            if (porA == NativeMaterialTestResult.Negative)
            {
                interpretation.TypingAttribute("PorA - Sequenztyp").Should().Contain("nicht amplifiziert");
            }
            else
            {
                interpretation.TypingAttribute("PorA - Sequenztyp").Should().Be("X, Y");
                interpretation.Result.Comment.Should().Contain("Microsynth Seqlab");
            }

            if (fetA == NativeMaterialTestResult.Negative)
            {
                interpretation.TypingAttribute("FetA - Sequenztyp").Should().Contain("nicht amplifiziert");
            }
            else
            {
                interpretation.TypingAttribute("FetA - Sequenztyp").Should().Be("Z");
                interpretation.Result.Comment.Should().Contain("Microsynth Seqlab");
            }

            if (porA == NativeMaterialTestResult.Negative && fetA == NativeMaterialTestResult.Negative)
            {
                interpretation.Result.Comment.Should().BeNull();
            }

            if (realTimePcr == NativeMaterialTestResult.Positive)
            {
                interpretation.TypingAttribute("NHS Real-Time-PCR (Sacace)")
                    .Should().Be("Positiv für bekapselte Neisseria meningitidis. Der molekularbiologische Nachweis von Neisseria meningitidis beruht auf dem Nachweis des Kapseltransportgens ctrA mittels spezifischer Real-Time-PCR.");
            }
            else
            {
                interpretation.Typings.Should().NotContain(t =>
                    t.Attribute == "NHS Real-Time-PCR (Sacace)");
            }
            interpretation.Serogroup.Should().Be("B");
            AssertNoMeningococciFlagIsValid(interpretation);
        }

        [Test]
        public void IsolateMatchingNativeMaterialRule22_ReturnsCorrespondingRule()
        {
            //Case MZ080/21
            var interpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                Sending = new MeningoSending { Material = MeningoMaterial.NativeMaterial, SamplingLocation = InvasiveSamplingLocation},
                                
                CsbPcr = NativeMaterialTestResult.Positive,
                CscPcr = NativeMaterialTestResult.NotDetermined,
                CswyPcr = NativeMaterialTestResult.NotDetermined,
                PorAPcr = NativeMaterialTestResult.Negative,
                FetAPcr = NativeMaterialTestResult.Negative,
                RibosomalRna16S = NativeMaterialTestResult.NotDetermined,
                RealTimePcr = NativeMaterialTestResult.Positive,
                RealTimePcrResult = RealTimePcrResult.NeisseriaMeningitidis
            };

            interpretation.Interpret(isolate);

            interpretation.Serogroup.Should().Be("B");
            interpretation.Rule.Should().Be("NativeMaterialInterpretation_22");
            AssertNoMeningococciFlagIsValid(interpretation);
        }

        [Test]
        public void IsolateMatchingNativeMaterialRule01_ReturnsCorrespondingRule()
        {
            //Case MZ043/21
            var interpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                Sending = new MeningoSending { Material = MeningoMaterial.IsolatedDna, SamplingLocation = MeningoSamplingLocation.Liquor},
                                
                CsbPcr = NativeMaterialTestResult.Positive,
                CscPcr = NativeMaterialTestResult.NotDetermined,
                CswyPcr = NativeMaterialTestResult.NotDetermined,
                PorAPcr = NativeMaterialTestResult.Positive,
                PorAVr1 = "19",
                PorAVr2 = "15-1",
                FetAPcr = NativeMaterialTestResult.Positive,
                FetAVr = "5-1",
                RibosomalRna16S = NativeMaterialTestResult.NotDetermined,
                RealTimePcr = NativeMaterialTestResult.NotDetermined,
            };

            interpretation.Interpret(isolate);

            interpretation.Serogroup.Should().Be("B");
            interpretation.Rule.Should().Be("NativeMaterialInterpretation_01a");
            AssertNoMeningococciFlagIsValid(interpretation);
        }

        [Test]
        public void IsolateMatchingNativeMaterialRule32_ReturnsCorrespondingRule()
        {
            var interpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate
            {
                Sending = new MeningoSending { Material = MeningoMaterial.NativeMaterial, SamplingLocation = InvasiveSamplingLocation},
                CsbPcr = NativeMaterialTestResult.Negative,
                CscPcr = NativeMaterialTestResult.Negative,
                CswyPcr = NativeMaterialTestResult.Negative,
                PorAPcr = NativeMaterialTestResult.Negative,
                FetAPcr = NativeMaterialTestResult.Negative,
                RibosomalRna16S = NativeMaterialTestResult.Negative,
                RealTimePcr = NativeMaterialTestResult.Negative,
            };

            interpretation.Interpret(isolate);

            interpretation.TypingAttribute("Molekulare Typisierung")
                .Should().Be("Die Serogruppen B, C, W und Y-spezifischen csb-, csc-, csw- und csy-Gene wurden nicht nachgewiesen.");
            interpretation.TypingAttribute("PorA - Sequenztyp").Should().Contain("nicht amplifiziert");
            interpretation.TypingAttribute("FetA - Sequenztyp").Should().Contain("nicht amplifiziert");
            interpretation.Result.Report.Should().Contain(s => s.Contains("Kein Hinweis auf Neisseria meningitidis."));

            AssertNoMeningococciFlagIsValid(interpretation);
        }

        // ---- specs/007 verification (2026-11-08) ----

        private const string DiscrepancyLine = "Diskrepante Ergebnisse, bitte Datenbankeinträge kontrollieren.";
        private const string SequencingComment = "Die technische Durchführung der Sequenzierung erfolgte durch den Unterauftragnehmer Microsynth Seqlab.";

        private static class Attr
        {
            public const string Identification = "Identifikation";
            public const string Serogroup = "Serogruppe";
            public const string Agglutination = "Agglutination";
            public const string Serogenogroup = "Serogenogruppe";
            public const string PorA = "PorA - Sequenztyp";
            public const string FetA = "FetA - Sequenztyp";
            public const string GrowthMartinLewis = "Wachstum auf Martin-Lewis-Agar";
            public const string GrowthBlood = "Wachstum auf Blutagar";
            public const string Onpg = "β-Galaktosidase";
            public const string GammaGt = "γ-Glutamyltransferase";
            public const string MaldiVitek = "MALDI-TOF (VITEK MS)";
            public const string MaldiBiotyper = "MALDI-TOF (Biotyper)";
            public const string ETest = "Empfindlichkeitstestung (Etest)";
            public const string RealTimePcr = "NSH Real-Time-PCR (BD-MAX)";
            public const string Rna16S = "16S-rDNA-Nachweis";
            public const string BestMatch = "Ergebnis der DNA-Sequenzierung";
            public const string MolecularTyping = "Molekulare Typisierung";
        }

        public class RuleCase
        {
            public string Rule { get; set; }
            public MeningoIsolate Isolate { get; set; }
            public string[] TypingAttributes { get; set; }
            public int ReportLines { get; set; }
            public bool? HasComment { get; set; }
            public bool? NoMeningococci { get; set; }
            public string Serogroup { get; set; }
            public override string ToString() => Rule;
        }

        private static MeningoIsolate StemIsolate(bool invasive, Gr blood, Gr martinLewis, Tr oxidase, Ag agglutination, Tr onpg, Tr gammaGt,
            Pc serogroupPcr, Nm porA, Nm fetA, Ut maldiTofVitek = Ut.NotDetermined, Ut maldiTofBiotyper = Ut.NotDetermined,
            MeningoMaterial material = MeningoMaterial.VitalStem)
        {
            return new MeningoIsolate
            {
                Sending = new MeningoSending { Material = material, SamplingLocation = invasive ? InvasiveSamplingLocation : NonInvasiveSamplingLocation },
                GrowthOnBloodAgar = blood,
                GrowthOnMartinLewisAgar = martinLewis,
                Oxidase = oxidase,
                Agglutination = agglutination,
                Onpg = onpg,
                GammaGt = gammaGt,
                SerogroupPcr = serogroupPcr,
                MaldiTofVitek = maldiTofVitek,
                MaldiTofVitekBestMatch = "SV-007 Vitek",
                MaldiTofBiotyper = maldiTofBiotyper,
                MaldiTofBiotyperBestMatch = "SV-007 Biotyper",
                PorAPcr = porA,
                FetAPcr = fetA,
                PorAVr1 = "X",
                PorAVr2 = "Y",
                FetAVr = "Z"
            };
        }

        private static MeningoIsolate NativeIsolate(Nm csb, Nm csc, Nm cswy, Nm porA, Nm fetA, Nm rna16S, Nm realTimePcr,
            Al allele = 0, Rt realTimePcrResult = 0, string bestMatch = null, MeningoMaterial material = MeningoMaterial.NativeMaterial)
        {
            return new MeningoIsolate
            {
                Sending = new MeningoSending { Material = material, SamplingLocation = InvasiveSamplingLocation },
                CsbPcr = csb,
                CscPcr = csc,
                CswyPcr = cswy,
                CswyAllele = allele,
                PorAPcr = porA,
                FetAPcr = fetA,
                RibosomalRna16S = rna16S,
                RibosomalRna16SBestMatch = bestMatch,
                RealTimePcr = realTimePcr,
                RealTimePcrResult = realTimePcrResult,
                PorAVr1 = "X",
                PorAVr2 = "Y",
                FetAVr = "Z"
            };
        }

        private static RuleCase Case(string rule, MeningoIsolate isolate, int reportLines, bool? hasComment, bool? noMeningococci, string serogroup, params string[] typingAttributes)
        {
            return new RuleCase
            {
                Rule = rule,
                Isolate = isolate,
                ReportLines = reportLines,
                HasComment = hasComment,
                NoMeningococci = noMeningococci,
                Serogroup = serogroup,
                TypingAttributes = typingAttributes
            };
        }

        private static IEnumerable<TestCaseData> RepresentativeRuleInputs()
        {
            const Gr No = Gr.No, Typ = Gr.TypicalGrowth, Atyp = Gr.ATypicalGrowth;
            const Tr Nd = Tr.NotDetermined, Neg = Tr.Negative, Pos = Tr.Positive;
            const Nm NmNd = Nm.NotDetermined, NmNeg = Nm.Negative, NmPos = Nm.Positive;
            const string S = "StemInterpretation_", N = "NativeMaterialInterpretation_";
            string[] idAggSeroPf = { Attr.Identification, Attr.Agglutination, Attr.Serogenogroup, Attr.PorA, Attr.FetA };
            string[] idAggSero = { Attr.Identification, Attr.Agglutination, Attr.Serogenogroup };
            string[] idSeroAndGenoPf = { Attr.Identification, Attr.Serogroup, Attr.Serogenogroup, Attr.PorA, Attr.FetA };
            string[] idGenoPf = { Attr.Identification, Attr.Serogenogroup, Attr.PorA, Attr.FetA };
            string[] idSeroPf = { Attr.Identification, Attr.Serogroup, Attr.PorA, Attr.FetA };
            string[] growthBiochemMaldi = { Attr.GrowthMartinLewis, Attr.GrowthBlood, Attr.Onpg, Attr.GammaGt, Attr.MaldiVitek };
            string[] rtMolPf = { Attr.RealTimePcr, Attr.MolecularTyping, Attr.PorA, Attr.FetA };
            string[] molPf = { Attr.MolecularTyping, Attr.PorA, Attr.FetA };

            // Cultured-isolate rules in declaration order. HasComment null: rules 11, 14, 41 (DEC-005 / DATA-018, not asserted).
            var cases = new List<RuleCase>
            {
                Case(S + "20", StemIsolate(true, Typ, Typ, Pos, Ag.Poly, Neg, Pos, Pc.Cnl, NmPos, NmPos), 2, true, false, "cnl", idAggSeroPf),
                Case(S + "22", StemIsolate(false, Typ, Typ, Pos, Ag.Poly, Neg, Pos, Pc.Cnl, NmPos, NmPos), 1, true, false, "cnl", idAggSeroPf),
                Case(S + "24", StemIsolate(true, Typ, Typ, Pos, Ag.Auto, Neg, Pos, Pc.Cnl, NmPos, NmPos), 2, true, false, "cnl", idAggSeroPf),
                Case(S + "26", StemIsolate(false, Typ, Typ, Pos, Ag.Auto, Neg, Pos, Pc.Cnl, NmPos, NmPos), 0, true, false, "cnl", idAggSeroPf),
                Case(S + "01", StemIsolate(false, No, No, Nd, Ag.NotDetermined, Nd, Nd, Pc.NotDetermined, NmNd, NmNd), 3, false, true, null),
                Case(S + "02", StemIsolate(true, No, No, Nd, Ag.NotDetermined, Nd, Nd, Pc.C, NmPos, NmPos), 4, true, false, "C", idGenoPf),
                Case(S + "03", StemIsolate(false, Atyp, No, Neg, Ag.NotDetermined, Pos, Neg, Pc.NotDetermined, NmNd, NmNd), 1, false, true, null, Attr.GrowthMartinLewis, Attr.GrowthBlood),
                Case(S + "04", StemIsolate(false, Atyp, No, Neg, Ag.NotDetermined, Neg, Neg, Pc.NotDetermined, NmNd, NmNd, Ut.Determined), 1, false, true, null, growthBiochemMaldi),
                Case(S + "05", StemIsolate(true, Atyp, No, Neg, Ag.NotDetermined, Neg, Pos, Pc.NotDetermined, NmNd, NmNd, Ut.Determined), 1, false, true, null, growthBiochemMaldi),
                Case(S + "06", StemIsolate(false, Typ, Typ, Pos, Ag.X, Neg, Pos, Pc.NotDetermined, NmNd, NmNd), 1, false, false, "X", Attr.Identification, Attr.Serogroup),
                Case(S + "07", StemIsolate(true, Typ, Typ, Pos, Ag.E, Neg, Pos, Pc.NotDetermined, NmNd, NmNd), 2, false, false, "E", Attr.Identification, Attr.Serogroup),
                Case(S + "08", StemIsolate(true, Typ, Typ, Nd, Ag.Auto, Neg, Pos, Pc.C, NmPos, NmPos), 2, true, false, "C", idSeroAndGenoPf),
                Case(S + "09", StemIsolate(true, Typ, Typ, Pos, Ag.Poly, Neg, Pos, Pc.C, NmPos, NmPos), 2, true, false, "C", idSeroAndGenoPf),
                Case(S + "10", StemIsolate(false, Typ, Typ, Nd, Ag.Auto, Neg, Pos, Pc.C, NmPos, NmPos), 1, true, false, "C", idSeroAndGenoPf),
                Case(S + "11", StemIsolate(false, Typ, Typ, Nd, Ag.Poly, Neg, Pos, Pc.C, NmPos, NmPos), 1, null, false, "C", idSeroAndGenoPf),
                Case(S + "12", StemIsolate(false, Typ, Typ, Nd, Ag.Auto, Neg, Pos, Pc.NotDetermined, NmNd, NmNd), 0, false, false, "NG", Attr.Identification, Attr.Serogroup),
                Case(S + "13", StemIsolate(false, Typ, Typ, Nd, Ag.Poly, Neg, Pos, Pc.NotDetermined, NmNd, NmNd), 0, false, false, "NG", Attr.Identification, Attr.Serogroup),
                Case(S + "14", StemIsolate(true, Typ, Atyp, Pos, Ag.B, Neg, Neg, Pc.NotDetermined, NmPos, NmPos), 2, null, false, "B", idSeroPf),
                Case(S + "15", StemIsolate(true, Typ, Typ, Pos, Ag.Negative, Neg, Pos, Pc.Cnl, NmNd, NmNd), 2, false, false, "cnl", idAggSero),
                Case(S + "16", StemIsolate(true, Typ, Typ, Pos, Ag.Negative, Neg, Pos, Pc.Cnl, NmPos, NmPos), 2, true, false, "cnl", idAggSeroPf),
                Case(S + "17", StemIsolate(false, Typ, Typ, Pos, Ag.Negative, Neg, Pos, Pc.Cnl, NmNd, NmNd), 1, false, false, "cnl", idAggSero),
                Case(S + "18", StemIsolate(false, Typ, Typ, Pos, Ag.Negative, Neg, Pos, Pc.Cnl, NmPos, NmPos), 1, true, false, "cnl", idAggSeroPf),
                Case(S + "19", StemIsolate(true, Typ, Typ, Pos, Ag.Poly, Neg, Pos, Pc.Cnl, NmNd, NmNd), 2, false, false, "cnl", idAggSero),
                Case(S + "21", StemIsolate(false, Typ, Typ, Pos, Ag.Poly, Neg, Pos, Pc.Cnl, NmNd, NmNd), 1, false, false, "cnl", idAggSero),
                Case(S + "23", StemIsolate(true, Typ, Typ, Pos, Ag.Auto, Neg, Pos, Pc.Cnl, NmNd, NmNd), 2, false, false, "cnl", idAggSero),
                Case(S + "25", StemIsolate(false, Typ, Typ, Pos, Ag.Auto, Neg, Pos, Pc.Cnl, NmNd, NmNd), 0, false, false, "cnl", idAggSero),
                Case(S + "27", StemIsolate(true, No, No, Nd, Ag.NotDetermined, Nd, Nd, Pc.NotDetermined, NmNd, NmNd), 3, false, true, null),
                Case(S + "28", StemIsolate(true, No, No, Nd, Ag.NotDetermined, Nd, Nd, Pc.C, NmPos, NmNeg), 4, true, false, "C", idGenoPf),
                Case(S + "29", StemIsolate(true, No, No, Nd, Ag.NotDetermined, Nd, Nd, Pc.C, NmNeg, NmPos), 4, true, false, "C", idGenoPf),
                Case(S + "30", StemIsolate(true, No, No, Nd, Ag.NotDetermined, Nd, Nd, Pc.Negative, NmPos, NmPos), 4, true, false, "NG", idGenoPf),
                Case(S + "31", StemIsolate(true, No, No, Nd, Ag.NotDetermined, Nd, Nd, Pc.Negative, NmPos, NmNeg), 4, true, false, "NG", idGenoPf),
                Case(S + "32", StemIsolate(true, No, No, Nd, Ag.NotDetermined, Nd, Nd, Pc.Negative, NmNeg, NmPos), 4, true, false, "NG", idGenoPf),
                Case(S + "33", StemIsolate(false, Typ, Typ, Pos, Ag.Negative, Neg, Pos, Pc.NotDetermined, NmNd, NmNd), 1, false, false, "NG", Attr.Identification, Attr.Agglutination),
                Case(S + "34", StemIsolate(true, Atyp, Atyp, Nd, Ag.NotDetermined, Neg, Neg, Pc.NotDetermined, NmNd, NmNd, Ut.Determined), 1, false, true, null, growthBiochemMaldi),
                Case(S + "35", StemIsolate(true, No, No, Nd, Ag.NotDetermined, Nd, Nd, Pc.C, NmNeg, NmNeg), 3, false, false, "C", idGenoPf),
                Case(S + "36", StemIsolate(false, Typ, Typ, Pos, Ag.B, Neg, Pos, Pc.NotDetermined, NmPos, NmPos), 1, true, false, "B", idSeroPf),
                Case(S + "37", StemIsolate(false, Typ, Typ, Pos, Ag.Poly, Neg, Pos, Pc.Negative, NmPos, NmPos), 1, true, false, "NG", idSeroPf),
                Case(S + "38", StemIsolate(false, Typ, Typ, Pos, Ag.Auto, Neg, Pos, Pc.NotDetermined, NmPos, NmPos), 1, true, false, "NG", idSeroPf),
                Case(S + "40", StemIsolate(false, Typ, Typ, Pos, Ag.Negative, Neg, Pos, Pc.C, NmNd, NmNd), 1, false, false, "C", Attr.Identification, Attr.Serogenogroup),
                Case(S + "41", StemIsolate(false, Typ, Typ, Pos, Ag.Negative, Neg, Pos, Pc.NotDetermined, NmPos, NmPos), 1, null, false, "NG", Attr.Identification, Attr.Agglutination, Attr.PorA, Attr.FetA),
                Case(S + "43", StemIsolate(false, Typ, Typ, Pos, Ag.NotDetermined, Neg, Pos, Pc.NotDetermined, NmNd, NmNd), 1, false, false, null, Attr.Identification),
                Case(S + "NoNM_01", StemIsolate(false, Atyp, No, Nd, Ag.NotDetermined, Pos, Neg, Pc.NotDetermined, NmNd, NmNd, Ut.Determined), 1, false, true, null, growthBiochemMaldi),
                Case(S + "NoNM_02", StemIsolate(true, Atyp, Atyp, Nd, Ag.NotDetermined, Pos, Pos, Pc.NotDetermined, NmNd, NmNd), 1, false, true, null, Attr.GrowthMartinLewis, Attr.GrowthBlood, Attr.Onpg, Attr.GammaGt),
                Case(S + "PartialReport", StemIsolate(true, Typ, Typ, Pos, Ag.B, Nd, Nd, Pc.NotDetermined, NmNd, NmNd), 2, false, false, "B", Attr.Identification, Attr.Serogroup, Attr.ETest),

                // Native-material rules in declaration order; 30a/30b are shadowed by 01b/01a (DEC-005) and have no case.
                // NoMeningococci null: rule 32 (DEC-005, not asserted).
                Case(N + "01a", NativeIsolate(NmPos, NmNd, NmNd, NmPos, NmPos, NmNd, NmNd), 3, true, false, "B", molPf),
                Case(N + "01b", NativeIsolate(NmPos, NmNd, NmNd, NmPos, NmPos, NmNd, NmPos, realTimePcrResult: Rt.NeisseriaMeningitidis), 3, true, false, "B", rtMolPf),
                Case(N + "02", NativeIsolate(NmNd, NmPos, NmNd, NmPos, NmPos, NmNd, NmNd), 3, true, false, "C", molPf),
                Case(N + "03", NativeIsolate(NmNd, NmNd, NmPos, NmPos, NmPos, NmNd, NmNd, Al.Allele1), 3, true, false, "W", molPf),
                Case(N + "04", NativeIsolate(NmNd, NmNd, NmPos, NmPos, NmPos, NmNd, NmNd, Al.Allele2), 3, true, false, "Y", molPf),
                Case(N + "05", NativeIsolate(NmNd, NmNd, NmPos, NmPos, NmPos, NmNd, NmNd, Al.Allele3), 3, true, false, "W/Y", molPf),
                Case(N + "06", NativeIsolate(NmNeg, NmNeg, NmNd, NmNeg, NmNeg, NmNd, NmNeg), 2, false, true, null, rtMolPf),
                Case(N + "07", NativeIsolate(NmNeg, NmNeg, NmNd, NmNeg, NmNeg, NmNd, NmPos, realTimePcrResult: Rt.NeisseriaMeningitidis), 3, false, false, null, rtMolPf),
                Case(N + "08", NativeIsolate(NmNeg, NmNeg, NmNd, NmNeg, NmNeg, NmNd, NmPos, realTimePcrResult: Rt.StreptococcusPneumoniae), 2, false, true, null, rtMolPf),
                Case(N + "09", NativeIsolate(NmNeg, NmNeg, NmNd, NmNeg, NmNeg, NmNd, NmPos, realTimePcrResult: Rt.HaemophilusInfluenzae), 2, false, true, null, rtMolPf),
                Case(N + "10", NativeIsolate(NmNeg, NmNeg, NmNd, NmNeg, NmNeg, NmPos, NmNd, bestMatch: "Neisseria meningitidis"), 3, true, false, null, Attr.Rna16S, Attr.MolecularTyping, Attr.BestMatch, Attr.PorA, Attr.FetA),
                Case(N + "11", NativeIsolate(NmNeg, NmNeg, NmNd, NmNeg, NmNeg, NmPos, NmNd, bestMatch: "Neisseria lactamica"), 2, true, true, null, Attr.Rna16S, Attr.MolecularTyping, Attr.BestMatch, Attr.PorA, Attr.FetA),
                Case(N + "12", NativeIsolate(NmNd, NmNd, NmNd, NmNeg, NmNeg, NmNeg, NmNd), 2, false, true, null, Attr.Rna16S, Attr.PorA, Attr.FetA),
                Case(N + "13", NativeIsolate(NmNeg, NmNeg, NmNeg, NmNeg, NmNeg, NmNd, NmNeg), 2, false, true, null, rtMolPf),
                Case(N + "14", NativeIsolate(NmNeg, NmNeg, NmNeg, NmNeg, NmNeg, NmNd, NmPos, realTimePcrResult: Rt.NeisseriaMeningitidis), 3, false, false, null, rtMolPf),
                Case(N + "15", NativeIsolate(NmNeg, NmNeg, NmNeg, NmNeg, NmNeg, NmNd, NmPos, realTimePcrResult: Rt.StreptococcusPneumoniae), 2, false, true, null, rtMolPf),
                Case(N + "16", NativeIsolate(NmNeg, NmNeg, NmNeg, NmNeg, NmNeg, NmNd, NmPos, realTimePcrResult: Rt.HaemophilusInfluenzae), 2, false, true, null, rtMolPf),
                Case(N + "17", NativeIsolate(NmNeg, NmNeg, NmNeg, NmNeg, NmNeg, NmPos, NmNd, bestMatch: "Neisseria meningitidis"), 3, true, false, null, Attr.Rna16S, Attr.BestMatch, Attr.MolecularTyping, Attr.PorA, Attr.FetA),
                Case(N + "18", NativeIsolate(NmNeg, NmNeg, NmNeg, NmNeg, NmNeg, NmPos, NmNd, bestMatch: "Neisseria lactamica"), 2, true, true, null, Attr.Rna16S, Attr.BestMatch, Attr.MolecularTyping, Attr.PorA, Attr.FetA),
                Case(N + "19", NativeIsolate(NmNeg, NmNeg, NmNeg, NmNeg, NmNeg, NmNeg, NmNd), 2, false, true, null, Attr.Rna16S, Attr.MolecularTyping, Attr.PorA, Attr.FetA),
                Case(N + "20", NativeIsolate(NmNeg, NmNeg, NmNeg, NmNeg, NmNeg, NmNd, NmNd), 2, false, true, null, molPf),
                Case(N + "21", NativeIsolate(NmNeg, NmNeg, NmNeg, NmPos, NmPos, NmNd, NmNd), 3, true, false, null, molPf),
                Case(N + "22", NativeIsolate(NmPos, NmNd, NmNd, NmNeg, NmNeg, NmNd, NmPos, realTimePcrResult: Rt.NeisseriaMeningitidis), 3, false, false, "B", rtMolPf),
                Case(N + "23", NativeIsolate(NmNd, NmNd, NmNd, NmNeg, NmNeg, NmNd, NmNeg), 2, false, true, null, Attr.RealTimePcr, Attr.PorA, Attr.FetA),
                Case(N + "24", NativeIsolate(NmNeg, NmNeg, NmNd, NmNeg, NmNeg, NmNd, NmNd), 2, false, true, null, molPf),
                Case(N + "25", NativeIsolate(NmNd, NmPos, NmNd, NmNeg, NmNeg, NmNd, NmPos, realTimePcrResult: Rt.NeisseriaMeningitidis), 3, false, false, "C", rtMolPf),
                Case(N + "26", NativeIsolate(NmNd, NmNd, NmPos, NmNeg, NmNeg, NmNd, NmPos, Al.Allele1, Rt.NeisseriaMeningitidis), 3, false, false, "W", rtMolPf),
                Case(N + "27", NativeIsolate(NmNd, NmNd, NmPos, NmNeg, NmNeg, NmNd, NmPos, Al.Allele2, Rt.NeisseriaMeningitidis), 3, false, false, "Y", rtMolPf),
                Case(N + "28", NativeIsolate(NmNd, NmNd, NmPos, NmNeg, NmNeg, NmNd, NmPos, Al.Allele3, Rt.NeisseriaMeningitidis), 3, false, false, "W/Y", rtMolPf),
                Case(N + "29a", NativeIsolate(NmPos, NmNd, NmNd, NmNeg, NmPos, NmNd, NmPos, realTimePcrResult: Rt.NeisseriaMeningitidis), 3, true, false, "B", rtMolPf),
                Case(N + "29b", NativeIsolate(NmPos, NmNd, NmNd, NmNeg, NmPos, NmNd, NmNd), 3, true, false, "B", molPf),
                Case(N + "31a", NativeIsolate(NmPos, NmNd, NmNd, NmPos, NmNeg, NmNd, NmPos, realTimePcrResult: Rt.NeisseriaMeningitidis), 3, true, false, "B", rtMolPf),
                Case(N + "31b", NativeIsolate(NmPos, NmNd, NmNd, NmPos, NmNeg, NmNd, NmNd), 3, true, false, "B", molPf),
                Case(N + "32", NativeIsolate(NmNeg, NmNeg, NmNeg, NmNeg, NmNeg, NmNeg, NmNeg), 2, true, null, null, Attr.RealTimePcr, Attr.Rna16S, Attr.MolecularTyping, Attr.PorA, Attr.FetA),
            };

            return cases.Select(c => new TestCaseData(c).SetName($"Interpret_RepresentativeInput_SelectsDocumentedRule({c.Rule})"));
        }

        // specs/007 SC-001, FR-002, FR-006, FR-007, FR-009, FR-010, FR-015, FR-016: one representative input per reachable
        // rule selects that rule (first match: the inputs of native 10/17 also satisfy 11/18) and yields the documented
        // typing order, report line count, comment, flag and normalized serogroup.
        [TestCaseSource(nameof(RepresentativeRuleInputs))]
        public void Interpret_RepresentativeInput_SelectsDocumentedRule(RuleCase ruleCase)
        {
            var interpretation = new MeningoIsolateInterpretation();

            interpretation.Interpret(ruleCase.Isolate);

            interpretation.Rule.Should().Be(ruleCase.Rule);
            interpretation.Typings.Select(t => t.Attribute).Should().Equal(ruleCase.TypingAttributes);
            interpretation.Result.Report.Should().HaveCount(ruleCase.ReportLines);
            if (ruleCase.HasComment == true)
            {
                interpretation.Result.Comment.Should().Be(SequencingComment);
            }
            else if (ruleCase.HasComment == false)
            {
                interpretation.Result.Comment.Should().BeNull();
            }
            if (ruleCase.NoMeningococci.HasValue)
            {
                interpretation.NoMeningococci.Should().Be(ruleCase.NoMeningococci.Value);
            }
            interpretation.Serogroup.Should().Be(ruleCase.Serogroup);
        }

        // specs/007 FR-001, EC1: native material and isolated DNA use the native rules, vital strains and "not grown" the cultured rules
        [TestCase(MeningoMaterial.VitalStem, "StemInterpretation_02")]
        [TestCase(MeningoMaterial.NoGrowth, "StemInterpretation_02")]
        [TestCase(MeningoMaterial.NativeMaterial, "NativeMaterialInterpretation_01a")]
        [TestCase(MeningoMaterial.IsolatedDna, "NativeMaterialInterpretation_01a")]
        public void Interpret_Material_SelectsRuleFamily(MeningoMaterial material, string expectedRule)
        {
            // matches cultured rule 02 and native rule 01a at the same time
            var isolate = StemIsolate(true, Gr.No, Gr.No, Tr.NotDetermined, Ag.NotDetermined, Tr.NotDetermined, Tr.NotDetermined,
                Pc.C, Nm.Positive, Nm.Positive, material: material);
            isolate.CsbPcr = Nm.Positive;
            var interpretation = new MeningoIsolateInterpretation();

            interpretation.Interpret(isolate);

            interpretation.Rule.Should().Be(expectedRule);
        }

        // specs/007 FR-003: cultured rule 03 does not configure oxidase, ONPG and gamma-GT, so any value matches
        [Test]
        public void Interpret_CulturedRuleOmittingOxidaseOnpgAndGammaGt_AcceptsAnyValue([Values] Tr oxidase, [Values] Tr onpg, [Values] Tr gammaGt)
        {
            var isolate = StemIsolate(false, Gr.ATypicalGrowth, Gr.No, oxidase, Ag.NotDetermined, onpg, gammaGt, Pc.NotDetermined, Nm.NotDetermined, Nm.NotDetermined);
            var interpretation = new MeningoIsolateInterpretation();

            interpretation.Interpret(isolate);

            interpretation.Rule.Should().Be("StemInterpretation_03");
        }

        // specs/007 FR-003: cultured rule 08 does not configure oxidase
        [Test]
        public void Interpret_CulturedRuleOmittingOxidase_AcceptsAnyOxidase([Values] Tr oxidase)
        {
            var isolate = StemIsolate(true, Gr.TypicalGrowth, Gr.TypicalGrowth, oxidase, Ag.Auto, Tr.Negative, Tr.Positive, Pc.C, Nm.Positive, Nm.Positive);
            var interpretation = new MeningoIsolateInterpretation();

            interpretation.Interpret(isolate);

            interpretation.Rule.Should().Be("StemInterpretation_08");
        }

        // specs/007 FR-003, EC13: rules NoNM_01 and NoNM_02 omit invasive status and match both kinds of submission
        [Test]
        public void Interpret_CulturedRuleOmittingInvasiveStatus_MatchesInvasiveAndNonInvasive([Values] bool invasive, [Values] bool maldiTofDetermined)
        {
            var isolate = StemIsolate(invasive, Gr.ATypicalGrowth, Gr.No, Tr.NotDetermined, Ag.NotDetermined, Tr.Positive, Tr.Positive,
                Pc.NotDetermined, Nm.NotDetermined, Nm.NotDetermined, maldiTofDetermined ? Ut.Determined : Ut.NotDetermined);
            if (!maldiTofDetermined)
            {
                isolate.GrowthOnMartinLewisAgar = Gr.ATypicalGrowth; // rule 03 would take Martin-Lewis "Nein" without MALDI-TOF
            }
            var interpretation = new MeningoIsolateInterpretation();

            interpretation.Interpret(isolate);

            interpretation.Rule.Should().Be(maldiTofDetermined ? "StemInterpretation_NoNM_01" : "StemInterpretation_NoNM_02");
        }

        // specs/007 FR-004, EC2: a native rule that omits the cswy allele requires the unset allele
        [Test]
        public void Interpret_NativeRuleOmittingAllele_DoesNotMatchASetAllele([Values(Al.Allele1, Al.Allele2, Al.Allele3)] Al allele)
        {
            var isolate = NativeIsolate(Nm.Negative, Nm.Negative, Nm.NotDetermined, Nm.Negative, Nm.Negative, Nm.NotDetermined, Nm.Negative, allele);
            var interpretation = new MeningoIsolateInterpretation();

            interpretation.Interpret(isolate);

            interpretation.Rule.Should().BeNull("rule 06 would match with the unset allele");
            interpretation.Result.Report.Should().Equal(DiscrepancyLine);
        }

        // specs/007 FR-004, EC2: a native rule that omits the real-time organism result requires the unset result
        [Test]
        public void Interpret_NativeRuleOmittingOrganismResult_DoesNotMatchASetOrganism(
            [Values(Rt.NeisseriaMeningitidis, Rt.HaemophilusInfluenzae, Rt.StreptococcusPneumoniae)] Rt organism)
        {
            var isolate = NativeIsolate(Nm.Negative, Nm.Negative, Nm.NotDetermined, Nm.Negative, Nm.Negative, Nm.NotDetermined, Nm.Negative,
                realTimePcrResult: organism);
            var interpretation = new MeningoIsolateInterpretation();

            interpretation.Interpret(isolate);

            interpretation.Rule.Should().BeNull("rule 06 would match with the unset organism result");
            interpretation.Result.Report.Should().Equal(DiscrepancyLine);
        }

        // specs/007 FR-004, EC2: an omitted 16S best match does not restrict a native rule
        [Test]
        public void Interpret_NativeRuleOmittingBestMatch_AcceptsAnyBestMatch([Values(null, "", "SV-007 Organismus")] string bestMatch)
        {
            var isolate = NativeIsolate(Nm.Negative, Nm.Negative, Nm.Negative, Nm.Negative, Nm.Negative, Nm.Negative, Nm.NotDetermined, bestMatch: bestMatch);
            var interpretation = new MeningoIsolateInterpretation();

            interpretation.Interpret(isolate);

            interpretation.Rule.Should().Be("NativeMaterialInterpretation_19");
        }

        // specs/007 EC12: "not determined" requires both MALDI-TOF devices to be not determined
        [TestCase(Ut.Determined, Ut.NotDetermined)]
        [TestCase(Ut.NotDetermined, Ut.Determined)]
        [TestCase(Ut.Determined, Ut.Determined)]
        public void Interpret_MaldiTofNotDeterminedRule_RequiresBothDevicesNotDetermined(Ut vitek, Ut biotyper)
        {
            var isolate = StemIsolate(false, Gr.TypicalGrowth, Gr.TypicalGrowth, Tr.Positive, Ag.X, Tr.Negative, Tr.Positive,
                Pc.NotDetermined, Nm.NotDetermined, Nm.NotDetermined, vitek, biotyper);
            var interpretation = new MeningoIsolateInterpretation();

            interpretation.Interpret(isolate);

            interpretation.Rule.Should().BeNull("rule 06 requires MALDI-TOF not determined on both devices");
        }

        // specs/007 EC12, FR-008: "determined" accepts either device; the Biotyper row is used whenever Biotyper is determined
        [TestCase(Ut.Determined, Ut.NotDetermined, Attr.MaldiVitek, "SV-007 Vitek")]
        [TestCase(Ut.NotDetermined, Ut.Determined, Attr.MaldiBiotyper, "SV-007 Biotyper")]
        [TestCase(Ut.Determined, Ut.Determined, Attr.MaldiBiotyper, "SV-007 Biotyper")]
        public void Interpret_MaldiTofDeterminedRule_AcceptsEitherDeviceAndPrefersBiotyperRow(Ut vitek, Ut biotyper, string attribute, string value)
        {
            var isolate = StemIsolate(false, Gr.ATypicalGrowth, Gr.No, Tr.Negative, Ag.NotDetermined, Tr.Negative, Tr.Negative,
                Pc.NotDetermined, Nm.NotDetermined, Nm.NotDetermined, vitek, biotyper);
            var interpretation = new MeningoIsolateInterpretation();

            interpretation.Interpret(isolate);

            interpretation.Rule.Should().Be("StemInterpretation_04");
            interpretation.Typings.Last().Attribute.Should().Be(attribute);
            interpretation.Typings.Last().Value.Should().Be(value);
        }

        // specs/007 FR-005, FR-011, SC-002, US4.1: without a matching rule the defaults stay: discrepancy line only, no typings,
        // no rule, no serogroup, flag false, no comment. "no growth, negative molecular results": US1.3 covers only
        // not-determined molecular tests (rules 01/27); negative serogroup PCR with non-amplified PorA and FetA matches no rule
        [TestCase("cultured")]
        [TestCase("native")]
        [TestCase("no growth, negative molecular results")]
        public void Interpret_NoRuleMatches_KeepsDiscrepancyDefaults(string kind)
        {
            var isolate = kind == "native"
                ? NativeIsolate(Nm.NotDetermined, Nm.NotDetermined, Nm.NotDetermined, Nm.NotDetermined, Nm.NotDetermined, Nm.NotDetermined, Nm.NotDetermined)
                : kind == "cultured"
                    ? StemIsolate(true, Gr.TypicalGrowth, Gr.TypicalGrowth, Tr.NotDetermined, Ag.NotDetermined, Tr.NotDetermined, Tr.NotDetermined,
                        Pc.NotDetermined, Nm.NotDetermined, Nm.NotDetermined)
                    : StemIsolate(true, Gr.No, Gr.No, Tr.NotDetermined, Ag.NotDetermined, Tr.NotDetermined, Tr.NotDetermined,
                        Pc.Negative, Nm.Negative, Nm.Negative);
            var interpretation = new MeningoIsolateInterpretation();

            interpretation.Interpret(isolate);

            interpretation.Result.Report.Should().Equal(DiscrepancyLine);
            interpretation.Typings.Should().BeEmpty();
            interpretation.Rule.Should().BeNull();
            interpretation.Serogroup.Should().BeNull();
            interpretation.NoMeningococci.Should().BeFalse();
            interpretation.Result.Comment.Should().BeNull();
        }

        // specs/007 FR-005, FR-012, EC11: a reused instance clears typings, rule, serogroup and flag and resets the report,
        // but an unmatched run keeps the comment of the previous match
        [Test]
        public void Interpret_ReusedInstanceWithoutMatch_ClearsStateButKeepsPreviousComment()
        {
            var interpretation = new MeningoIsolateInterpretation();
            interpretation.Interpret(StemIsolate(true, Gr.No, Gr.No, Tr.NotDetermined, Ag.NotDetermined, Tr.NotDetermined, Tr.NotDetermined,
                Pc.C, Nm.Positive, Nm.Positive));
            interpretation.Rule.Should().Be("StemInterpretation_02");
            interpretation.Typings.Should().NotBeEmpty();

            interpretation.Interpret(StemIsolate(true, Gr.TypicalGrowth, Gr.TypicalGrowth, Tr.NotDetermined, Ag.NotDetermined, Tr.NotDetermined,
                Tr.NotDetermined, Pc.NotDetermined, Nm.NotDetermined, Nm.NotDetermined));

            interpretation.Result.Report.Should().Equal(DiscrepancyLine);
            interpretation.Typings.Should().BeEmpty();
            interpretation.Rule.Should().BeNull();
            interpretation.Serogroup.Should().BeNull();
            interpretation.NoMeningococci.Should().BeFalse();
            interpretation.Result.Comment.Should().Be(SequencingComment);
        }

        // specs/007 SC-001, FR-003, FR-004, EC2, EC14: the rulesets declare 44 cultured and 36 native rules; every cultured
        // rule configures blood-agar growth, every native rule the list-valued criteria; no cultured rule emits a plain
        // agglutination group row together with a serogenogroup row
        [Test]
        public void Rulesets_DeclareTheDocumentedRulesAndCriteria()
        {
            var stemRules = LoadRuleset("StemRules.json").Properties().ToList();
            var nativeRules = LoadRuleset("NativeMaterialRules.json").Properties().ToList();

            stemRules.Should().HaveCount(44);
            nativeRules.Should().HaveCount(36);
            stemRules.Should().OnlyContain(r => r.Value["GrowthOnBloodAgar"] != null);
            foreach (var criterion in new[] { "CsbPcr", "CscPcr", "CswyPcr", "PorAPcr", "FetAPcr", "RibosomalRna16S", "RealTimePcr" })
            {
                nativeRules.Should().OnlyContain(r => r.Value[criterion] != null && r.Value[criterion].Type == JTokenType.Array, criterion);
            }
            stemRules.Should().NotContain(r =>
                r.Value["Typings"].Values<string>().Contains("Agglutination")
                && r.Value["Typings"].Values<string>().Any(t => t.StartsWith("Serogenogroup")));
        }

        private static JObject LoadRuleset(string fileName)
        {
            using (var stream = typeof(MeningoIsolateInterpretation).Assembly.GetManifestResourceStream("HaemophilusWeb.Domain.Interpretation." + fileName))
            using (var reader = new StreamReader(stream))
            {
                return JObject.Parse(reader.ReadToEnd());
            }
        }

        private NativeMaterialTestResult GetRandomNegativeOrInhibitory()
        {
            return _random.Next(2) == 0 ? NativeMaterialTestResult.Negative : NativeMaterialTestResult.Inhibitory;
        }
        private NativeMaterialTestResult GetRandomPositiveOrNotDetermined()
        {
            return _random.Next(2) == 0 ? NativeMaterialTestResult.Positive : NativeMaterialTestResult.NotDetermined;
        }

        private NativeMaterialTestResult GetRandomNegativeOrNotDetermined()
        {
            switch (_random.Next(2))
            {
                case 0: return NativeMaterialTestResult.Negative;
                default: return NativeMaterialTestResult.NotDetermined;
            }
        }

        private NativeMaterialTestResult GetRandomNegativeInhibitoryOrNotDetermined()
        {
            switch (_random.Next(3))
            {
                case 0: return NativeMaterialTestResult.Negative;
                case 1: return NativeMaterialTestResult.Inhibitory;
                default: return NativeMaterialTestResult.NotDetermined;
            }
        }
        
        private void AssertNoMeningococciFlagIsValid(MeningoIsolateInterpretation isolateInterpretation)
        {
            var rule = isolateInterpretation.Rule;
            var expectedToReportNoMeningococci = RulesWithNoMeningococci.Contains(rule);
            isolateInterpretation.NoMeningococci.Should().Be(expectedToReportNoMeningococci);
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