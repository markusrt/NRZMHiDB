using System.Collections.Generic;
using System.Data;
using System.Linq;
using static HaemophilusWeb.Tools.PubMlstColumns;


namespace HaemophilusWeb.Tools
{
    public static class PubMlstDuplicateResolver
    {
        public static void CleanOrMarkDuplicates(DataTable table)
        {
            ClearEntriesWithEmptyStrainParameters(table);

            var duplicatePatientses = GroupDuplicatePatients(table);
            foreach (var duplicatePatients in duplicatePatientses)
            {
                foreach (var duplicateRow
                    in duplicatePatients.OrderBy(r => r[ColIsolate]).Skip(1))
                {
                    table.Rows.Remove(duplicateRow);    
                }
            }
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
                foreach (var potentialEmpty in duplicatePatients.ToList().Where(potentialEmpty => string.IsNullOrEmpty(
                    $"{potentialEmpty[ColBetaLactamase]}{potentialEmpty[ColAmxSir]}{potentialEmpty[ColSerotype]}")))
                {
                    table.Rows.Remove(potentialEmpty);
                }
            }
        }

        private static IEnumerable<IGrouping<string, DataRow>> GroupDuplicatePatients(DataTable table)
        {
            return table.AsEnumerable().GroupBy(t => $"{t[ColInitials]}{t[ColDateOfBirth]}{t[ColPostalCode]}{t[ColSex]}");
        }
    }
}