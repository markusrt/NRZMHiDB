using System;
using System.Collections.Generic;
using HaemophilusWeb.Domain;
using HaemophilusWeb.Models;
using HaemophilusWeb.Utils;
using static HaemophilusWeb.Tools.PubMlstColumns;

namespace HaemophilusWeb.Tools
{
    public class HaemophilusPubMlstExport : SendingExportDefinition<Sending, Patient>
    {
        public HaemophilusPubMlstExport()
        {

            AddField(s => s.Patient.Initials, ColInitials);
            AddField(s => s.Patient.BirthDate, ColDateOfBirth);
            AddField(s => s.Patient.PostalCode, ColPostalCode);
            AddField(s => s.Isolate.StemNumberWithPrefix, "isolate");
            AddNullColumn("aliases");
            AddNullColumn("references");
            AddField(s => "Germany", ColCountry);
            AddField(s => ExportState(s.Patient.State), ColRegion);

            AddField(s => s.SamplingDate.HasValue ? (int?) s.SamplingDate.Value.Year : null, ColYear);
            AddField(s => s.ReceivingDate.ToReportFormatPubMlst(), ColSampleDate);
            AddField(s => s.SamplingDate.ToReportFormatPubMlst(), ColReceivedDate);

            AddNullColumn("non_culture");
            AddField(s => ExportEvaluation(s.Isolate.Evaluation), ColSerotype);
            AddField(s => ExportAgglutination(s.Isolate.Agglutination), ColSerotypeBySerology);
            AddField(s => ExportSerotypePcr(s.Isolate.SerotypePcr), ColSerotypeByPcr);
            AddNullColumn("genotype");
            AddNullColumn("biotype");
            AddNullColumn("ribotype");

            AddField(s => s.Isolate.PatientAgeAtSampling(), ColAgeInYears);
            AddField(s => GetMonthAge(s), ColAgeInMonths);

            AddField(s => ExportSex(s.Patient.Gender), ColSex);
            AddField(s => ExportSamplingLocation(s.SamplingLocation, s), ColSource);

            AddNullColumn("disease");
            AddNullColumn("epidemiology");
            AddNullColumn("MLEE_lineage");
            AddNullColumn("MLEE_ET");

            AddField(s => ExportBetaLactamase(s.Isolate.BetaLactamase, s), ColBetaLactamase);

            AddNullColumn(ColAmxMic);
            AddField(s => ExportEpsilometerTestResult(FindEpsilometerTestEvaluation(s, Antibiotic.Ampicillin)), ColAmxSir);
            AddField(s => FindEpsilometerTestMeasurement(s, Antibiotic.AmoxicillinClavulanate), ColAmcMic);
            AddField(s => ExportEpsilometerTestResult(FindEpsilometerTestEvaluation(s, Antibiotic.AmoxicillinClavulanate)), ColAmcSir);
            AddField(s => FindEpsilometerTestMeasurement(s, Antibiotic.Cefotaxime), ColCtxMic);
            AddField(s => ExportEpsilometerTestResult(FindEpsilometerTestEvaluation(s, Antibiotic.Cefotaxime)), ColCtxSir);

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

            AddField(s => s.Isolate.FtsiEvaluation3, ColFtsI);
        }

        private static string ExportState(State state)
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