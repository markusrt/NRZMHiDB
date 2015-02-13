using System;
using System.ComponentModel.DataAnnotations;

namespace HaemophilusWeb.ViewModels
{
    public class RkiExportQuery
    {
        [Display(Name = "Start-Datum")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime From { get; set; }

        [Display(Name = "Ende-Datum")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime To { get; set; }
    }
}