using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TextProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            // Choose encoding of your file: default is UTF-8
            // var encoding = Encoding.GetEncoding("windows-1251");
            var encoding = Encoding.GetEncoding("utf-8");
            ProcessFile(@"..\..\test.txt", @"..\..\test_processed.txt",encoding);
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
            var str = File.ReadAllText(sourceFilename, encoding);
            var sb = new StringBuilder(str);

            // Find all occurrences of eqno and extract number
            var matches = Regex.Matches(str, @"\\eqno *\((.+)\)");

            // In eqNumbers we will collect equation numbers
            // It will be used to change formula references from (52.1) to \eqref{52.1}
            List<string> eqNumbers= new List<string>();
            
            // Process matches (to avoid changing matching positions during replace we do processing from end to begin) 
            foreach (Match match in matches.OfType<Match>().OrderByDescending(m => m.Index))
            {
                //Console.WriteLine("{0}({1})", match.Groups[1].Value.Trim(), match.Index);

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
            foreach (var eqNumber in eqNumbers)
            {
                text = Regex.Replace(text, string.Format(@"\(\s*{0}\s*\)", eqNumber), @"\eqref{"+eqNumber+"}");
            }

            File.WriteAllText(destinationFilename, text, encoding);
        }
    }
}
