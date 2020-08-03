namespace HaemophilusWeb.Tools
{
    public class PubMlstColumns : IDuplicatePatientDetectionColumns
    {
        public string Initials => "initials";
        public string DateOfBirth => "date_of_birth";
        public string PostalCode => "postal_code";
        public string StemNumber => "isolate";
        public string Country => "country";
        public string Region => "region";
        public string Year => "year";
        public string SampleDate => "date_sampled";
        public string ReceivedDate => "date_received";
        public string Serotype => "serotype";
        public string SerotypeBySerology => "serotype_by_serology";
        public string SerotypeByPcr => "serotype_by_PCR";
        public string AgeInYears => "age_yr";
        public string AgeInMonths => "age_mth";
        public string Sex => "sex";
        public string Source => "source";
        public string BetaLactamase => "beta_lactamase";
        public string AmxMic => "AMX_MIC";
        public string AmxSir => "AMX_SIR";
        public string AmcMic => "AMC_MIC";
        public string AmcSir => "AMC_SIR";
        public string CtxMic => "CTX_MIC";
        public string CtxSir =>  "CTX_SIR";
        public string FtsI => "ftsI";
    }
}