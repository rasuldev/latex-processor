using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using TextProcessor.Latex;

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
        /// <param name="blocknameInText"></param>
        /// <param name="envName"></param>
        /// <param name="labelPrefix"></param>
        /// <returns></returns>
        public static string WrapInEnvironment(string source, string startBlock, string endBlock, string blocknameInText, string envName, Func<string,string> labelForNum)
        {
            var sb = new StringBuilder(source);
            // First step: Find blocks and wrap them in environments
            var matches = Regex.Matches(source, startBlock);
            // Keeps map from numbers to labels
            var envNumbers = new List<string>();

            // Process matches (to avoid changing matching positions during replace we do processing from end to begin) 
            foreach (Match match in matches.OfType<Match>().OrderByDescending(m => m.Index))
            {
                Console.WriteLine("{0}({1})", match.Groups[1].Value.Trim(), match.Index);

                int startBlockPos = match.Index;
                sb.Remove(match.Index, match.Value.Length);
                var labelName = labelForNum(match.Groups[1].Value);
                sb.Insert(match.Index, String.Format(@"\begin{{{0}}}\label{{{1}}}", envName, labelName));
                envNumbers.Add(match.Groups[1].Value);

                // find endBlock
                int endBlockPos = sb.ToString().IndexOf(endBlock, startBlockPos);
                if (endBlockPos == -1)
                {
                    Console.WriteLine("Error on line: " + Utils.FindLine(sb.ToString(), startBlockPos));
                    continue;
                }


                sb.Remove(endBlockPos, endBlock.Length);
                sb.Insert(endBlockPos, String.Format(@"\end{{{0}}}", envName));
            }

            // Second step: find all references and replace them to \ref{labelName}
            ProcessRefs(sb, blocknameInText, envNumbers.ToArray(), labelForNum);

            return sb.ToString();
        }

        public static void ProcessRefs(StringBuilder sb, string blockname, string[] refsAsNums, Func<string, string> labelForNum)
        {
            var refsBlocksPos = new List<int>();
            var text = sb.ToString();
            int pos = 0;
            int len = blockname.Length;

            // Find blockname occurrences in text and save their pos
            while ((pos = text.IndexOf(blockname, pos)) > -1)
            {
                pos += len;
                refsBlocksPos.Add(pos);
            }


            foreach (var refsBlocksIndex in refsBlocksPos.OrderByDescending(x => x))
            {
                //Console.WriteLine(Utils.FindLine(text,refsBlocksIndex));
                // in right neighboorhood find numbers block. 
                // refsAsNumsBlock contains text like " 1, 2-3 и 7. "
                var refsAsNumsBlock = Utils.ExtractRefsBlock(text, refsBlocksIndex);
                // Replace numbers by \ref{label}
                var refs = InsertRefs(refsAsNumsBlock.Content, refsAsNums.ToArray(), labelForNum);

                // Replace refsAsNumsBlock by refs
                Utils.RemoveBlock(sb, refsAsNumsBlock);
                sb.Insert(refsAsNumsBlock.StartPos, refs);
            }
        }

        public static string InsertRefs(string textBlock, string[] refs, Func<string,string> labelForRef)
        {
            var sb = new StringBuilder(textBlock);
            foreach (var item in refs)
            {
                sb = sb.Replace(item, String.Format(@"\ref{{{0}}}", labelForRef(item)));
            }

            return sb.ToString();


            char[] separators = { ' ', ',', '-', 'и', '~' };
            
            // each chunk is a reference represented as a number (numRef). 
            // We're going to replace them by \ref{label}
            var numRefs = textBlock.Split(separators, StringSplitOptions.RemoveEmptyEntries).Select(s=>s.Trim()).ToList();
            
            // We don't touch numRef if it is not presented in refs array
            foreach (var numRef in numRefs)
            {
                if (refs.Contains(numRef))
                {
                    
                }
            }

        }
    }
}
