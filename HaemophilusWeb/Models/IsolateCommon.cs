using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Script.Serialization;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Utils;

namespace HaemophilusWeb.Models
{
    public abstract class IsolateCommon
    {
        [NotMapped]
        public DatabaseType DatabaseType { get; }

        public IsolateCommon(DatabaseType databaseType)
        {
            DatabaseType = databaseType;
        }

        private string customLaboratoryNumber;

        [Display(Name = "Stammnummer")]
        public int? StemNumber { get; set; }

        [NotMapped]
        [Display(Name = "Stammnummer")]
        public string StemNumberWithPrefix => StemNumber.ToStemNumberWithPrefix(DatabaseType);

        public TestResult Oxidase { get; set; }
        
        [Display(Name = "NHS Real-Time-PCR")]
        public NativeMaterialTestResult RealTimePcr { get; set; }

        [Display(Name = "NHS Real-Time-PCR Auswertung (RIDOM)")]
        public RealTimePcrResult RealTimePcrResult { get; set; }

        [Display(Name = "16S rDNA")]
        public NativeMaterialTestResult RibosomalRna16S { get; set; }
        
        [Display(Name = "16S rDNA beste Übereinstimmung")]
        public string RibosomalRna16SBestMatch { get; set; }
        
        [Display(Name = "16S rDNA Übereinstimmung")]
        public double? RibosomalRna16SMatchInPercent { get; set; }

        [Display(Name = "MALDI-TOF (VITEK MS)")]
        public UnspecificTestResult MaldiTofVitek { get; set; }

        [Display(Name = "MALDI-TOF (VITEK MS) beste Übereinstimmung")]
        public string MaldiTofVitekBestMatch { get; set; }

        [Display(Name = "MALDI-TOF (VITEK MS) Übereinstimmung")]
        public double? MaldiTofVitekMatchConfidence { get; set; }

        [Display(Name = "Befund am")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? ReportDate { get; set; }

        [Display(Name = "Befund-Status")]
        public ReportStatus ReportStatus { get; set; }

        [Display(Name = "Bemerkung")]
        public string Remark { get; set; }

        public virtual ICollection<EpsilometerTest> EpsilometerTests { get; set; }

        [Display(Name = "Labornummer")]
        [NotMapped]
        public string LaboratoryNumber
        {
            get => customLaboratoryNumber ?? ReportFormatter.ToLaboratoryNumber(YearlySequentialIsolateNumber, Year);
            set => customLaboratoryNumber = value;
        }

        [NotMapped]
        [Display(Name = "Labornummer")]
        public string LaboratoryNumberWithPrefix => LaboratoryNumber.ToLaboratoryNumberWithPrefix(DatabaseType);

        public int YearlySequentialIsolateNumber { get; set; }

        public int Year { get; set; }
    }
}