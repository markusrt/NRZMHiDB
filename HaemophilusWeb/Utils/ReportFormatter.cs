using System;
using System.Collections.Generic;
using System.Globalization;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Views.Utils;

namespace HaemophilusWeb.Utils
{
    public static class ReportFormatter
    {
        public static string ToReportFormat(this DateTime? dateTime)
        {
            return dateTime.ToReportFormat("keine Angabe");
        }

        public static string ToReportFormat(this DateTime? dateTime, string emptyString)
        {
            if (!dateTime.HasValue)
            {
                return emptyString;
            }
            return dateTime.Value.ToReportFormat();
        }

        public static string ToReportFormat(this DateTime dateTime)
        {
            return dateTime.ToString("dd.MM.yyyy");
        }

        public static string ToReportFormatMonthYear(this DateTime? dateTime)
        {
            if (!dateTime.HasValue)
            {
                return "";
            }
            return dateTime.Value.ToString("MM / yyyy", CultureInfo.InvariantCulture);
        }

        public static string ToReportFormatPubMlst(this DateTime? dateTime)
        {
            if (!dateTime.HasValue)
            {
                return "n.a.";
            }
            return dateTime.Value.ToReportFormatPubMlst();
        }

        public static string ToReportFormatPubMlst(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd");
        }

        public static string ToReportFormat(this PatientBase patient)
        {
            return $"{patient.Initials} / {patient.PostalCode}";
        }

        public static string ToStemNumberWithPrefix(this int? stemNumber, DatabaseType databaseType = DatabaseType.None)
        {
            var stemNumberString = stemNumber.HasValue? stemNumber.ToString() : " -";
            return $"{databaseType.StemNumberPrefix()}{stemNumberString}";
        }

        public static string StemNumberPrefix(this DatabaseType databaseType)
        {
            if (databaseType == DatabaseType.Haemophilus)
            {
                return "H";
            }
            if (databaseType == DatabaseType.Meningococci)
            {
                return "DE";
            }
            return "";
        }

        public static string ToLaboratoryNumber(int yearlySequentialIsolateNumber, int year, DatabaseType databaseType = DatabaseType.None)
        {
            return ToLaboratoryNumberWithPrefix($"{yearlySequentialIsolateNumber:000}/{year - 2000:00}", databaseType);
        }

        public static string ToLaboratoryNumberWithPrefix(this string laboratoryNumberWithoutPrefix, DatabaseType databaseType = DatabaseType.None)
        {
            if (databaseType == DatabaseType.Meningococci
                && !string.IsNullOrEmpty(laboratoryNumberWithoutPrefix)
                && laboratoryNumberWithoutPrefix.StartsWith("-"))
            {
                return "NR" + laboratoryNumberWithoutPrefix.TrimStart('-');
            }
            return $"{databaseType.LaboratoryNumberPrefix()}{laboratoryNumberWithoutPrefix??" -"}";
        }

        public static string LaboratoryNumberPrefix(this DatabaseType databaseType)
        {
            if (databaseType == DatabaseType.Haemophilus)
            {
                return "KL";
            }
            if (databaseType == DatabaseType.Meningococci)
            {
                return "MZ";
            }
            return "";
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