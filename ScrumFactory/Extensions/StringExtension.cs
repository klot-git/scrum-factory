using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrumFactory.Extensions {

    public static class StringExtension {

        private static System.Text.RegularExpressions.Regex nonSpacingMarkRegex = new System.Text.RegularExpressions.Regex(@"\p{Mn}", System.Text.RegularExpressions.RegexOptions.Compiled);

        public static bool NormalizedContains(this string str, string other) {

            string norm1 = str.Normalize(System.Text.NormalizationForm.FormD).ToLower();
            norm1 = nonSpacingMarkRegex.Replace(norm1, string.Empty);

            string norm2 = other.Normalize(System.Text.NormalizationForm.FormD).ToLower();
            norm2 = nonSpacingMarkRegex.Replace(norm2, string.Empty);

            return norm1.Contains(norm2);
            
        }

        public static string NormalizeD(this string str) {

            string norm1 = str.Normalize(System.Text.NormalizationForm.FormD).ToLower();
            norm1 = nonSpacingMarkRegex.Replace(norm1, string.Empty);            
           return norm1;
        }


    }
}
