using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Script.Serialization;

namespace HaemophilusWeb.Models
{
    public class IsolateCommon
    {
        [Display(Name = "Stammnummer")]
        public int? StemNumber { get; set; }

        public TestResult Oxidase { get; set; }

        [Display(Name = "16S rRNA")]
        public UnspecificTestResult RibosomalRna16S { get; set; }

        [Display(Name = "16S rRNA beste Übereinstimmung")]
        public string RibosomalRna16SBestMatch { get; set; }

        [Display(Name = "16S rRNA Übereinstimmung")]
        public double? RibosomalRna16SMatchInPercent { get; set; }

        [Display(Name = "api NH")]
        public UnspecificTestResult ApiNh { get; set; }

        [Display(Name = "api NH beste Übereinstimmung")]
        public string ApiNhBestMatch { get; set; }

        [Display(Name = "api NH Übereinstimmung")]
        public double? ApiNhMatchInPercent { get; set; }

        [Display(Name = "MALDI-TOF")]
        public UnspecificTestResult MaldiTof { get; set; }

        [Display(Name = "MALDI-TOF beste Übereinstimmung")]
        public string MaldiTofBestMatch { get; set; }

        [Display(Name = "MALDI-TOF Übereinstimmung")]
        public double? MaldiTofMatchConfidence { get; set; }

        [Display(Name = "Befund am")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? ReportDate { get; set; }

        [Display(Name = "Befund-Status")]
        public ReportStatus ReportStatus { get; set; }

        [Display(Name = "Bemerkung")]
        public string Remark { get; set; }

        [Display(Name = "Wachstum")]
        public YesNoOptional Growth { get; set; }

        public virtual ICollection<EpsilometerTest> EpsilometerTests { get; set; }
    }
}