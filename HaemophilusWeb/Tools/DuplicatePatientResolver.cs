using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NodaTime;

namespace HaemophilusWeb.Tools
{
    public class DuplicatePatientResolver
    {
        public const string ColDuplicateGroup = "duplicate_group";

        private readonly IDuplicatePatientDetectionColumns _col;

        public DuplicatePatientResolver(IDuplicatePatientDetectionColumns duplicatePatientDetectionColumns)
        {
            _col = duplicatePatientDetectionColumns;
        }

        public void CleanOrMarkDuplicates(DataTable table)
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
        private void ClearEntriesWithEmptyStrainParameters(DataTable table)
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
        private void CleanDuplicateIdenticalStrainsNotTooFarApart(DataTable table)
        {
            foreach (var duplicatePatients in GroupDuplicatePatients(table))
            {
                var knownStrainIds = new Dictionary<string, DateTime>();
                foreach (var row in duplicatePatients.OrderBy(r => r[_col.StemNumber]))
                {
                    var strainId = GetStrainId(row, true);
                    var containsKey = knownStrainIds.TryGetValue(strainId, out var previousReceivingDate);
                    var currentReceivingDate = DateTime.Parse(row.Field<string>(_col.ReceivedDate));
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

       
        private void MarkDuplicateGroups(DataTable table)
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
        
        public void RemovePatientData(DataTable table)
        {
            table.Columns.Remove(_col.Initials);
            table.Columns.Remove(_col.DateOfBirth);
            table.Columns.Remove(_col.PostalCode);
        }

        private string GetStrainId(DataRow row, bool withSource = false)
        {
            var source = withSource ? row[_col.Source] : "";
            return $"{row[_col.BetaLactamase]}{row[_col.AmxSir]}{row[_col.Serotype]}{source}";
        }

        private IEnumerable<IGrouping<string, DataRow>> GroupDuplicatePatients(DataTable table)
        {
            return table.AsEnumerable().GroupBy(t => $"{t[_col.Initials]}{t[_col.DateOfBirth]}{t[_col.PostalCode]}{t[_col.Sex]}");
        }
    }
}