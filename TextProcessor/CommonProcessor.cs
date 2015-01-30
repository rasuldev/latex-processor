using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TextProcessor
{
    public static class CommonProcessor
    {
        // todo: works wrong when labels inside other than equation environments 
        public static string MakeDollarsFromEquationWithLabels(string source)
        {
            var sb = new StringBuilder(source);

            // Replacing \begin{equation},\end{equation} with $$
            sb = sb.Replace(@"\begin{equation}", "$$");
            sb = sb.Replace(@"\end{equation}", "$$");

            // Harvesting labels, placing eqno() and removing labels
            var labels = new Stack<string>();
            var matches = Regex.Matches(sb.ToString(), @"\\label(?:.|\r?\n)*?\{(.+?)\}");
            // Labels count
            var eqNumber = matches.Count;

            // Enumeration is started from end.
            // So last equation number would be equal to labels count
            foreach (Match match in matches.OfType<Match>().OrderByDescending(m => m.Index))
            {
                labels.Push(match.Groups[1].Value);
                var endDollarsPos = sb.ToString().IndexOf("$$", match.Index);
                sb = sb.Insert(endDollarsPos, @"\eqno(" + eqNumber + ")\r\n");
                eqNumber--;
                sb = sb.Remove(match.Index, match.Length);
            }

            // Replacing \eqref 
            var labelsArr = labels.ToArray();
            for (int i = 0; i < labelsArr.Length; i++)
            {
                sb = sb.Replace(@"\eqref{" + labelsArr[i] + @"}", "(" + (i + 1) + ")");
            }

            return sb.ToString();
        }

        public static string MakeEquationWithLabelsFromDollars(string source, string labelPrefix = "eq")
        {
            var sb = new StringBuilder(source);

            // Find all occurrences of eqno and extract number
            var matches = Regex.Matches(source, @"(?:\r?\n)*\\eqno(?:.|\r?\n)*?\((.+?)\)");

            // In eqNumbers we will collect equation numbers
            // It will be used to change formula references from (52.1) to \eqref{52.1}
            List<string> eqNumbers = new List<string>();

            // Process matches (to avoid changing matching positions during replace we do processing from end to begin) 
            foreach (Match match in matches.OfType<Match>().OrderByDescending(m => m.Index))
            {
                Console.WriteLine("{0}({1})", match.Groups[1].Value.Trim(), match.Index);

                int endDollarsPos = sb.ToString().IndexOf("$$", match.Index);
                sb = sb.Replace("$$", @"\end{equation}", endDollarsPos, 2);

                sb = sb.Remove(match.Index, match.Value.Length);

                int beginDollarsPos = sb.ToString().LastIndexOf("$$", match.Index);
                var eqNumber = match.Groups[1].Value.Trim();
                sb = sb.Replace("$$", @"\begin{equation}\label{" + labelPrefix + eqNumber + "}", beginDollarsPos, 2);
                eqNumbers.Add(eqNumber);
            }

            // Changing formula references
            string text = sb.ToString();

            // Splitting into fragments: fragments with even indices are outside of math area
            string[] textFragments = Regex.Split(text, @"(?=\$\$|[^\$]\$[^\$])|(?=\\begin{equation}|\\end{equation})");

            // Test 
            var text2 = string.Join("", textFragments);
            if (text == text2)
            {
                Console.Write("");
            }

            foreach (var eqNumber in eqNumbers)
            {
                // Processing only even fragments
                for (int i = 0; i < textFragments.Length; i += 2)
                {
                    // fragment = textFragments[i];
                    textFragments[i] = Regex.Replace(textFragments[i], string.Format(@"\(\s*{0}\s*\)", Regex.Escape(eqNumber)), @"\eqref{" + labelPrefix + eqNumber + "}");
                }
            }
            // Join fragments 
            text = string.Join("", textFragments);

            // Removing redundant newlines before \end{equation}
            //text = Regex.Replace(text, @"(\r?\n\s*){2,}\\end{equation}", "\r\n\\end{equation}");
            // Removing redundant newlines after \begin{equation}
            //text = Regex.Replace(text, @"\\begin{equation}(?<label>\\label{.*?})?(\r?\n\s*){2,}", "\\begin{equation}${label}\r\n");
            return text;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="startBlock">Must contain one capture block ()</param>
        /// <param name="endBlock"></param>
        /// <param name="blockname"></param>
        /// <param name="envName"></param>
        /// <param name="labelPrefix"></param>
        /// <returns></returns>
        public static string WrapInEnvironment(string source, string startBlock, string endBlock, string blockname, string envName, string labelPrefix)
        {
            var sb = new StringBuilder(source);
            char[] passChars = { ' ', ',', '-', 'и' };
            var matches = Regex.Matches(source, startBlock);

            List<string> envNumbers = new List<string>();

            // Process matches (to avoid changing matching positions during replace we do processing from end to begin) 
            foreach (Match match in matches.OfType<Match>().OrderByDescending(m => m.Index))
            {
                Console.WriteLine("{0}({1})", match.Groups[1].Value.Trim(), match.Index);

                int startBlockPos = match.Index;
                sb = sb.Remove(match.Index, match.Value.Length);
                sb = sb.Insert(match.Index, String.Format(@"\begin{{{0}}}\label{{{1}:{2}}}", envName, labelPrefix, match.Groups[1].Value));

                // find endBlock
                int endBlockPos = sb.ToString().IndexOf(endBlock, startBlockPos);
                if (endBlockPos == -1)
                {
                    Console.WriteLine("Error: " + startBlockPos);
                    continue;
                }
                sb.Remove(endBlockPos, endBlock.Length);
                sb.Insert(endBlockPos, String.Format(@"\end{{{0}}}", envName));


            }

            return sb.ToString();
        }
    }
}
