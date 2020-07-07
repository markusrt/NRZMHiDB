using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NodaTime;
using static HaemophilusWeb.Tools.PubMlstColumns;


namespace HaemophilusWeb.Tools
{
    public static class PubMlstDuplicateResolver
    {
        public static void CleanOrMarkDuplicates(DataTable table)
        {
            ClearEntriesWithEmptyStrainParameters(table);
            CleanDuplicateIdenticalStrainsNotTooFarApart(table);
            MarkDuplicateGroups(table);
            RemovePatientData(table);
        }


        /// <summary>
        /// If all patient parameters are identical, but all strain parameters are blank in one
        /// submission,  then eliminate the complete submission with blank parameters
        /// <p>
        /// <b>Explanation:</b>
        /// In case there was no growth, we aks the laboratory to resubmit the strain. Usually, the
        /// strain can be cultivated and analysed upon resubmission.  Since all our submissions are
        /// documented, these cases create duplicate datasets for the same strain
        /// (the first one with no results, the second one with valid results).
        /// </p>
        /// </summary>
        /// <param name="table"></param>
        private static void ClearEntriesWithEmptyStrainParameters(DataTable table)
        {
            var groupDuplicatePatients = GroupDuplicatePatients(table);
            foreach (var duplicatePatients in groupDuplicatePatients)
            {
                foreach (var potentialEmpty in duplicatePatients.ToList().Where(potentialEmpty => string.IsNullOrEmpty(GetStrainId(potentialEmpty))))
                {
                    table.Rows.Remove(potentialEmpty);
                }
            }
        }

        /// <summary>
        /// If all parameters are identical, but date sampled are more than 6 months apart,
        /// then indicate both submissions. If all patient parameters are identical, but at
        /// least one of the strain parameters differs, then indicate both submissions
        /// </summary>
        private static void CleanDuplicateIdenticalStrainsNotTooFarApart(DataTable table)
        {
            foreach (var duplicatePatients in GroupDuplicatePatients(table))
            {
                var knownStrainIds = new Dictionary<string, DateTime>();
                foreach (var row in duplicatePatients.OrderBy(r => r[ColIsolate]))
                {
                    var strainId = GetStrainId(row, true);
                    var containsKey = knownStrainIds.TryGetValue(strainId, out var previousReceivingDate);
                    var currentReceivingDate = DateTime.Parse(row.Field<string>(ColReceivedDate));
                    var period1 = Period.Between(
                        LocalDate.FromDateTime(currentReceivingDate),
                        LocalDate.FromDateTime(previousReceivingDate));
                    if (containsKey && period1.Months >= 0 && period1.Months < 6)
                    {
                        table.Rows.Remove(row);
                    }
                    else
                    {
                        if (containsKey)
                        {
                            knownStrainIds.Remove(strainId);
                        }
                        knownStrainIds.Add(strainId, currentReceivingDate);
                    }
                }
            }
        }

       
        private static void MarkDuplicateGroups(DataTable table)
        {
            var duplicateGroupId = 1;
            foreach (var duplicatePatients in GroupDuplicatePatients(table))
            {
                if (duplicatePatients.Count() <= 1)
                {
                    continue;
                }

                if (!table.Columns.Contains(ColDuplicateGroup))
                {
                    table.Columns.Add(ColDuplicateGroup);
                }

                foreach (var row in duplicatePatients)
                {
                    row[ColDuplicateGroup] = $"Group {duplicateGroupId}";
                }

                duplicateGroupId++;
            }
        }
        
        public static void RemovePatientData(DataTable table)
        {
            table.Columns.Remove(ColInitials);
            table.Columns.Remove(ColDateOfBirth);
            table.Columns.Remove(ColPostalCode);
        }

        private static string GetStrainId(DataRow row, bool withSource = false)
        {
            var source = withSource ? row[ColSource] : "";
            return $"{row[ColBetaLactamase]}{row[ColAmxSir]}{row[ColSerotype]}{source}";
        }

        private static IEnumerable<IGrouping<string, DataRow>> GroupDuplicatePatients(DataTable table)
        {
            return table.AsEnumerable().GroupBy(t => $"{t[ColInitials]}{t[ColDateOfBirth]}{t[ColPostalCode]}{t[ColSex]}");
        }
    }
}