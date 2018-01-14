﻿using System;
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
            // TODO: skip commented lines
            MergeBib(new[]
            {
                @"h:\Dropbox\INFO_BASE\000 Делопроизводство\000 НОР\Планы и отчеты ДНЦ\Отчёты\2017\reportnir2017\chapters\section2-common.tex",
                @"h:\Dropbox\INFO_BASE\000 Делопроизводство\000 НОР\Планы и отчеты ДНЦ\Отчёты\2017\reportnir2017\chapters\section2-sob.tex",
                @"h:\Dropbox\INFO_BASE\000 Делопроизводство\000 НОР\Планы и отчеты ДНЦ\Отчёты\2017\reportnir2017\chapters\section2-sobleg.tex",
                @"h:\Dropbox\INFO_BASE\000 Делопроизводство\000 НОР\Планы и отчеты ДНЦ\Отчёты\2017\reportnir2017\chapters\section2-laplas.tex",
                @"h:\Dropbox\INFO_BASE\000 Делопроизводство\000 НОР\Планы и отчеты ДНЦ\Отчёты\2017\reportnir2017\chapters\section2-equ102.tex",
                @"h:\Dropbox\INFO_BASE\000 Делопроизводство\000 НОР\Планы и отчеты ДНЦ\Отчёты\2017\reportnir2017\chapters\section-ramis.tex",
                @"h:\Dropbox\INFO_BASE\000 Делопроизводство\000 НОР\Планы и отчеты ДНЦ\Отчёты\2017\reportnir2017\chapters\section-charlier.tex",
                @"h:\Dropbox\INFO_BASE\000 Делопроизводство\000 НОР\Планы и отчеты ДНЦ\Отчёты\2017\reportnir2017\chapters\intros\intro2.tex",
                @"h:\Dropbox\INFO_BASE\000 Делопроизводство\000 НОР\Планы и отчеты ДНЦ\Отчёты\2017\reportnir2017\biblios\biblio2.tex"
            });

            ArticlePreProcessing();

            //RenameCitesAndBiblio(@"h:\Dropbox\INFO_BASE\000 Делопроизводство\000 НОР\Планы и отчеты ДНЦ\Отчёты\2017\reportnir2017\chapters\section-charlier.tex", "charlier-");


            //Process2(@"d:\Downloads\Саратов 04.2014\SharapudinovII_AknievGG.tex",
            //         @"d:\Downloads\Саратов 04.2014\SharapudinovII_AknievGG_p.tex",
            //    Encoding.GetEncoding("windows-1251"));

            //ProcessFile(
            //    @"g:\Dropbox\INFO_BASE\DOCS\000 делопроизводство\001 Grants\Проект РФФИ 2016\ПроектРФФИ_2016до2018_Form4.tex",
            //    Encoding.GetEncoding("windows-1251"));

            //ProcessFile(
            //    @"d:\Dropbox\~INFO_BASE_EXT\000 DOC SRW\Rasul\Статьи\Специальные ряды sigma rr\article.tex", Encoding.GetEncoding("windows-1251"));

            //ProcessFile(@"d:\Dropbox\INFO_BASE\DOCS\000 DOC SRW\Tadg\Shakh-Emirov\Ограниченность операторов свертки main — копия.tex", @"D:\Dropbox\INFO_BASE\DOCS\000 DOC SRW\Tadg\Shakh-Emirov\Ограниченность операторов свертки mainEq.tex", Encoding.GetEncoding("windows-1251"));

            //return;

            // Choose encoding of your file: default is UTF-8
            // var encoding = Encoding.GetEncoding("windows-1251");
            //var encoding = Encoding.GetEncoding("utf-8");
            //ProcessFile(@"..\..\test.txt", @"..\..\test_processed.txt",encoding);
            //ProcessFile(@"..\..\test_processed.txt", @"..\..\test2.txt", encoding);
        }

        private static void ArticlePreProcessing()
        {
            // add prefixes to formulas
            // add prefixes to cites and biblio
            // expand range cites (convert \cite{1} -- \cite{4} to \cite{1,2,3,4})
            // merge cites (convert \cite{1}, \cite{2} to \cite{1,2})
            // detect broken cites
            // detect and drop unused biblio
        }

        private static void RenameCitesAndBiblio(string filename, string prefix, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = new UTF8Encoding();
            File.Copy(filename, GetBakFilename(filename));
            var sb = new StringBuilder(File.ReadAllText(filename, encoding));
            Utils.RenameCites(ref sb, key => prefix + key);
            Utils.RenameBibitems(ref sb, key => prefix + key);
            Utils.RenameRBibitems(ref sb, key => prefix + key);
            File.WriteAllText(filename, sb.ToString(), encoding);
        }

        private static void MergeBib(string[] filenames, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = new UTF8Encoding();

            List<string> sources = filenames.Select(f => File.ReadAllText(f, encoding)).ToList();
            sources = CommonProcessor.MergeBibitemsAndReplaceCites(sources);
            for (int i = 0; i < sources.Count; i++)
            {
                File.Copy(filenames[i], GetBakFilename(filenames[i]));
                File.WriteAllText(filenames[i], sources[i], encoding);
            }
        }



        private static void Process3(string sourceFilename, string prefix, Encoding encoding = null)
        {
            string destinationFilename = Path.Combine(
                Path.GetDirectoryName(sourceFilename),
                Path.GetFileNameWithoutExtension(sourceFilename) + "_processed" +
                Path.GetExtension(sourceFilename));
            if (encoding == null)
                encoding = new UTF8Encoding();
            var sb = new StringBuilder(File.ReadAllText(sourceFilename, encoding));
            Utils.RenameLabels(ref sb, $"{prefix}#name#");
            File.WriteAllText(destinationFilename, sb.ToString(), encoding);
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
            Utils.RenameLabels(ref sb, "mmg_#name#_#num#");

            // replace theorem environments
            Utils.RenameEnvs(ref sb, "lemma", "\r\n\r\n" + @"\textbf{Лемма #counter#.}\textit{", "}\r\n\r\n");
            Utils.RenameEnvs(ref sb, "theorem", "\r\n\r\n" + @"\textbf{Теорема #counter#.}\textit{", "}\r\n\r\n");
            Utils.RenameEnvs(ref sb, "theoremA", "\r\n\r\n" + @"\textbf{Теорема #counter#.}\textit{", "}\r\n\r\n", i => new[] { "A", "B", "C" }[i - 1]);

            // remove block "My notations"

            var lineStart = Utils.FindLine(sb.ToString(), "===My notations");
            var lineEnd = Utils.FindLine(sb.ToString(), "===/My notations");
            for (int i = lineEnd; i >= lineStart; i--)
            {
                Utils.RemoveLine(sb, i);
            }


            File.WriteAllText(destinationFilename, sb.ToString(), encoding);
        }



        /// <summary>
        /// Replaces $$-blocks that have \eqno{num} by 
        /// \begin{equation}\label{num} ... \end{equation}
        /// </summary>
        /// <param name="sourceFilename">File to be processed</param>
        /// <param name="destinationFilename">Result will be saved in this file</param>
        static void ProcessFile(string sourceFilename, Encoding encoding = null)
        {
            string destinationFilename = Path.Combine(
                Path.GetDirectoryName(sourceFilename),
                Path.GetFileNameWithoutExtension(sourceFilename) + "_processed" +
                Path.GetExtension(sourceFilename));
            if (encoding == null)
                encoding = new UTF8Encoding();
            var source = File.ReadAllText(sourceFilename, encoding);
            var text = CommonProcessor.MakeEquationWithLabelsFromDollars(source, "eq");
            //var text = CommonProcessor.MakeDollarsFromEquationWithLabels(source);
            //var text = CommonProcessor.ArrangeCites(source);
            //var text = CommonProcessor.WrapInEnvironment(source, @"\\textbf{Замечание (\d*).*}", "%e", "замечани", "remark", n => "kad-ito:"+n);
            //var text = CommonProcessor.WrapInEnvironment(source, @"\\textbf{Определение ((\d|\.)*).*?}", "%e", "определени", "definition", n => "sirazh2:" + n);
            File.WriteAllText(destinationFilename, text, encoding);
        }


        private static string GetBakFilename(string filename)
        {
            var i = 1;
            string bakName;
            while (File.Exists(bakName = filename + ".bak" + i))
            {
                ++i;
            }
            return bakName;
        }
        private static string GetProcessedFilename(string filename)
        {
            return Path.Combine(
                            Path.GetDirectoryName(filename),
                            Path.GetFileNameWithoutExtension(filename) + "_processed" +
                            Path.GetExtension(filename));
        }

    }
}
