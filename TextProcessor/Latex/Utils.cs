using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TextProcessor.Latex
{
    public class Utils
    {
        public static IList<Environment> GetEnvironments(string text)
        {
            // find all occurrences of \begin and \end 
            var envBoundItems = new List<EnvironmentBound>();
            int pos = -1;
            while ((pos = text.IndexOf(@"\begin", pos + 1, StringComparison.Ordinal)) > -1)
            {
                if (IsComment(text, pos))
                    continue;
                var paramsInfo = HarvestParams(text, pos, 1);
                var bItem = new EnvironmentBound(pos, paramsInfo.EndPos, EnvironmentBound.Types.Begin, paramsInfo.ParamsList[0]);
                envBoundItems.Add(bItem);
            }

            pos = -1;
            while ((pos = text.IndexOf(@"\end", pos + 1, StringComparison.Ordinal)) > -1)
            {
                if (IsComment(text, pos))
                    continue;
                var paramsInfo = HarvestParams(text, pos, 1);
                var bItem = new EnvironmentBound(pos, paramsInfo.EndPos, EnvironmentBound.Types.End, paramsInfo.ParamsList[0]);
                envBoundItems.Add(bItem);
            }

            // sort
            envBoundItems = envBoundItems.OrderBy(i => i.Start).ToList();

            // === extract environments ===
            var envs = new List<Environment>();
            var stacks = new Stack<EnvironmentBound>();
            foreach (var envBound in envBoundItems)
            {
                if (envBound.Type == EnvironmentBound.Types.Begin)
                {
                    stacks.Push(envBound);
                }
                else
                {
                    var envStartBound = stacks.Pop();
                    var env = new Environment(
                        envStartBound.Start, text.Substring(envStartBound.Start, envStartBound.End - envStartBound.Start + 1),
                        envBound.Start, text.Substring(envBound.Start, envBound.End - envBound.Start + 1), envBound.Name);
                    envs.Add(env);
                }
            }
            return envs;
        }



        ////public static int Find
        //public static int FindClosingTag(string text, int start, string openTag, string closeTag)
        //{
        //    text.IndexOf("sss");
        //}

        public static TextBlock GetEnvironmentName(string text, int pos)
        {
            // find enclosed \begin: 
            // we must take into account cases when environment contains other environments
            int envStartPos, envEndPos;
            Stack<int> envStack = new Stack<int>();
            while (true)
            {
                // on every iteration we find closest environment bound item (\begin or \end)
                envStartPos = text.LastIndexOf(@"\begin", pos, System.StringComparison.Ordinal);
                envEndPos = text.LastIndexOf(@"\end", pos, System.StringComparison.Ordinal);

                // case when label was not inside of the environment
                // for example, \section{Sec1}\label{sec}
                // TODO: Fix to properly handle labels outside the environments
                if (envStartPos == -1)
                    return null;

                if (envEndPos > envStartPos)
                {
                    // move to found bound item
                    pos = envEndPos;
                    // if it is a comment do nothing and go to the next iteration
                    if (IsComment(text, pos))
                        continue;

                    // if closest bound item was \end then it means that we found inner environment
                    // \b....\b...\e....<pos>...
                    envStack.Push(1);
                }
                else
                {
                    pos = envStartPos;
                    if (IsComment(text, pos))
                        continue;

                    if (envStack.Count == 0)
                        break;
                    envStack.Pop();
                }
            }

            var envNameStartPos = text.IndexOf('{', envStartPos);
            var envNameEndPos = text.IndexOf('}', envNameStartPos);
            return new TextBlock(envNameStartPos + 1,
                    text.Substring(envNameStartPos + 1, envNameEndPos - envNameStartPos - 1));
        }

        public static IList<Label> GetLabels(string text)
        {
            List<Label> labels = new List<Label>();
            var matches = Regex.Matches(text, @"\\label(?:\s|\r?\n)*?\{(.+?)\}");
            foreach (Match match in matches.OfType<Match>())
            {
                if (IsComment(text, match.Index))
                    continue;
                var label = new Label(match.Index, match.Value);
                label.Name = match.Groups[1].Value;
                label.EnvironmentName = GetEnvironmentName(text, match.Index)?.Content;
                labels.Add(label);
            }
            return labels;
        }

        public static IList<Eqref> GetEqrefs(string text)
        {
            List<Eqref> items = new List<Eqref>();
            var matches = Regex.Matches(text, @"\\eqref(?:.|\r?\n)*?\{(.+?)\}");
            foreach (Match match in matches.OfType<Match>())
            {
                var item = new Eqref(match.Index, match.Value);
                items.Add(item);
            }
            return items;
        }

        public static IList<Ref> GetRefs(string text)
        {
            List<Ref> items = new List<Ref>();
            var matches = Regex.Matches(text, @"\\ref(?:.|\r?\n)*?\{(.+?)\}");
            foreach (Match match in matches.OfType<Match>())
            {
                var item = new Ref(match.Index, match.Value);
                items.Add(item);
            }
            return items;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="format">#name#</param>
        public static void RenameLabels(ref StringBuilder sb, string format)
        {
            var labels = Utils.GetLabels(sb.ToString()).ToList();

            Dictionary<string, string> labelsOldNew = new Dictionary<string, string>();
            var eqNumber = labels.Count;
            foreach (var label in labels.OrderByDescending(l => l.Block.StartPos))
            {
                Console.WriteLine(label.Name + "; " + label.EnvironmentName);
                var newName = format.Replace("#name#", label.Name).Replace("#num#", eqNumber.ToString());
                labelsOldNew[label.Name] = newName;
                sb.Remove(label.Block.StartPos, label.Block.Length);
                sb.Insert(label.Block.StartPos, Label.GenerateMarkup(newName));
                --eqNumber;
            }

            // replaces labels in eqref
            var eqrefs = Utils.GetEqrefs(sb.ToString());
            foreach (var eqref in eqrefs.OrderByDescending(e => e.Block.StartPos))
            {

                if (labelsOldNew.ContainsKey(eqref.Value))
                {
                    sb.Remove(eqref.Block.StartPos, eqref.Block.Length);
                    sb.Insert(eqref.Block.StartPos, Eqref.GenerateMarkup(labelsOldNew[eqref.Value]));
                }
            }

            // replaces labels in refs
            var refs = Utils.GetRefs(sb.ToString());
            foreach (var reff in refs.OrderByDescending(e => e.Block.StartPos))
            {

                if (labelsOldNew.ContainsKey(reff.Value))
                {
                    sb.Remove(reff.Block.StartPos, reff.Block.Length);
                    sb.Insert(reff.Block.StartPos, Ref.GenerateMarkup(labelsOldNew[reff.Value]));
                }
            }


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="keyConverter">Converter that accepts current cite key as param and should return new key</param>
        public static void RenameCites(ref StringBuilder sb, Func<string, string> keyConverter)
        {
            var cites = GetCites(sb.ToString());
            foreach (var cite in cites.OrderByDescending(c => c.Block.StartPos))
            {
                var newKeys = cite.Keys.Select(keyConverter).ToList();
                sb.Remove(cite.Block.StartPos, cite.Block.Length);
                sb.Insert(cite.Block.StartPos, Cite.GenerateMarkup(newKeys));
            }

            Console.WriteLine($"Renamed {cites.Sum(c => c.Keys.Count)} keys in {cites.Count} cites");
        }

        /// <summary>
        /// All bibitems should be contained in \begin{thebibliography}\end{thebibliography} environment.
        /// Method will process only first.
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="keyConverter">Converter that accepts current cite key as param and should return new key</param>
        public static void RenameBibitems(ref StringBuilder sb, Func<string, string> keyConverter)
        {
            var bibenv = FindEnv(sb.ToString(), "thebibliography");
            if (bibenv == null)
                throw new Exception("thebibliography environment not found");
            RenameBibitems(ref sb, keyConverter, bibenv);
        }

        /// <summary>
        /// Processes all bibitems inside given bibenv
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="keyConverter"></param>
        /// <param name="bibenv"></param>
        public static void RenameBibitems(ref StringBuilder sb, Func<string, string> keyConverter, Environment bibenv)
        {
            var bibitems = GetBibitems(sb.ToString(), bibenv);
            foreach (var bibitem in bibitems.OrderByDescending(c => c.Block.StartPos))
            {
                sb.Remove(bibitem.Block.StartPos, bibitem.Block.Length);
                sb.Insert(bibitem.Block.StartPos, Bibitem.GenerateMarkup(keyConverter(bibitem.Key), bibitem.FullTitle));
            }
            Console.WriteLine($"Renamed {bibitems.Count} bibitems");
        }

        public static void RenameRBibitems(ref StringBuilder sb, Func<string, string> keyConverter)
        {
            int endpos = 0;
            var rbibitems = new List<Command>();
            Command command;
            while ((command = FindOneParamCommand(sb.ToString(), "RBibitem", endpos)) != null)
            {
                rbibitems.Add(command);
                endpos = command.Block.EndPos;
            }

            foreach (var rbibitem in rbibitems.OrderByDescending(r => r.Block.StartPos))
            {
                sb.Remove(rbibitem.Block.StartPos, rbibitem.Block.Length);
                sb.Insert(rbibitem.Block.StartPos, $@"\RBibitem{{{keyConverter(rbibitem.Params.First())}}}");
            }
            Console.WriteLine($"Renamed {rbibitems.Count} RBibitems");
        }

        /// <summary>
        /// It doesn't work for nested environments
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="envName"></param>
        /// <param name="prefix">Use placeholder #counter# for number</param>
        /// <param name="postfix"></param>
        public static void RenameEnvs(ref StringBuilder sb, string envName, string prefix, string postfix, Func<int, string> counterFunc = null)
        {
            var envs = GetEnvironments(sb.ToString()).Where(e => e.Name == envName).OrderByDescending(e => e.OpeningBlock.StartPos);
            if (counterFunc == null)
                counterFunc = i => i.ToString();
            var counter = envs.Count();

            // Saves label numbers to replace refs 
            var labelNumbers = new Dictionary<string, string>();

            // Replaces all environments with name = envName
            foreach (var env in envs)
            {
                sb.Remove(env.ClosingBlock.StartPos, env.ClosingBlock.Length);
                sb.Insert(env.ClosingBlock.StartPos, postfix);

                var label = env.GetLabel(sb.ToString());
                if (label != null)
                {
                    labelNumbers[label.Name] = counterFunc(counter);
                    sb.Remove(label.Block.StartPos, label.Block.Length);
                }

                sb.Remove(env.OpeningBlock.StartPos, env.OpeningBlock.Length);
                sb.Insert(env.OpeningBlock.StartPos, prefix.Replace("#counter#", counterFunc(counter)));
                --counter;
            }

            // Processing refs
            var refs = GetRefs(sb.ToString()).OrderByDescending(r => r.Block.StartPos);
            foreach (var refItem in refs)
            {
                if (labelNumbers.ContainsKey(refItem.Value))
                {
                    RemoveBlock(sb, refItem.Block);
                    sb.Insert(refItem.Block.StartPos, labelNumbers[refItem.Value]);
                }
            }

        }

        /// <summary>
        /// Starting from startPos gathers paramsCount params in text. Param is a string, enclosed in braces:
        /// {val1}{val_{2}} -> [val1,val_{2}].
        /// </summary>
        /// <param name="text"></param>
        /// <param name="startPos"></param>
        /// <param name="paramsCount"></param>
        /// <returns></returns>
        public static ParamsInfo HarvestParams(string text, int startPos, int paramsCount)
        {
            List<string> paramsList = new List<string>(paramsCount);
            Stack<int> bracePosStack = new Stack<int>();

            int startBlock = 0, endBlock = 0;
            for (int j = startPos; j < text.Length; j++)
            {
                if (text[j] == '{')
                {
                    bracePosStack.Push(j);
                }
                else if (text[j] == '}')
                {
                    startBlock = bracePosStack.Pop();
                    if (bracePosStack.Count == 0)
                    {
                        endBlock = j;
                        var param = text.Substring(startBlock + 1, endBlock - startBlock - 1);
                        paramsList.Add(param);
                        if (paramsList.Count == paramsCount)
                        {
                            return new ParamsInfo() { ParamsList = paramsList, EndPos = endBlock };
                        }
                    }
                }
            }

            throw new Exception(String.Format("Wrong number of macros params (macros name: {0}; pos: {1}).", startPos));
        }

        public static bool IsComment(string text, int pos)
        {
            while (pos >= 0)
            {
                if (text[pos] == '%')
                    return true;
                if (text[pos] == '\n')
                    return false;
                --pos;
            }
            return false;
        }

        public static StringBuilder RemoveBlock(StringBuilder sb, TextBlock block)
        {
            return sb.Remove(block.StartPos, block.Length);
        }




        /*
        /// <summary>
        /// Removes line where symbol with pos number is located
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="pos"></param>
        public static void RemoveLine(StringBuilder sb, int pos)
        {
            string text = sb.ToString();
            var s = text.LastIndexOf('\n', pos);
            if (s == -1) s = 0;
            var e = text.IndexOfAny(new[] { '\r', '\n' }, pos);
            if (e == -1)
                e = text.Length - 1;

            sb.Remove(s, e - s + 1);
        }
        */

        public static void RemoveLine(StringBuilder sb, int lineNumber)
        {
            int c = 0, pos = -1;
            string text = sb.ToString();
            while (c < lineNumber && (pos = text.IndexOf('\n', pos + 1)) > -1)
                ++c;
            if (c < lineNumber)
                throw new Exception("No line with number " + lineNumber);
            int posEnd = text.IndexOf('\n', pos + 1);
            sb.Remove(pos + 1, posEnd - pos);
        }

        public static int FindLine(string text, int pos)
        {
            if (pos >= text.Length || pos < 0)
                throw new IndexOutOfRangeException("Parameter pos can't be greater than text length or less than 0");
            int c = 0, p = -1;

            while ((p = text.IndexOf('\n', p + 1)) > -1 && p < pos)
                ++c;

            return c;
        }
        public static int FindLine(string text, string search)
        {
            return FindLine(text, text.IndexOf(search));
        }

        /// <summary>
        /// Every symbol after startIndex will be tested by startSymbols regexp to find start of block. 
        /// After that every symbol will be tested by inBlockSymbols until it fails. 
        /// That position is considered as end of block. 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="startIndex"></param>
        /// <param name="startSymbols">Regular expression for possible start symbols</param>
        /// <param name="inBlockSymbols">Regular expression for possible symbols within block</param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TextBlock ExtractBlock(string text, int startIndex, string startSymbols, string inBlockSymbols, int count = 20)
        {
            var testedStr = text.Substring(startIndex, Math.Min(count, text.Length - startIndex));
            var match = Regex.Match(testedStr, startSymbols, RegexOptions.IgnoreCase);
            if (!match.Success)
                return null;
            int pos = match.Index;
            do
            {
                ++pos;
            } while (pos < testedStr.Length && Regex.IsMatch(testedStr[pos].ToString(), inBlockSymbols));
            if (pos == testedStr.Length)
                Console.WriteLine("Warning: possible cut of block");
            return new TextBlock(text, startIndex + match.Index, startIndex + pos - 1);
        }

        public static TextBlock ExtractRefsBlock(string text, int startIndex, int count = 20)
        {
            string startRegexp = " |~";
            string endRegexp = @" |\d|,|~|и|-";
            return ExtractBlock(text, startIndex, startRegexp, endRegexp, count);
        }

        public class ParamsInfo
        {
            public IList<string> ParamsList { get; set; }
            public int EndPos { get; set; }
        }

        public static Environment FindEnv(string text, string envName)
        {
            var start = FindOneParamCommandWithValue(text, "begin", envName);
            if (start == null) return null;
            var end = FindOneParamCommandWithValue(text, "end", envName);
            return new Environment(start, end, envName);
        }

        public static List<Bibitem> GetBibitems(string text, Environment bibEnv)
        {
            var bibitems = new List<Bibitem>();
            Command cmd;
            int start = bibEnv.OpeningBlock.EndPos + 1;
            while ((cmd = FindOneParamCommand(text, "bibitem", start)) != null)
            {
                if (cmd.Block.EndPos > bibEnv.ClosingBlock.StartPos)
                    break;

                //int endTitlePos = text.IndexOf(@"\bibitem", cmd.Block.EndPos + 1);
                //if (endTitlePos == -1)
                //    endTitlePos = text.IndexOf(@"\end", cmd.Block.EndPos + 1);
                int endTitlePos =
                    FindOneParamCommand(text, "bibitem", cmd.Block.EndPos + 1)?.Block.StartPos ??
                    FindOneParamCommand(text, "end", cmd.Block.EndPos + 1).Block.StartPos;
                string title = text.Substring(cmd.Block.EndPos + 1, endTitlePos - cmd.Block.EndPos - 1);
                bibitems.Add(new Bibitem(
                    new TextBlock(text, cmd.Block.StartPos, endTitlePos - 1), cmd.Params[0], title));

                start = endTitlePos;
                if (start > bibEnv.ClosingBlock.StartPos)
                    break;
            }
            return bibitems;

            //var envInnerText = text.Substring(bibEnv.OpeningBlock.EndPos + 1, bibEnv.ClosingBlock.StartPos - 1);
            //var bibitemsRaw = envInnerText.Split(new[] { @"\bibi" }, StringSplitOptions.None)
            //    .Where(b => b.StartsWith("tem"))
            //    .Select(b => @"\bibi" + b);
            //foreach (var item in bibitemsRaw)
            //{
            //    var info = HarvestParams(item, 1, 1);
            //    bibitems.Add(new Bibitem());
            //}
        }

        public static List<Cite> GetCites(string text)
        {
            var cites = new List<Cite>();
            Command cmd = FindOneParamCommand(text, "cite", 0);
            while (cmd != null)
            {
                cites.Add(new Cite(cmd.Block.StartPos, cmd.Block.Content));
                cmd = FindOneParamCommand(text, "cite", cmd.Block.EndPos + 1);
            }
            return cites;
        }

        public static Command FindOneParamCommand(string text, string commandName, int start)
        {
            var match = Regex.Match(text.Substring(start), $@"\\{commandName}[^{{]*?{{([^}}]*?)}}");
            // check if found command is commented out and skip if it is
            while (match.Success && IsComment(text, start + match.Index))
            {
                match = match.NextMatch();
            }

            if (!match.Success)
                return null;

            return new Command(start + match.Index, match.Value, 1);
        }

        public static TextBlock FindOneParamCommandWithValue(string text, string commandName, string paramValue)
        {
            string pattern = $@"\\{commandName}[^{{]*?{{[^}}]*?{paramValue}[^}}]*?}}";
            var match = Regex.Match(text, pattern);
            if (!match.Success)
                return null;
            var start = match.Index;
            var end = match.Index + match.Length - 1;
            return new TextBlock(text, start, end);
        }


        class EnvironmentBound
        {
            public EnvironmentBound(int start, int end, Types type, string name)
            {
                Start = start;
                End = end;
                Type = type;
                Name = name;
            }
            /// <summary>
            /// Start position of \begin{envname} or \end{envname} block
            /// </summary>
            public int Start { get; set; }
            /// <summary>
            /// End position of \begin{envname} or \end{envname} block
            /// </summary>
            public int End { get; set; }

            /// <summary>
            /// It can be "begin" or "end"
            /// </summary>
            public Types Type { get; set; }
            /// <summary>
            /// Contains the name of environment.
            /// For example, if it was \begin{equation} then Name would be "equation".
            /// </summary>
            public string Name { get; set; }

            public enum Types
            {
                Begin, End
            }
        }
    }
}