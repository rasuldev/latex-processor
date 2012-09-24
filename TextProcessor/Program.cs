﻿using System;
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
            var matches = Regex.Matches(str, @"\\eqno(?:.|\r?\n)*?\((.+?)\)");

            // In eqNumbers we will collect equation numbers
            // It will be used to change formula references from (52.1) to \eqref{52.1}
            List<string> eqNumbers= new List<string>();
            
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
            string[] textFragments = Regex.Split(text, @"(?=\$|\\begin{equation}|\\end{equation})");

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
            text = Regex.Replace(text, @"(\r?\n){2,}\\end{equation}", "\r\n\\end{equation}");
            // Removing redundant newlines after \begin{equation}
            text = Regex.Replace(text, @"\\begin{equation}(?<label>\\label{.*?})?(\r?\n){2,}", "\\begin{equation}${label}\r\n");

            File.WriteAllText(destinationFilename, text, encoding);
        }
    }
}
