using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HaemophilusWeb.Views.Utils
{
    public static class AutoCompleteUtils
    {
        public static IEnumerable<string> AsDataList(this IEnumerable<string> possibleItems)
        {
            return possibleItems.Distinct().OrderBy(i => i);
        }
    }
}