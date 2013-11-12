using System;
using System.ComponentModel.DataAnnotations;

namespace HaemophilusWeb.Models
{
    public class Change
    {
        public Change(DateTime date, string details, ChangeType type)
        {
            Date = date;
            Details = details;
            Type = type;
        }

        [Display(Name = "Datum")]
        public DateTime Date { get; set; }

        [Display(Name = "Details")]
        public string Details { get; set; }

        [Display(Name = "Typ")]
        public ChangeType Type { get; set; }
    }
}