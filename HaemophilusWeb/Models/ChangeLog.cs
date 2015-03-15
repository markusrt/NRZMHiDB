using System.Collections.Generic;
using System.Linq;

namespace HaemophilusWeb.Models
{
    public class ChangeLog
    {
        public IEnumerable<Change> NextChanges { get; set; }
        public IEnumerable<Change> PreviousChanges { get; set; }

        public ChangeLog(IEnumerable<Change> nextChanges, IEnumerable<Change> previousChanges)
        {
            NextChanges = AssignPriority(nextChanges);
            PreviousChanges = previousChanges.OrderByDescending(c => c.Date).ToList();
        }

        private static IEnumerable<Change> AssignPriority(IEnumerable<Change> nextChanges)
        {
            var nextChangesList = nextChanges.ToList();
            for (var i = 0; i < nextChangesList.Count; i++)
            {
                nextChangesList[i].Priority = i + 1;
            }
            return nextChangesList;
        }
    }
}