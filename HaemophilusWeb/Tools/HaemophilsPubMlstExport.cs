using System;
using HaemophilusWeb.Domain;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
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
            AddField(s => ExportToString(s.Patient.State), "region");

            AddField(s => s.SamplingDate.HasValue ? (int?) s.SamplingDate.Value.Year : null, "year");
            AddField(s => s.ReceivingDate.ToReportFormatPubMlst(), "date_sampled");
            AddField(s => s.SamplingDate.ToReportFormatPubMlst(), "date_received");
            
            AddNullColumn("non_culture");
            AddNullColumn("serotype"); //TODO
            AddNullColumn("serotype_by_serology"); //TODO
            AddNullColumn("serotype_by_PCR"); //TODO
            AddNullColumn("genotype");
            AddNullColumn("biotype");
            AddNullColumn("ribotype");

            AddField(s => s.Isolate.PatientAgeAtSampling(), "age_yr");
            AddField(s => GetMonthAge(s), "age_mth");

            AddField(s => ExportSex(s.Patient.Gender), "sex"); //TODO check question on #45
            AddNullColumn("source"); //TODO

            AddNullColumn("disease");
            AddNullColumn("epidemiology");
            AddNullColumn("MLEE_lineage");
            AddNullColumn("MLEE_ET");

            AddNullColumn("beta_lactamase"); //TODO

            AddNullColumn("AMX_MIC"); //TODO check question on #45
            AddNullColumn("AMX_SIR"); //TODO
            AddNullColumn("AMC_MIC"); //TODO check question on #45
            AddNullColumn("AMC_SIR"); //TODO
            AddNullColumn("CTX_MIC"); //TODO
            AddNullColumn("CTX_SIR"); //TODO

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
            
            AddNullColumn("ftsI"); //TODO
        }

        private string ExportSex(Gender? gender)
        {
            switch (gender)
            {
                case Gender.Female:
                    return "f";
                case Gender.Male:
                    return "m";
                default:
                    return string.Empty;
            }
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