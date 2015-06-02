using System;
using System.ComponentModel.DataAnnotations;

namespace HaemophilusWeb.Models
{
    public class LaboratoryExportRecord : ExportRecord
    {
        [Display(Name = "Labornummer")]
        public string LaboratoryNumber { get; set; }

        [Display(Name = "Befund am")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? ReportDate { get; set; }

        [Display(Name = "Material")]
        public string Material { get; set; }

        [Display(Name = "Invasiv")]
        public string Invasive { get; set; }

        [Display(Name = "Labnr. Einsender")]
        public string SenderLaboratoryNumber { get; set; }

        [Display(Name = "Ergebnis Einsender")]
        public string SenderConclusion { get; set; }

        [Display(Name = "Initialen")]
        public string Initials { get; set; }

        [Display(Name = "Geburtsdatum")]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "Patientenalter bei Entnahme")]
        public int PatientAgeAtSampling { get; set; }

        [Display(Name = "Postleitzahl")]
        public string PostalCode { get; set; }

        [Display(Name = "Wohnort")]
        public string City { get; set; }

        [Display(Name = "Klinische Angaben")]
        public string ClinicalInformation { get; set; }

        [Display(Name = "Hib-Impfung")]
        public string HibVaccination { get; set; }

        [Display(Name = "Datum Hib-Impfung")]
        public DateTime? HibVaccinationDate { get; set; }

        [Display(Name = "Therapie")]
        public string Therapy { get; set; }

        [Display(Name = "Therapie Details")]
        public string TherapyDetails { get; set; }

        [Display(Name = "Faktoren-test")]
        public string FactorTest { get; set; }

        public string Agglutination { get; set; }

        public string Oxidase { get; set; }

        [Display(Name = "ompP2")]
        public string OuterMembraneProteinP2 { get; set; }

        [Display(Name = "ompP6")]
        public string OuterMembraneProteinP6 { get; set; }

        [Display(Name = "fucK")]
        public string FuculoKinase { get; set; }

        [Display(Name = "bexA")]
        public string BexA { get; set; }

        [Display(Name = "Serotyp-PCR")]
        public string SerotypePcr { get; set; }

        [Display(Name = "16S rRNA")]
        public string RibosomalRna16S { get; set; }

        [Display(Name = "16S rRNA beste Übereinstimmung")]
        public string RibosomalRna16SBestMatch { get; set; }

        [Display(Name = "16S rRNA Übereinstimmung")]
        public double? RibosomalRna16SMatchInPercent { get; set; }

        [Display(Name = "api NH")]
        public string ApiNh { get; set; }

        [Display(Name = "api NH beste Übereinstimmung")]
        public string ApiNhBestMatch { get; set; }

        [Display(Name = "api NH Übereinstimmung")]
        public double? ApiNhMatchInPercent { get; set; }

        [Display(Name = "MALDI-TOF")]
        public string MaldiTof { get; set; }

        [Display(Name = "MALDI-TOF beste Übereinstimmung")]
        public string MaldiTofBestMatch { get; set; }

        [Display(Name = "MALDI-TOF Übereinstimmung")]
        public double? MaldiTofMatchConfidence { get; set; }

        [Display(Name = "ftsI")]
        public string Ftsi { get; set; }

        [Display(Name = "ftsI Beurteilung 1")]
        public string FtsiEvaluation1 { get; set; }

        [Display(Name = "ftsI Beurteilung 2")]
        public string FtsiEvaluation2 { get; set; }

        [Display(Name = "MLST")]
        public string Mlst { get; set; }

        [Display(Name = "MLST Sequenztyp")]
        public string MlstSequenceType { get; set; }

        [Display(Name = "Bemerkung (Isolat)")]
        public string IsolateRemark { get; set; }

        [Display(Name = "Bemerkung (Einsendung)")]
        public string SendingRemark { get; set; }

        [Display(Name = "Cefotaxim MHK")]
        public double? CefotaximeMeasurement { get; set; }

        [Display(Name = "Cefotaxim Bewertung")]
        public string CefotaximeEpsilometerTestResult { get; set; }

        [Display(Name = "Meropenem MHK")]
        public double? MeropenemMeasurement { get; set; }

        [Display(Name = "Bewertung Meropenem")]
        public string MeropenemEpsilometerTestResult { get; set; }
    }
}