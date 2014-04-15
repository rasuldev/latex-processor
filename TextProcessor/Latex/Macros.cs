using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TextProcessor.Latex
{
    public class Macros
    {
        public string Signature { get; private set; }
        private string body;
        public int ParamsCount { get; private set; }
        public Macros(string macrosDef)
        {
            // TODO: add check for \newcommand
            //@"\newcommand{\norm}[1]{\|#1\|_{p(\cdot),w}}"
            var commandParams = Utils.HarvestParams(macrosDef, 0, 2);
            Signature = commandParams.ParamsList[0];
            body = commandParams.ParamsList[1];

            ParamsCount = Regex.Matches(macrosDef, @"#\d").OfType<Match>().Count();
        }

        public void ApplyMacros(ref StringBuilder text)
        {
            var str = text.ToString();
            var matches = Regex.Matches(str, Regex.Escape(Signature));
            foreach (Match match in matches.OfType<Match>().OrderByDescending(m => m.Index))
            {
                var paramsList = Utils.HarvestParams(str, match.Index, ParamsCount);

                // remove macros invoking text
                text.Remove(match.Index, paramsList.EndPos - match.Index + 1);
                // place macros output
                text.Insert(match.Index, GetOutput(paramsList.ParamsList.ToArray()));
            }
        }

        public string GetOutput(params string[] paramsList)
        {
            string result = body;
            for (int i = 1; i <= ParamsCount; i++)
            {
                result = result.Replace("#" + i, paramsList[i - 1]);
            }
            return result;
        }


        
    }
}