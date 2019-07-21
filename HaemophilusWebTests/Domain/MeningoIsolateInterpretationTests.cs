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

        [Test]
        public void EmptyIsolate_ReturnsEmptyInterpretation()
        {
            var isolateInterpretation = new MeningoIsolateInterpretation();
            var isolate = new MeningoIsolate();

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
                Oxidase = TestResult.Positive,
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
    }
}