using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbfTests
{
    internal class OutputRow : IComparable
    {
        internal DateTime Timestamp { get; set; }
        internal List<double?> Values { get; set; } = new List<double?>();
        /// <summary>
        /// shall be 1-n directory names where a 128.dbf files was found. Order must be identical to <see cref="Values"/> list order
        /// </summary>
        internal static List<string> Headers { get; set; } = new List<string>();

        internal string AsTextLine()
        {
            return $"{Timestamp}\t{string.Join("\t", Values)}";
        }

        public int CompareTo(object obj)
        {
            if (obj == null || !(obj is OutputRow))
            {
                return 1;
            }
            return Timestamp.CompareTo(((OutputRow)obj).Timestamp);
        }
    }
}
