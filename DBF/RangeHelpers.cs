using DbfTests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbfTests
{
    internal static class RangeHelpers
    {
        internal static List<OutputRow> FillRange(this List<OutputRow> outputRows)
        {
            foreach (var item in outputRows)
            {
                if (item.Values.Count <= OutputRow.Headers.Count)
                {
                    item.Values.AddRange(new double?[OutputRow.Headers.Count - item.Values.Count]);
                }
            }
            return outputRows;
        }
    }
}
