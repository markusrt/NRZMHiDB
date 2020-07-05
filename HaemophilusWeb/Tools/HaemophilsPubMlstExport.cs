using System;
using System.Collections.Generic;
using HaemophilusWeb.Domain;
using HaemophilusWeb.Models;
using HaemophilusWeb.Utils;

namespace HaemophilusWeb.Tools
{
    public class HaemophilusPubMlstExport : SendingExportDefinition<Sending, Patient>
    {
        public HaemophilusPubMlstExport()
        {

            AddField(s => s.Isolate.StemNumberWithPrefix, "isolate");
            AddNullColumn("aliases");
            AddNullColumn("references");
            AddField(s => "Germany", "country");
            AddField(s => ExportState(s.Patient.State), "region");

            AddField(s => s.SamplingDate.HasValue ? (int?) s.SamplingDate.Value.Year : null, "year");
            AddField(s => s.ReceivingDate.ToReportFormatPubMlst(), "date_sampled");
            AddField(s => s.SamplingDate.ToReportFormatPubMlst(), "date_received");

            AddNullColumn("non_culture");
            AddField(s => ExportEvaluation(s.Isolate.Evaluation), "serotype");
            AddField(s => ExportAgglutination(s.Isolate.Agglutination), "serotype_by_serology");
            AddField(s => ExportSerotypePcr(s.Isolate.SerotypePcr), "serotype_by_PCR");
            AddNullColumn("genotype");
            AddNullColumn("biotype");
            AddNullColumn("ribotype");

            AddField(s => s.Isolate.PatientAgeAtSampling(), "age_yr");
            AddField(s => GetMonthAge(s), "age_mth");

            AddField(s => ExportSex(s.Patient.Gender), "sex");
            AddField(s => ExportSamplingLocation(s.SamplingLocation, s), "source");

            AddNullColumn("disease");
            AddNullColumn("epidemiology");
            AddNullColumn("MLEE_lineage");
            AddNullColumn("MLEE_ET");

            AddField(s => ExportBetaLactamase(s.Isolate.BetaLactamase, s), "beta_lactamase");

            AddNullColumn("AMX_MIC");
            AddField(s => ExportEpsilometerTestResult(FindEpsilometerTestEvaluation(s, Antibiotic.Ampicillin)), "AMX_SIR");
            AddField(s => FindEpsilometerTestMeasurement(s, Antibiotic.AmoxicillinClavulanate), "AMC_MIC");
            AddField(s => ExportEpsilometerTestResult(FindEpsilometerTestEvaluation(s, Antibiotic.AmoxicillinClavulanate)), "AMC_SIR");
            AddField(s => FindEpsilometerTestMeasurement(s, Antibiotic.Cefotaxime), "CTX_MIC");
            AddField(s => ExportEpsilometerTestResult(FindEpsilometerTestEvaluation(s, Antibiotic.Cefotaxime)), "CTX_SIR");

            AddNullColumn("CRO_MIC");
            AddNullColumn("CRO_SIR");
            AddNullColumn("ENA_accession");
            AddNullColumn("comments");
            AddNullColumn("adk");
            AddNullColumn("atpG");
            AddNullColumn("frdB");
            AddNullColumn("fucK");
            AddNullColumn("mdh");
            AddNullColumn("pgi");
            AddNullColumn("recA");

            AddField(s => s.Isolate.FtsiEvaluation3, "ftsI");
        }

        private string ExportState(State state)
        {
            return state switch
            {
                State.SH => "Schleswig-Holstein",
                State.HH => "Hamburg",
                State.NI => "Lower Saxony",
                State.HB => "Bremen",
                State.NW => "North Rhine-Westphalia",
                State.HE => "Hesse",
                State.RP => "Rhineland-Palatinate",
                State.BW => "Baden-Württemberg",
                State.BY => "Bavaria",
                State.SL => "Saarland",
                State.BE => "Berlin",
                State.BB => "Brandenburg",
                State.MV => "Mecklenburg-Vorpommern",
                State.SN => "Saxony",
                State.ST => "Saxony-Anhalt",
                State.TH => "Thuringia",
                State.Overseas => "blood",
                _ => "n.a."
            };
        }

        private string ExportEpsilometerTestResult(EpsilometerTestResult? testResult)
        {
            return testResult.HasValue
                ? Enum.GetName(typeof(EpsilometerTestResult), testResult.Value)?.Substring(0, 1)
                : null;
        }


        private static Dictionary<Evaluation, string> _evaluationToString = new Dictionary<Evaluation, string>()
        {
            {Evaluation.HaemophilusNonEncapsulated, "NT"},
            {Evaluation.HaemophilusTypeA, "a"},
            {Evaluation.HaemophilusTypeB, "b"},
            {Evaluation.HaemophilusTypeC, "c"},
            {Evaluation.HaemophilusTypeD, "d"},
            {Evaluation.HaemophilusTypeE, "e"},
            {Evaluation.HaemophilusTypeF, "f"},
            {Evaluation.HaemophilusInfluenzae, "ND"},
            // these are actually not to be exported but in case it happens
            // the evaluation should at least be visible
            {Evaluation.HaemophilusHemolyticus, "Haemophilus Hemolyticus"},
            {Evaluation.HaemophilusParainfluenzae, "Haemophilus Parainfluenzae"},
            {Evaluation.NoGrowth, "No Growth"},
            {Evaluation.NoHaemophilusSpecies, "No Haemophilus Species"},
            {Evaluation.HaemophilusSpeciesNoHaemophilusInfluenzae, "Haemophilus Species but no Haemophilus Influenzae"},
            {Evaluation.NoHaemophilusInfluenzae, "No Haemophilus Influenzae"}
        };

        private string ExportEvaluation(Evaluation evaluation)
        {
            return _evaluationToString[evaluation];
        }

        private static Dictionary<SerotypeAgg, string> _agglutinationToString = new Dictionary<SerotypeAgg, string>()
        {
            {SerotypeAgg.NotDetermined, ""},
            {SerotypeAgg.A, "a"},
            {SerotypeAgg.B, "b"},
            {SerotypeAgg.C, "c"},
            {SerotypeAgg.D, "d"},
            {SerotypeAgg.E, "e"},
            {SerotypeAgg.F, "f"},
            {SerotypeAgg.Negative, "NT"},
            {SerotypeAgg.Auto, "NT"},
            {SerotypeAgg.Poly, "NT"},
            {SerotypeAgg.NotEvaluable, ""}
        };

        private string ExportAgglutination(SerotypeAgg agglutination)
        {
            return _agglutinationToString[agglutination];
        }

        private static Dictionary<SerotypePcr, string> _serotypePcrToString = new Dictionary<SerotypePcr, string>
        {
            {SerotypePcr.NotDetermined, ""},
            {SerotypePcr.A, "a"},
            {SerotypePcr.B, "b"},
            {SerotypePcr.C, "c"},
            {SerotypePcr.D, "d"},
            {SerotypePcr.E, "e"},
            {SerotypePcr.F, "f"},
            {SerotypePcr.Negative, "NT"},
        };

        private string ExportSerotypePcr(SerotypePcr agglutination)
        {
            return _serotypePcrToString[agglutination];
        }

        private string ExportSex(Gender? gender)
        {
            return gender switch
            {
                Gender.Female => "f",
                Gender.Male => "m",
                _ => string.Empty
            };
        }

        private static string ExportSamplingLocation(SamplingLocation location, Sending sending)
        {
            return location switch
            {
                SamplingLocation.Blood => "blood",
                SamplingLocation.Liquor => "CSF",
                SamplingLocation.Other => sending.OtherSamplingLocation,
                _ => "other"
            };
        }

        private string ExportBetaLactamase(TestResult betalactamase, Sending sending)
        {
            return betalactamase switch
            {
                TestResult.Positive => "positive",
                TestResult.Negative => "negative",
                _ => string.Empty
            };
        }

        private int? GetMonthAge(Sending sending)
        {
            var monthAge = sending.Isolate.PatientMonthAgeAtSampling();
            return monthAge >=12 ? (int?) null : monthAge;
        }

        private void AddNullColumn(string headerName)
        {
            AddField(sending => (object) null, headerName);
        }
    }
}