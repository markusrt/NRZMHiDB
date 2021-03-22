using System;
using System.ComponentModel.DataAnnotations;

namespace HaemophilusWeb.Models
{
    public class Change
    {
        public Change(DateTime date, string details, ChangeType type, DatabaseType database = DatabaseType.Haemophilus, int priority = 0)
        {
            Date = date;
            Details = details;
            Type = type;
            Priority = priority;
            Database = database;
        }

        public Change(DateTime date, string details, int gitHubIssue, ChangeType type, DatabaseType database = DatabaseType.Haemophilus, int priority = 0) : this(date, "", type, database, priority)
        {
            Details = $"<p>{details}</p>" +
                      $"<p><a class=\"btn btn-default btn-xs\" href=\"https://github.com/markusrt/NRZMHiDB/issues/{gitHubIssue}\"><span class=\"glyphicon glyphicon-link\" aria-hidden=\"true\"></span> GitHub Issue {gitHubIssue}</a></p>";
        }

        [Display(Name = "Priorität")]
        public int Priority { get; set; }

        [Display(Name = "Datum")]
        public DateTime Date { get; set; }

        [Display(Name = "Details")]
        public string Details { get; set; }

        [Display(Name = "Typ")]
        public ChangeType Type { get; set; }

        public DatabaseType Database { get; }
    }
}