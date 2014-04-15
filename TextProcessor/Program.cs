using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TextProcessor.Latex;

namespace TextProcessor
{
    class Program
    {
        static void Main(string[] args)
        {

            Process2(@"d:\Dropbox\INFO_BASE\DOCS\000 DOC SRW\Rasul\Статьи\Скорость сходимости сумм Фурье-Хаара в весовых пространствах Лебега\Оформление\processing\Magomed-Kasumov — копия.tex",
                @"d:\Dropbox\INFO_BASE\DOCS\000 DOC SRW\Rasul\Статьи\Скорость сходимости сумм Фурье-Хаара в весовых пространствах Лебега\Оформление\processing\Magomed-Kasumov.tex",
                Encoding.GetEncoding("windows-1251"));

            //Process2(@"d:\Downloads\Саратов 04.2014\SharapudinovII_AknievGG.tex",
            //         @"d:\Downloads\Саратов 04.2014\SharapudinovII_AknievGG_p.tex",
            //    Encoding.GetEncoding("windows-1251"));



            return;

            // Choose encoding of your file: default is UTF-8
            // var encoding = Encoding.GetEncoding("windows-1251");
            var encoding = Encoding.GetEncoding("utf-8");
            //ProcessFile(@"..\..\test.txt", @"..\..\test_processed.txt",encoding);
            ProcessFile(@"..\..\test_processed.txt", @"..\..\test2.txt", encoding);
        }

        private static void Process2(string sourceFilename, string destinationFilename, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = new UTF8Encoding();
            var sb = new StringBuilder(File.ReadAllText(sourceFilename, encoding));
            
            // Macroses inlining
            var macrosDef = @"\newcommand{\norm}[1]{\|#1\|_{p(\cdot),w}}";
            sb.Replace(macrosDef, "");
            var macros = new Macros(macrosDef);
            macros.ApplyMacros(ref sb);

            // equation labels renaming
            Utils.RenameLabels(ref sb,"iish_gga_#num#");
            
            File.WriteAllText(destinationFilename, sb.ToString(), encoding);
        }



        /// <summary>
        /// Replaces $$-blocks that have \eqno{num} by 
        /// \begin{equation}\label{num} ... \end{equation}
        /// </summary>
        /// <param name="sourceFilename">File to be processed</param>
        /// <param name="destinationFilename">Result will be saved in this file</param>
        static void ProcessFile(string sourceFilename, string destinationFilename, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = new UTF8Encoding();
            var source = File.ReadAllText(sourceFilename, encoding);
            //var text = MakeEquationWithLabelsFromDollars(source);
            var text = MakeDollarsFromEquationWithLabels(source);
            File.WriteAllText(destinationFilename, text, encoding);
        }

        static string MakeDollarsFromEquationWithLabels(string source)
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

        static string MakeEquationWithLabelsFromDollars(string source)
        {
            var sb = new StringBuilder(source);

            // Find all occurrences of eqno and extract number
            var matches = Regex.Matches(source, @"\\eqno(?:.|\r?\n)*?\((.+?)\)");

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
                sb = sb.Replace("$$", @"\begin{equation}\label{" + eqNumber + "}", beginDollarsPos, 2);
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
                    textFragments[i] = Regex.Replace(textFragments[i], string.Format(@"\(\s*{0}\s*\)", eqNumber), @"\eqref{" + eqNumber + "}");
                }
            }
            // Join fragments 
            text = string.Join("", textFragments);

            // Removing redundant newlines before \end{equation}
            text = Regex.Replace(text, @"(\r?\n\s*){2,}\\end{equation}", "\r\n\\end{equation}");
            // Removing redundant newlines after \begin{equation}
            text = Regex.Replace(text, @"\\begin{equation}(?<label>\\label{.*?})?(\r?\n\s*){2,}", "\\begin{equation}${label}\r\n");
            return text;
        }
    }
}
