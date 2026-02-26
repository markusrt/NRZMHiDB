using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentAssertions;
using HaemophilusWeb.CustomAssertions;
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
            isolateInterpretation.Result.Should().ContainReportLine("konnte nicht angezüchtet werden")
                .And.ContainReportLine("sollte unmittelbar vor dem Versand in Transportmedien");
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
            isolateInterpretation.Result.Should().ContainReportLine("konnte nicht angezüchtet werden");
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
           
            isolateInterpretation.Result.Should().ContainReportLine("konnte nicht angezüchtet werden");
            isolateInterpretation.Should()
                .HaveTyping("Identifikation", "Neisseria meningitidis")
                .And.HaveTyping("Serogenogruppe",
                serogroupPcr != MeningoSerogroupPcr.Negative ? "C" : "nicht gruppierbar")
                .And.HaveTyping("PorA - Sequenztyp",
                porAPcr == NativeMaterialTestResult.Positive ? "X, Y" : "das porA-Gen konnte nicht amplifiziert werden")
                .And.HaveTyping("FetA - Sequenztyp",
                fetAPcr == NativeMaterialTestResult.Positive ? "Z" : "das fetA-Gen konnte nicht amplifiziert werden");
            isolateInterpretation.Result.Comment.Should().NotBeEmpty();

            if (serogroupPcr == MeningoSerogroupPcr.Negative)
            {
                isolateInterpretation.Result.Should().ContainReportLine("Meldekategorie dieses Befundes: Neisseria meningitidis, NG (keine Serogruppe bestimmbar).");
                isolateInterpretation.Serogroup.Should().Be("NG");
            }
            else
            {
                isolateInterpretation.Result.Should().ContainReportLine("Meldekategorie dieses Befundes: Neisseria meningitidis, Serogruppe C.");
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

            isolateInterpretation.Result.Should().ContainReportLine("Kein Nachweis von Neisseria meningitidis.");
            isolateInterpretation.Typings.Should().NotContain(t => t.Attribute == "Identifikation");
            isolateInterpretation.Should()
                .HaveTyping("Wachstum auf Blutagar", "atypisches Wachstum")
                .And.HaveTyping("Wachstum auf Martin-Lewis-Agar", "Nein");
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

            isolateInterpretation.Result.Should().ContainReportLine("Kein Nachweis von Neisseria meningitidis.");
            isolateInterpretation.Typings.Should().NotContain(t => t.Attribute == "Identifikation");
            isolateInterpretation.Should()
                .HaveTyping("β-Galaktosidase", "negativ")
                .And.HaveTyping("γ-Glutamyltransferase", EnumUtils.GetEnumDescription(typeof(TestResult), gammaGtTestResult))
                .And.HaveTyping("MALDI-TOF (VITEK MS)", "N. gonorrhoeae");
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

            isolateInterpretation.Result.Should().ContainReportLine("Kein Nachweis von Neisseria meningitidis.");
            isolateInterpretation.Typings.Should().NotContain(t => t.Attribute == "Identifikation");
            isolateInterpretation.Should()
                .HaveTyping("β-Galaktosidase", "negativ")
                .And.HaveTyping("γ-Glutamyltransferase", EnumUtils.GetEnumDescription(typeof(TestResult), gammaGtTestResult))
                .And.HaveTyping("MALDI-TOF (Biotyper)", "N. gonorrhoeae");
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

            isolateInterpretation.Result.Should().ContainReportLine("Anmerkung: Eine Resistenztestung wurde aus epidemiologischen und  Kostengründen nicht durchgeführt.");

            isolateInterpretation.Should()
                .HaveTyping("Identifikation", "Neisseria meningitidis")
                .And.HaveTyping("Serogruppe", "X");
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

            isolateInterpretation.Result.Should().ContainReportLine("Meldekategorie dieses Befundes: Neisseria meningitidis, Serogruppe E.");
            isolateInterpretation.Should()
                .HaveTyping("Identifikation", "Neisseria meningitidis")
                .And.HaveTyping("Serogruppe", "E");
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

            isolateInterpretation.Result.Should().ContainReportLine("Meldekategorie dieses Befundes: Neisseria meningitidis, Serogruppe W/Y.");
            isolateInterpretation.Should().HaveTyping("Identifikation", "Neisseria meningitidis");
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "Serogruppe" && t.Value.Contains(agglutination.ToString()) && !t.Value.Contains("Nicht-invasiv"));
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "Serogenogruppe" && t.Value.Contains("W/Y") && t.Value.Contains("molekularbiologisch bestimmt"));
            isolateInterpretation.Should()
                .HaveTyping("PorA - Sequenztyp", "X, Y")
                .And.HaveTyping("FetA - Sequenztyp", "Z");
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

            isolateInterpretation.Result.Should().ContainReportLine("Anmerkung: Eine Resistenztestung wurde aus epidemiologischen und  Kostengründen nicht durchgeführt.");

            isolateInterpretation.Should().HaveTyping("Identifikation", "Neisseria meningitidis");
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "Serogruppe" && t.Value.Contains(agglutination.ToString()) && t.Value.Contains("Nicht-invasiv"));
            isolateInterpretation.Typings.Should().Contain(t =>
                t.Attribute == "Serogenogruppe" && t.Value.Contains("W/Y") && t.Value.Contains("molekularbiologisch"));
            isolateInterpretation.Should()
                .HaveTyping("PorA - Sequenztyp", "X, Y")
                .And.HaveTyping("FetA - Sequenztyp", "Z");
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
            isolateInterpretation.Should().HaveTyping("Identifikation", "Neisseria meningitidis");
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

            isolateInterpretation.Result.Should().ContainReportLine("Meldekategorie dieses Befundes: Neisseria meningitidis, Serogruppe W/Y.");
            isolateInterpretation.Should()
                .HaveTyping("Identifikation", "Neisseria meningitidis")
                .And.HaveTyping("Serogruppe", "W/Y")
                .And.HaveTyping("PorA - Sequenztyp", "X, Y")
                .And.HaveTyping("FetA - Sequenztyp", "Z");
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
                interpretation.Result.Should().ContainReportLine("Meldekategorie dieses Befundes: Neisseria meningitidis, cnl (capsule null locus).");
            }
            else
            {
                interpretation.Result.Should().ContainReportLine("Anmerkung: Eine Resistenztestung wurde aus epidemiologischen und  Kostengründen nicht durchgeführt.");
            }

            interpretation.Should()
                .HaveTyping("Identifikation", "Neisseria meningitidis")
                .And.HaveTyping("Agglutination", "keine Agglutination mit Antikörpern gegen die Serogruppen A, B, C, E, W, X und Y");

            if (poraAndFeta != NativeMaterialTestResult.NotDetermined)
            {
                interpretation.Should()
                    .HaveTyping("PorA - Sequenztyp", "X, Y")
                    .And.HaveTyping("FetA - Sequenztyp", "Z");
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
                interpretation.Result.Should().ContainReportLine("Meldekategorie dieses Befundes: Neisseria meningitidis, cnl (capsule null locus).");
            }
            else
            {
                interpretation.Result.Should().ContainReportLine("Anmerkung: Eine Resistenztestung wurde aus epidemiologischen und  Kostengründen nicht durchgeführt.");
            }

            interpretation.Should()
                .HaveTyping("Identifikation", "Neisseria meningitidis")
                .And.HaveTyping("Agglutination", "Poly-Agglutination mit Antikörpern gegen die Serogruppen B, C, W und Y");

            if (poraAndFeta != NativeMaterialTestResult.NotDetermined)
            {
                interpretation.Should()
                    .HaveTyping("PorA - Sequenztyp", "X, Y")
                    .And.HaveTyping("FetA - Sequenztyp", "Z");
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
                interpretation.Result.Should().ContainReportLine("Meldekategorie dieses Befundes: Neisseria meningitidis, cnl (capsule null locus).");
            }
            else
            {
                interpretation.Result.Report.Should().BeEmpty();
            }

            interpretation.Should()
                .HaveTyping("Identifikation", "Neisseria meningitidis")
                .And.HaveTyping("Agglutination", "Auto-Agglutination");

            if (poraAndFeta != NativeMaterialTestResult.NotDetermined)
            {
                interpretation.Should()
                    .HaveTyping("PorA - Sequenztyp", "X, Y")
                    .And.HaveTyping("FetA - Sequenztyp", "Z");
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

            interpretation.Result.Should().ContainReportLine("Anmerkung: Eine Resistenztestung wurde aus epidemiologischen und  Kostengründen nicht durchgeführt.");
            interpretation.Should()
                .HaveTyping("Identifikation", "Neisseria meningitidis")
                .And.HaveTyping("Agglutination", "keine Agglutination mit Antikörpern gegen die Serogruppen A, B, C, E, W, X und Y (Nicht-invasive Meningokokken sind oft unbekapselt.)");
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

            isolateInterpretation.Result.Should().ContainReportLine("Kein Nachweis von Neisseria meningitidis.");
            isolateInterpretation.Typings.Should().NotContain(t => t.Attribute == "Identifikation");
            isolateInterpretation.Should()
                .HaveTyping("β-Galaktosidase", "negativ")
                .And.HaveTyping("γ-Glutamyltransferase", "negativ")
                .And.HaveTyping("MALDI-TOF (VITEK MS)", "N. gonorrhoeae");
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

            isolateInterpretation.Result.Should().ContainReportLine("Kein Nachweis von Neisseria meningitidis.");
            isolateInterpretation.Typings.Should().NotContain(t => t.Attribute == "Identifikation");
            isolateInterpretation.Should()
                .HaveTyping("β-Galaktosidase", "negativ")
                .And.HaveTyping("γ-Glutamyltransferase", "negativ")
                .And.HaveTyping("MALDI-TOF (Biotyper)", "N. gonorrhoeae");
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
           
            interpretation.Result.Should().ContainReportLine("meldepflichtig")
                .And.ContainReportLine("Meldekategorie dieses Befundes: Neisseria meningitidis, Serogruppe W/Y")
                .And.ContainReportLine("konnte nicht angezüchtet werden");
            interpretation.Should()
                .HaveTyping("Serogenogruppe", "W/Y")
                .And.HaveTypingContaining("PorA - Sequenztyp", "nicht amplifiziert")
                .And.HaveTypingContaining("FetA - Sequenztyp", "nicht amplifiziert");
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
            interpretation.Result.Should().ContainReportLine("Anmerkung: Eine Resistenztestung wurde aus epidemiologischen und  Kostengründen nicht durchgeführt.");

            interpretation.Should()
                .HaveTyping("Identifikation", "Neisseria meningitidis")
                .And.HaveTyping("PorA - Sequenztyp", "X, Y")
                .And.HaveTyping("FetA - Sequenztyp", "Z")
                .And.HaveTypingContaining("Serogruppe", agglutinationText);
            interpretation.Result.Comment.Should().Contain("Microsynth Seqlab");
            
            if (agglutination == MeningoSerogroupAgg.Poly || agglutination == MeningoSerogroupAgg.Auto)
            {
                interpretation.Should().HaveTypingContaining("Serogruppe", "Nicht-invasiv");
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

            interpretation.Result.Should().ContainReportLine("Diskrepante Ergebnisse");
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

            interpretation.Result.Should().ContainReportLine("Anmerkung: Eine Resistenztestung wurde aus epidemiologischen und  Kostengründen nicht durchgeführt.");

            interpretation.Should()
                .HaveTyping("Identifikation", "Neisseria meningitidis")
                .And.HaveTypingContaining("Serogenogruppe", expectedSerogroup)
                .And.HaveTypingContaining("Serogenogruppe", "molekularbiologisch bestimmt");
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

            interpretation.Result.Should().ContainReportLine("Anmerkung: Eine Resistenztestung wurde aus epidemiologischen und  Kostengründen nicht durchgeführt.");

            interpretation.Should()
                .HaveTyping("Identifikation", "Neisseria meningitidis")
                .And.HaveTyping("Agglutination", "keine Agglutination mit Antikörpern gegen die Serogruppen  B, C, W und Y")
                .And.HaveTyping("PorA - Sequenztyp", "X, Y")
                .And.HaveTyping("FetA - Sequenztyp", "Z");
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

            interpretation.Result.Should().ContainReportLine("Anmerkung: Aus Kostengründen werden bei nicht-invasiven Isolaten keine Serogruppenbestimmungen und Resistenztestungen durchgeführt.");

            interpretation.Should().HaveTyping("Identifikation", "Neisseria meningitidis");

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

            interpretation.Result.Should().ContainReportLine("Kein Nachweis von Neisseria meningitidis.");
            interpretation.Typings.Should().NotContain(t => t.Attribute == "Identifikation");
            interpretation.Should()
                .HaveTyping("β-Galaktosidase", "negativ")
                .And.HaveTyping("γ-Glutamyltransferase", "n.d.")
                .And.HaveTyping("MALDI-TOF (VITEK MS)", "N. gonorrhoeae");
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

            interpretation.Result.Should().ContainReportLine("Kein Nachweis von Neisseria meningitidis.");
            interpretation.Typings.Should().NotContain(t => t.Attribute == "Identifikation");
            interpretation.Should()
                .HaveTyping("β-Galaktosidase", "negativ")
                .And.HaveTyping("γ-Glutamyltransferase", "n.d.")
                .And.HaveTyping("MALDI-TOF (Biotyper)", "N. gonorrhoeae");
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

            interpretation.Result.Should().ContainReportLine("Kein Nachweis von Neisseria meningitidis.");
            interpretation.Typings.Should().NotContain(t => t.Attribute == "Identifikation");
            interpretation.Typings.Should().NotContain(t => t.Attribute == "MALDI-TOF (VITEK MS)");
            interpretation.Should()
                .HaveTyping("β-Galaktosidase", "n.d.")
                .And.HaveTyping("γ-Glutamyltransferase", "negativ");
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

            interpretation.Result.Should().ContainReportLine("meldepflichtig")
                .And.ContainReportLine("Meldekategorie dieses Befundes: Neisseria meningitidis, Serogruppe B");

            interpretation.Should()
                .HaveTyping("Identifikation", "Neisseria meningitidis")
                .And.HaveTyping("Serogruppe", "B")
                .And.HaveTyping("Empfindlichkeitstestung (Etest)", "folgt");
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

            interpretation.Result.Should().ContainReportLine($"Meningokokken-spezifische DNA konnte nachgewiesen werden. Die Ergebnisse sprechen für eine invasive Infektion mit Meningokokken der Serogruppe {serogroup}.")
                .And.ContainReportLine("meldepflichtig")
                .And.ContainReportLine($"Serogruppe {serogroup}");
            interpretation.Should()
                .HaveTyping("Molekulare Typisierung", $"Das Serogruppe-{serogroup}-spezifische cs{serogroup.ToLower()}-Gen wurde nachgewiesen.")
                .And.HaveTyping("PorA - Sequenztyp", "X, Y")
                .And.HaveTyping("FetA - Sequenztyp", "Z");
            interpretation.Result.Comment.Should().Contain("Microsynth Seqlab");
            interpretation.Serogroup.Should().Be(serogroup);

            if (realTimePcr == NativeMaterialTestResult.Positive)
            {
                interpretation.Should().HaveTyping("NHS Real-Time-PCR (Sacace)", "Positiv für bekapselte Neisseria meningitidis. Der molekularbiologische Nachweis von Neisseria meningitidis beruht auf dem Nachweis des Kapseltransportgens ctrA mittels spezifischer Real-Time-PCR.");
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

            interpretation.Result.Should().ContainReportLine($"Meningokokken-spezifische DNA konnte nachgewiesen werden. Die Ergebnisse sprechen für eine invasive Infektion mit Meningokokken der Serogruppe {serogroup}.")
                .And.ContainReportLine("meldepflichtig")
                .And.ContainReportLine($"Serogruppe {serogroup}");
            interpretation.Should()
                .HaveTyping("Molekulare Typisierung", $"Das Serogruppe-{serogroup}-spezifische cs{serogroup.ToLower()}-Gen wurde nachgewiesen.")
                .And.HaveTyping("PorA - Sequenztyp", "X, Y")
                .And.HaveTyping("FetA - Sequenztyp", "Z");
            interpretation.Result.Comment.Should().Contain("Microsynth Seqlab");
            interpretation.Serogroup.Should().Be(serogroup);

            if (realTimePcr == NativeMaterialTestResult.Positive)
            {
                interpretation.Should().HaveTyping("NSH Real-Time-PCR (BD-MAX)", "Positiv für bekapselte Neisseria meningitidis. Der molekularbiologische Nachweis von Neisseria meningitidis beruht auf dem Nachweis des Kapseltransportgens ctrA mittels spezifischer Real-Time-PCR.");
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

            interpretation.Result.Should().ContainReportLine($"Meningokokken-spezifische DNA konnte nachgewiesen werden. Die Ergebnisse sprechen für eine invasive Infektion mit Meningokokken der Serogruppe {serogroup}.")
                .And.ContainReportLine("meldepflichtig")
                .And.ContainReportLine($"Serogruppe {serogroup}");
            interpretation.Should()
                .HaveTyping("Molekulare Typisierung", $"Das Serogruppe{plural}-{serogroup}-spezifische {gen}-Gen wurde nachgewiesen.")
                .And.HaveTyping("PorA - Sequenztyp", "X, Y")
                .And.HaveTyping("FetA - Sequenztyp", "Z");
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

            interpretation.Result.Should().ContainReportLine(
                realTimePcrResult == RealTimePcrResult.NeisseriaMeningitidis
                    ? "Meningokokken-spezifische DNA konnte nachgewiesen werden."
                    : "Kein Hinweis auf Neisseria meningitidis");

            if (realTimePcrResult == RealTimePcrResult.NeisseriaMeningitidis)
            {
                interpretation.Result.Should().ContainReportLine("meldepflichtig");
            }
            else
            {
                interpretation.Result.Should().NotContainReportLine("meldepflichtig");
            }

            interpretation.Should()
                .HaveTyping("Molekulare Typisierung", 
                    cswyNegative
                        ? "Die Serogruppen B, C,  W und Y-spezifischen csb-, csc-, csw- und csy-Gene wurden nicht nachgewiesen."
                        : "Die Serogruppen B und C-spezifischen csb- und csc-Gene wurden nicht nachgewiesen.")
                .And.HaveTyping("NHS Real-Time-PCR (Sacace)", realTimePcrInterpretation)
                .And.HaveTypingContaining("PorA - Sequenztyp", "nicht amplifiziert")
                .And.HaveTypingContaining("FetA - Sequenztyp", "nicht amplifiziert");
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

            interpretation.Result.Should().ContainReportLine("Diskrepante Ergebnisse, bitte Datenbankeinträge kontrollieren.");
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

            interpretation.Result.Should().ContainReportLine(expectedInterpretation);

            if ("Neisseria meningitidis".Equals(ribosomalRna16SBestMatch))
            {
                interpretation.Result.Should().ContainReportLine("meldepflichtig");
            }
            else
            {
                interpretation.Result.Should().NotContainReportLine("meldepflichtig");
            }

            interpretation.Should()
                .HaveTyping("Molekulare Typisierung", 
                    cswyNegative
                        ? "Die Serogruppen B, C,  W und Y-spezifischen csb-, csc-, csw- und csy-Gene wurden nicht nachgewiesen."
                        : "Die Serogruppen B und C-spezifischen csb- und csc-Gene wurden nicht nachgewiesen.")
                .And.HaveTyping("16S-rDNA-Nachweis", "positiv")
                .And.HaveTyping("Ergebnis der DNA-Sequenzierung", ribosomalRna16SBestMatch)
                .And.HaveTypingContaining("PorA - Sequenztyp", "nicht amplifiziert")
                .And.HaveTypingContaining("FetA - Sequenztyp", "nicht amplifiziert");
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

            interpretation.Result.Should().ContainReportLine("Meningokokken- spezifische DNA konnte nicht nachgewiesen werden.")
                .And.ContainReportLine("Kein Hinweis auf Neisseria meningitidis.")
                .And.NotContainReportLine("meldepflichtig");

            interpretation.Typings.Should().NotContain(t => t.Attribute == "Molekulare Typisierung");

            interpretation.Should()
                .HaveTypingMatching("16S-rDNA-Nachweis", s => new List<string> {"negativ", "inhibitorisch" }.Contains(s))
                .And.HaveTypingContaining("PorA - Sequenztyp", "nicht amplifiziert")
                .And.HaveTypingContaining("FetA - Sequenztyp", "nicht amplifiziert");
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

            interpretation.Result.Should().ContainReportLine("Meningokokken- spezifische DNA konnte nicht nachgewiesen werden.")
                .And.ContainReportLine("Kein Hinweis auf Neisseria meningitidis.")
                .And.NotContainReportLine("meldepflichtig");

            interpretation.Should()
                .HaveTyping("Molekulare Typisierung", "Die Serogruppen B, C,  W und Y-spezifischen csb-, csc-, csw- und csy-Gene wurden nicht nachgewiesen.")
                .And.HaveTypingMatching("16S-rDNA-Nachweis", s => new List<string> { "negativ", "inhibitorisch" }.Contains(s))
                .And.HaveTypingContaining("PorA - Sequenztyp", "nicht amplifiziert")
                .And.HaveTypingContaining("FetA - Sequenztyp", "nicht amplifiziert");
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

            interpretation.Result.Should().ContainReportLine("Meningokokken- spezifische DNA konnte nicht nachgewiesen werden.")
                .And.ContainReportLine("Kein Hinweis auf Neisseria meningitidis.")
                .And.NotContainReportLine("meldepflichtig");

            interpretation.Should()
                .HaveTyping("Molekulare Typisierung", "Die Serogruppen B, C,  W und Y-spezifischen csb-, csc-, csw- und csy-Gene wurden nicht nachgewiesen.")
                .And.HaveTypingContaining("PorA - Sequenztyp", "nicht amplifiziert")
                .And.HaveTypingContaining("FetA - Sequenztyp", "nicht amplifiziert");
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

            
            interpretation.Result.Should().ContainReportLine("Meningokokken-spezifische DNA konnte nachgewiesen werden.")
                .And.ContainReportLine("meldepflichtig")
                .And.ContainReportLine("Meldekategorie dieses Befundes: Neisseria meningitidis.");
            interpretation.Should()
                .HaveTyping("Molekulare Typisierung", "Die Serogruppen B, C,  W und Y-spezifischen csb-, csc-, csw- und csy-Gene wurden nicht nachgewiesen.")
                .And.HaveTyping("PorA - Sequenztyp", "X, Y")
                .And.HaveTyping("FetA - Sequenztyp", "Z");
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

            interpretation.Result.Should().ContainReportLine("Meningokokken-spezifische DNA konnte nachgewiesen werden.")
                .And.ContainReportLine("meldepflichtig")
                .And.ContainReportLine("Meldekategorie dieses Befundes: Neisseria meningitidis, Serogruppe B");
            interpretation.Should()
                .HaveTyping("Molekulare Typisierung", "Das Serogruppe-B-spezifische csb-Gen wurde nachgewiesen.")
                .And.HaveTypingContaining("PorA - Sequenztyp", "nicht amplifiziert")
                .And.HaveTypingContaining("FetA - Sequenztyp", "nicht amplifiziert")
                .And.HaveTyping("NHS Real-Time-PCR (Sacace)", "Positiv für bekapselte Neisseria meningitidis. Der molekularbiologische Nachweis von Neisseria meningitidis beruht auf dem Nachweis des Kapseltransportgens ctrA mittels spezifischer Real-Time-PCR.");
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

            interpretation.Result.Should().ContainReportLine("Meningokokken- spezifische DNA konnte nicht nachgewiesen werden.")
                .And.ContainReportLine("Kein Hinweis auf Neisseria meningitidis.")
                .And.NotContainReportLine("meldepflichtig");

            interpretation.Typings.Should().NotContain(t => t.Attribute == "Molekulare Typisierung");
            interpretation.Should()
                .HaveTypingContaining("PorA - Sequenztyp", "nicht amplifiziert")
                .And.HaveTypingContaining("FetA - Sequenztyp", "nicht amplifiziert")
                .And.HaveTyping("NHS Real-Time-PCR (Sacace)", "Negativ für bekapselte Neisseria meningitidis, bekapselte Haemophilus influenzae und Streptococcus pneumoniae.");
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

            interpretation.Result.Should().ContainReportLine("Meningokokken- spezifische DNA konnte nicht nachgewiesen werden.")
                .And.ContainReportLine("Kein Hinweis auf Neisseria meningitidis.")
                .And.NotContainReportLine("meldepflichtig");

            interpretation.Should().HaveTyping("Molekulare Typisierung", "Die Serogruppen B und C-spezifischen csb- und csc-Gene wurden nicht nachgewiesen.");
            interpretation.Typings.Should().NotContain(t => t.Attribute == "NHS Real-Time-PCR (Sacace)");
            interpretation.Should()
                .HaveTypingContaining("PorA - Sequenztyp", "nicht amplifiziert")
                .And.HaveTypingContaining("FetA - Sequenztyp", "nicht amplifiziert");
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

            interpretation.Result.Should().ContainReportLine("Meningokokken-spezifische DNA konnte nachgewiesen werden.")
                .And.ContainReportLine("meldepflichtig")
                .And.ContainReportLine("Meldekategorie dieses Befundes: Neisseria meningitidis, Serogruppe C");
            interpretation.Should()
                .HaveTyping("Molekulare Typisierung", "Das Serogruppe-C-spezifische csc-Gen wurde nachgewiesen.")
                .And.HaveTypingContaining("PorA - Sequenztyp", "nicht amplifiziert")
                .And.HaveTypingContaining("FetA - Sequenztyp", "nicht amplifiziert")
                .And.HaveTyping("NHS Real-Time-PCR (Sacace)", "Positiv für bekapselte Neisseria meningitidis. Der molekularbiologische Nachweis von Neisseria meningitidis beruht auf dem Nachweis des Kapseltransportgens ctrA mittels spezifischer Real-Time-PCR.");
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

            interpretation.Result.Should().ContainReportLine("Meningokokken-spezifische DNA konnte nachgewiesen werden.")
                .And.ContainReportLine("meldepflichtig")
                .And.ContainReportLine("Meldekategorie dieses Befundes: Neisseria meningitidis, Serogruppe W");
            interpretation.Should()
                .HaveTyping("Molekulare Typisierung", "Das Serogruppe-W-spezifische csw-Gen wurde nachgewiesen.")
                .And.HaveTypingContaining("PorA - Sequenztyp", "nicht amplifiziert")
                .And.HaveTypingContaining("FetA - Sequenztyp", "nicht amplifiziert")
                .And.HaveTyping("NHS Real-Time-PCR (Sacace)", "Positiv für bekapselte Neisseria meningitidis. Der molekularbiologische Nachweis von Neisseria meningitidis beruht auf dem Nachweis des Kapseltransportgens ctrA mittels spezifischer Real-Time-PCR.");
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

            interpretation.Result.Should().ContainReportLine("Meningokokken-spezifische DNA konnte nachgewiesen werden.")
                .And.ContainReportLine("meldepflichtig")
                .And.ContainReportLine("Meldekategorie dieses Befundes: Neisseria meningitidis, Serogruppe Y");
            interpretation.Should()
                .HaveTyping("Molekulare Typisierung", "Das Serogruppe-Y-spezifische csy-Gen wurde nachgewiesen.")
                .And.HaveTypingContaining("PorA - Sequenztyp", "nicht amplifiziert")
                .And.HaveTypingContaining("FetA - Sequenztyp", "nicht amplifiziert")
                .And.HaveTyping("NHS Real-Time-PCR (Sacace)", "Positiv für bekapselte Neisseria meningitidis. Der molekularbiologische Nachweis von Neisseria meningitidis beruht auf dem Nachweis des Kapseltransportgens ctrA mittels spezifischer Real-Time-PCR.");
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

            interpretation.Result.Should().ContainReportLine("Meningokokken-spezifische DNA konnte nachgewiesen werden.")
                .And.ContainReportLine("meldepflichtig")
                .And.ContainReportLine("Meldekategorie dieses Befundes: Neisseria meningitidis, Serogruppe W/Y");
            interpretation.Should()
                .HaveTyping("Molekulare Typisierung", "Das Serogruppen-W/Y-spezifische csw/csy-Gen wurde nachgewiesen.")
                .And.HaveTypingContaining("PorA - Sequenztyp", "nicht amplifiziert")
                .And.HaveTypingContaining("FetA - Sequenztyp", "nicht amplifiziert")
                .And.HaveTyping("NHS Real-Time-PCR (Sacace)", "Positiv für bekapselte Neisseria meningitidis. Der molekularbiologische Nachweis von Neisseria meningitidis beruht auf dem Nachweis des Kapseltransportgens ctrA mittels spezifischer Real-Time-PCR.");
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

            interpretation.Result.Should().ContainReportLine("Meningokokken-spezifische DNA konnte nachgewiesen werden.")
                .And.ContainReportLine("meldepflichtig")
                .And.ContainReportLine("Meldekategorie dieses Befundes: Neisseria meningitidis, Serogruppe B");
            interpretation.Should().HaveTyping("Molekulare Typisierung", "Das Serogruppe-B-spezifische csb-Gen wurde nachgewiesen.");

            if (porA == NativeMaterialTestResult.Negative)
            {
                interpretation.Should().HaveTypingContaining("PorA - Sequenztyp", "nicht amplifiziert");
            }
            else
            {
                interpretation.Should().HaveTyping("PorA - Sequenztyp", "X, Y");
                interpretation.Result.Comment.Should().Contain("Microsynth Seqlab");
            }

            if (fetA == NativeMaterialTestResult.Negative)
            {
                interpretation.Should().HaveTypingContaining("FetA - Sequenztyp", "nicht amplifiziert");
            }
            else
            {
                interpretation.Should().HaveTyping("FetA - Sequenztyp", "Z");
                interpretation.Result.Comment.Should().Contain("Microsynth Seqlab");
            }

            if (porA == NativeMaterialTestResult.Negative && fetA == NativeMaterialTestResult.Negative)
            {
                interpretation.Result.Comment.Should().BeNull();
            }

            if (realTimePcr == NativeMaterialTestResult.Positive)
            {
                interpretation.Should().HaveTyping("NHS Real-Time-PCR (Sacace)", "Positiv für bekapselte Neisseria meningitidis. Der molekularbiologische Nachweis von Neisseria meningitidis beruht auf dem Nachweis des Kapseltransportgens ctrA mittels spezifischer Real-Time-PCR.");
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

            interpretation.Should()
                .HaveTyping("Molekulare Typisierung", "Die Serogruppen B, C, W und Y-spezifischen csb-, csc-, csw- und csy-Gene wurden nicht nachgewiesen.")
                .And.HaveTypingContaining("PorA - Sequenztyp", "nicht amplifiziert")
                .And.HaveTypingContaining("FetA - Sequenztyp", "nicht amplifiziert");
            interpretation.Result.Should().ContainReportLine("Kein Hinweis auf Neisseria meningitidis.");

            AssertNoMeningococciFlagIsValid(interpretation);
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
}
