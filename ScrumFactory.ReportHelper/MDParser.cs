using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ScrumFactory.ReportHelper
{
    public class MDParser
    {

        private static Regex bold = new Regex(@"(__) (?=\S) (.+?[*_]*) (?<=\S) \1", RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);
        private static Regex italic = new Regex(@"(_) (?=\S) (.+?) (?<=\S) \1", RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);
        private static Regex quote = new Regex(@"(>) (?=\S) (.+?) (?<=\S) \1", RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);
        private static Regex code = new Regex(@"(`) (?=\S) (.+?) (?<=\S) \1", RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);

        public static string ConvertToXAML(string mdInput)
        {
            var text = mdInput;
            text = bold.Replace(text, @"<Bold>$2</Bold>");
            text = italic.Replace(text, @"<Italic>$2</Italic>");
            text = code.Replace(text, "<Span Background=\"#AAAAAA\">$2</Span>");

            text = ConvertEmojis(text);

            return text;

        }

        public static string ConvertToHTML(string mdInput)
        {
            var text = mdInput;
            text = bold.Replace(text, @"<b>$2</b>");
            text = italic.Replace(text, @"<i>$2</i>");
            text = code.Replace(text, "<span style=\"background-color: #AAAAAA\">$2</span>");

            text = ConvertEmojis(text);

            return text;

        }


        private static string ConvertEmojis(string mdInput)
        {
            var text = mdInput;
            text = text.Replace(":smile:", ":)");
            text = text.Replace(":smiley:", ":)");
            text = text.Replace(":wink:", ";)");
            text = text.Replace(":worried:", ":(");
            text = text.Replace(":sunglasses:", "8)");
            text = text.Replace(":neutral_face:", ":|");
            text = text.Replace(":open_mouth:", ":o");
            return text;
        }
    }

}
