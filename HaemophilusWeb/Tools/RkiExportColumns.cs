namespace HaemophilusWeb.Tools
{
    public class RkiExportColumns : IDuplicatePatientDetectionColumns
    {
        public string Initials => "initials";
        public string DateOfBirth => "date_of_birth";
        public string PostalCode => "postal_code";
        public string HibVaccination => "hib_impf";
        public string State => "bundeslandName";
        public string StateNumber => "bundesland";
        public string County => "landkreis";
        public string CountyNumber => "kreis_nr";
        public string SenderId => "einsender";
        public string BirthMonth => "geb_monat";
        public string BirthYear => "geb_jahr";
        public string StemNumber => "klhi_nr";
        public string SampleDate => "ent";
        public string ReceivedDate => "eing";
        public string Serotype => "styp";
        public string Sex => "geschlecht";
        public string Source => "mat";
        public string BetaLactamase => "b_lac";
        public string AmxMic => "ampicillinMHK";
        public string AmxSir => "ampicillinBewertung";
        public string AmcMic => "amoxicillinClavulansaeureMHK";
        public string AmcSir => "bewertungAmoxicillinClavulansaeure";
    }
}