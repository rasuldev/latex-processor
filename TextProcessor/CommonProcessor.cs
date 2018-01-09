using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices;
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

            // Removing blank lines at begin or end of $$-blocks
            var cleaned = Regex.Replace(sb.ToString(), @"\$\$(\r?\n){2,}", "$$$$\r\n");
            //var cleaned = Regex.Replace(sb.ToString(), @"(?<=\$\$.*?)(\r?\n){2,}(?=.*?\$\$)", "\r\n");
            // TODO: remove all blank lines inside $$-blocks
            //cleaned = Regex.Replace(cleaned, @"(\r?\n)+\$\$", "\r\n$$$$");
            return cleaned;
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
        public static string WrapInEnvironment(string source, string startBlock, string endBlock, string blocknameInText, string envName, Func<string, string> labelForNum)
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

        public static string InsertRefs(string textBlock, string[] refs, Func<string, string> labelForRef)
        {
            var sb = new StringBuilder(textBlock);
            foreach (var item in refs)
            {
                sb = sb.Replace(item, $@"\ref{{{labelForRef(item)}}}");
            }

            return sb.ToString();


            char[] separators = { ' ', ',', '-', 'и', '~' };

            // each chunk is a reference represented as a number (numRef). 
            // We're going to replace them by \ref{label}
            var numRefs = textBlock.Split(separators, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

            // We don't touch numRef if it is not presented in refs array
            foreach (var numRef in numRefs)
            {
                if (refs.Contains(numRef))
                {

                }
            }

        }

        /// <summary>
        /// It's supposed that among sources there is a source with thebibliography environment. 
        /// This env will be processed first. Then all cites in sources will be modified correspondingly.
        /// </summary>
        /// <param name="sources"></param>
        /// <returns></returns>
        public static List<string> MergeBibitemsAndReplaceCites(IList<string> sources)
        {
            // find thebibliography env
            Tuple<string, Dictionary<string, string>> mergeResult = null;
            var modSources = new List<string>(sources);
            for (int i = 0; i < modSources.Count; i++)
            {
                var source = sources[i];
                if (source.Contains("thebibliography"))
                {
                    mergeResult = MergeBibitems(source);
                    modSources[i] = mergeResult.Item1;
                    break;
                }
            }

            if (mergeResult == null)
                throw new Exception("thebibliography environment not found");

            var newKeysFor = mergeResult.Item2;
            for (int i = 0; i < modSources.Count; i++)
            {
                var source = modSources[i];
                modSources[i] = ReplaceCitesKeys(source, newKeysFor);
            }
            return modSources;
        }

        private static string ReplaceCitesKeys(string source, Dictionary<string, string> newKeysFor)
        {
            var cites = Utils.GetCites(source);
            var sb = new StringBuilder(source);
            foreach (var cite in cites.OrderByDescending(c => c.Block.StartPos))
            {
                cite.Keys = cite.Keys.Select(k =>
                {
                    if (newKeysFor.ContainsKey(k))
                        return newKeysFor[k];
                    else
                    {
                        Console.WriteLine($"Warning: not correspondence for bibkey {k}");
                        return k;
                    }
                }).ToList();
                sb = sb.Remove(cite.Block.StartPos, cite.Block.EndPos - cite.Block.StartPos + 1)
                       .Insert(cite.Block.StartPos, cite.ToString());
            }
            return sb.ToString();
        }

        /// <summary>
        /// Finds identical bibitems and merges them
        /// </summary>
        /// <returns>
        /// New merged list of bibitems as string and 
        /// correspondence between original keys and new keys:
        /// result.Item2['oldKey'] returns key of bibitem from merged list
        /// </returns>
        public static Tuple<string, Dictionary<string, string>> MergeBibitems(string text)
        {
            var bibenv = Utils.FindEnv(text, "thebibliography");
            var bibitems = Utils.GetBibitems(text, bibenv);

            // Make groups: each group contains identical bibitems
            var groups = bibitems.GroupBy(b => ExtractComparablePart(b.FullTitle));
            var keysOldNew = new Dictionary<string, string>();
            var filteredBibitems = new List<Bibitem>();

            foreach (var g in groups)
            {
                // Take one (first) bibitem from all groups
                var bibitem = g.First();
                filteredBibitems.Add(bibitem);

                // Make correspondence for keys from groups to first bibitem key
                foreach (var item in g)
                {
                    keysOldNew[item.Key] = bibitem.Key;
                }

            }
            var biblistStr = String.Join("\r\n", filteredBibitems);
            // TODO: thebibliography has one params so we should remove not from bibenv.OpeningBlock.EndPos + 1
            // or we have to detect bibenv.OpeningBlock.EndPos more accurate taking into account one param
            text = text.Remove(bibenv.OpeningBlock.EndPos + 1,
                        bibenv.ClosingBlock.StartPos - bibenv.OpeningBlock.EndPos - 1)
                        .Insert(bibenv.OpeningBlock.EndPos + 1, biblistStr);
            
            return Tuple.Create(text, keysOldNew);
        }

        /// <summary>
        /// Extracts authors' lastnames and title as one string and 
        /// filters this string removing 
        /// 1) punctuation marks (replaces it for spaces)
        /// 2) words with length less than 4 chars (authors' initials, prepositions etc.)
        /// </summary>
        /// <param name="bibentry"></param>
        /// <returns></returns>
        public static string ExtractComparablePart(string bibentry)
        {
            string essence;
            if (bibentry.Contains("//"))
            {
                // this is an article
                essence = bibentry.Split(new[] { "//" }, StringSplitOptions.RemoveEmptyEntries)[0];
            }
            else
            {
                essence = bibentry.Substring(0, 60);
            }

            essence = essence.Replace('.', ' ').Replace(',', ' ').Replace("\n", "").Replace("\r", "")
                .Replace('~',' ').Replace('{', ' ').Replace('}', ' ');
            var words = essence.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 3);
            essence = String.Join(" ", words).Trim().ToLower();
            return essence;
        }

        public static string ArrangeCites(string text)
        {
            var cites = Utils.GetCites(text);
            var bibenv = Utils.FindEnv(text, "thebibliography");
            var bibitems = Utils.GetBibitems(text, bibenv);
            //cites.ForEach(c => Console.WriteLine(c));

            // to change ref numbers we rename them into some temp refs
            //foreach (var cite in cites)
            //{
            //    cite.Keys = cite.Keys.Select(k => "a" + k).ToList();
            //}
            //foreach (var bibitem in bibitems)
            //{
            //    bibitem.Key = "a" + bibitem.Key;
            //}


            var keys = cites.SelectMany(c => c.Keys).ToList();

            // remove bibitems that have no refs from text
            bibitems = bibitems.Where(b => keys.Contains(b.Key)).ToList();
            //keys.ForEach(c => Console.WriteLine(c));

            // numeration in appearence order
            var newKeyFor = new Dictionary<string, int>();
            var seenBefore = new HashSet<string>();
            int num = 1;
            foreach (var key in keys)
            {
                if (seenBefore.Contains(key))
                    continue;
                newKeyFor[key] = num;
                ++num;
                seenBefore.Add(key);
            }

            //foreach (var pair in newKeyFor.OrderBy(p => p.Value))
            //{
            //    Console.WriteLine(pair);
            //}

            foreach (var cite in cites)
            {
                cite.Keys = cite.Keys.Select(k => newKeyFor[k].ToString()).ToList();
            }

            foreach (var bibitem in bibitems)
            {
                bibitem.Key = newKeyFor[bibitem.Key].ToString();
            }

            var sb = new StringBuilder(text);

            // Clear bib environment content and place bibitems in ordered way
            sb.Remove(bibenv.OpeningBlock.EndPos + 1, bibenv.ClosingBlock.StartPos - bibenv.OpeningBlock.EndPos - 1);

            var sbBibInner = new StringBuilder();
            foreach (var item in bibitems.OrderBy(b => int.Parse(b.Key)))
            {
                sbBibInner.AppendLine(item.ToString());
            }

            sb.Insert(bibenv.OpeningBlock.EndPos + 1, sbBibInner.ToString());

            // change cites
            foreach (var cite in cites.OrderByDescending(c => c.Block.StartPos))
            {
                Utils.RemoveBlock(sb, cite.Block);
                sb.Insert(cite.Block.StartPos, cite.ToString());
            }

            return sb.ToString();
        }
    }
}
