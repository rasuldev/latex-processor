using System;
using System.Collections;
using System.Collections.Generic;
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
            while ((pos = text.IndexOf(@"\begin", pos+1, StringComparison.Ordinal)) > -1)
            {
                if (IsComment(text,pos))
                    continue;
                var paramsInfo = HarvestParams(text, pos, 1);
                var bItem = new EnvironmentBound(pos, paramsInfo.EndPos, EnvironmentBound.Types.Begin, paramsInfo.ParamsList[0]);
                envBoundItems.Add(bItem);
            }

            pos = -1;
            while ((pos = text.IndexOf(@"\end", pos+1, StringComparison.Ordinal)) > -1)
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
                        envStartBound.Start, text.Substring(envStartBound.Start,envStartBound.End-envStartBound.Start+1),
                        envBound.Start, text.Substring(envBound.Start,envBound.End-envBound.Start+1), envBound.Name);
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
            var matches = Regex.Matches(text, @"\\label(?:.|\r?\n)*?\{(.+?)\}");
            foreach (Match match in matches.OfType<Match>())
            {
                if (IsComment(text, match.Index))
                    continue;
                var label = new Label(match.Index, match.Value);
                label.Name = match.Groups[1].Value;
                label.EnvironmentName = GetEnvironmentName(text, match.Index).Content;
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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="format">#name#</param>
        public static void RenameLabels(ref StringBuilder sb, string format)
        {
            var labels = Utils.GetLabels(sb.ToString()).Where(l =>
                l.EnvironmentName != "lemma" && l.EnvironmentName != "lemmaA" &&
                l.EnvironmentName != "theorem" && l.EnvironmentName != "theoremA" &&
                l.EnvironmentName != "enumerate").ToList();

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

        public class ParamsInfo
        {
            public IList<string> ParamsList { get; set; }
            public int EndPos { get; set; }
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