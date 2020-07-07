using System;
using System.ComponentModel.DataAnnotations;
using HaemophilusWeb.Models;

namespace HaemophilusWeb.ViewModels
{
    public class FromToQuery
    {
        [Display(Name = "Start-Datum")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime From { get; set; }

        [Display(Name = "Ende-Datum")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime To { get; set; }
    }

    public class PubMlstQuery : FromToQuery
    {
        [Display(Name = "Unbereinigte Daten")]
        public YesNo Unadjusted { get; set; }
    }
}