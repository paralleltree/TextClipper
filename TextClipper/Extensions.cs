using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextClipper
{
    static class Extensions
    {
        public static IEnumerable<string> SplitByNewLine(this string source)
        {
            return System.Text.RegularExpressions.Regex.Split(source, "\r*\n+");
        }
    }
}
