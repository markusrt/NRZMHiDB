using System;
using System.ComponentModel.DataAnnotations;

namespace HaemophilusWeb.Models
{
    public class Change
    {
        public Change(DateTime date, string details, ChangeType type, int priority = 0)
        {
            Date = date;
            Details = details;
            Type = type;
            Priority = priority;
        }

        [Display(Name = "Priorität")]
        public int Priority { get; set; }

        [Display(Name = "Datum")]
        public DateTime Date { get; set; }

        [Display(Name = "Details")]
        public string Details { get; set; }

        [Display(Name = "Typ")]
        public ChangeType Type { get; set; }
    }
}