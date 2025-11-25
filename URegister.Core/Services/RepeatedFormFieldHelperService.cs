using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace URegister.Core.Services
{
    public static class RepeatedFormFieldHelperService
    {
        public static string InsertBeforeFirstUnderscore(string original, string toInsert)
        {
            if (string.IsNullOrEmpty(original) || string.IsNullOrEmpty(toInsert))
                return original;

            int underscoreIndex = original.IndexOf('_');
            if (underscoreIndex >= 0)
            {
                return original.Substring(0, underscoreIndex) + toInsert + original.Substring(underscoreIndex);
            }
            return original + toInsert;
        }

        public static int GetRepetitionIndex(string input)
        {
            if (string.IsNullOrEmpty(input))
                return 0;

            var match = Regex.Match(input, @"#(\d+)");
            return match.Success ? int.Parse(match.Groups[1].Value) : 0;
        }
    }
}
