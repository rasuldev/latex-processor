using System;
using System.Collections.Generic;

namespace TextProcessor.Latex
{
    public class LatexPreProcessor
    {
        private string text;
        public LatexPreProcessor(string text)
        {
            this.text = text;
        }

        public string GetText()
        {
            return text;
        }

        public void MakeMacrosesInlined()
        {
            MakeMacrosesInlined(new String[]{@"\newcommand{\norm}[1]{\|#1\|_{p(\cdot),w}}"});
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="macrosesDefs"></param>
        private void MakeMacrosesInlined(IEnumerable<string> macrosesDefs)
        {

        }


        //public void RenameLabels(string )
        //{
            
        //}


    }
}