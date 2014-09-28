using System;
using System.Collections.Generic;
using HaemophilusWeb.Models;
using HaemophilusWeb.Views.Utils;

namespace HaemophilusWeb.Utils
{
    public static class ReportFormatter
    {
        public static string ToReportFormat(this DateTime? dateTime)
        {
            if (!dateTime.HasValue)
            {
                return "keine Angabe";
            }
            return dateTime.Value.ToReportFormat();
        }

        public static string ToReportFormat(this DateTime dateTime)
        {
            return dateTime.ToString("dd.MM.yyyy");
        }

        public static string ToReportFormat(this Patient patient)
        {
            return string.Format("{0} / {1}", patient.Initials, patient.PostalCode);
        }


        private static readonly List<Evaluation> EvaluationHaemophilus = new List<Evaluation>
        {
            Evaluation.HaemophilusTypeA,
            Evaluation.HaemophilusTypeB,
            Evaluation.HaemophilusTypeC,
            Evaluation.HaemophilusTypeD,
            Evaluation.HaemophilusTypeE,
            Evaluation.HaemophilusTypeF,
            Evaluation.HaemophilusNonEncapsulated
        };

        public static string ToReportFormat(this Evaluation evaluation)
        {
            return EvaluationHaemophilus.Contains(evaluation)
                ? "Haemophilus influenzae"
                : EnumEditor.GetEnumDescription(evaluation);
        }
    }
}