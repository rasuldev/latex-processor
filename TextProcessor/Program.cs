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
            MergeBib(new []
            {
                @"d:\Dropbox\INFO_BASE\000 Делопроизводство\000 НОР\Планы и отчеты ДНЦ\Отчёты\2016\reportnir2016\biblios\biblio2.tex",
                @"d:\Dropbox\INFO_BASE\000 Делопроизводство\000 НОР\Планы и отчеты ДНЦ\Отчёты\2016\reportnir2016\chapters\intros\intro2.tex",
                @"d:\Dropbox\INFO_BASE\000 Делопроизводство\000 НОР\Планы и отчеты ДНЦ\Отчёты\2016\reportnir2016\chapters\chapter2\AGG12.tex",
                @"d:\Dropbox\INFO_BASE\000 Делопроизводство\000 НОР\Планы и отчеты ДНЦ\Отчёты\2016\reportnir2016\chapters\chapter2\haar-tcheb.tex",
                @"d:\Dropbox\INFO_BASE\000 Делопроизводство\000 НОР\Планы и отчеты ДНЦ\Отчёты\2016\reportnir2016\chapters\chapter2\meixner.tex",
                @"d:\Dropbox\INFO_BASE\000 Делопроизводство\000 НОР\Планы и отчеты ДНЦ\Отчёты\2016\reportnir2016\chapters\chapter2\ramazanov.tex",
                @"d:\Dropbox\INFO_BASE\000 Делопроизводство\000 НОР\Планы и отчеты ДНЦ\Отчёты\2016\reportnir2016\chapters\chapter2\SMS12.tex",
                @"d:\Dropbox\INFO_BASE\000 Делопроизводство\000 НОР\Планы и отчеты ДНЦ\Отчёты\2016\reportnir2016\chapters\chapter2\SMS13.tex",
                @"d:\Dropbox\INFO_BASE\000 Делопроизводство\000 НОР\Планы и отчеты ДНЦ\Отчёты\2016\reportnir2016\chapters\chapter2\sob-jac-discrete.tex",
                @"d:\Dropbox\INFO_BASE\000 Делопроизводство\000 НОР\Планы и отчеты ДНЦ\Отчёты\2016\reportnir2016\chapters\chapter2\sob-lag-sb.tex",
                @"d:\Dropbox\INFO_BASE\000 Делопроизводство\000 НОР\Планы и отчеты ДНЦ\Отчёты\2016\reportnir2016\chapters\chapter2\sob-lag-smj.tex",
                @"d:\Dropbox\INFO_BASE\000 Делопроизводство\000 НОР\Планы и отчеты ДНЦ\Отчёты\2016\reportnir2016\chapters\chapter2\sob-leg.tex",
                @"d:\Dropbox\INFO_BASE\000 Делопроизводство\000 НОР\Планы и отчеты ДНЦ\Отчёты\2016\reportnir2016\chapters\chapter2\sob-tcheb-difference.tex"
            });


            //Process2(@"d:\Dropbox\INFO_BASE\DOCS\000 DOC SRW\Rasul\Статьи\Скорость сходимости сумм Фурье-Хаара в весовых пространствах Лебега\Оформление\Magomed-Kasumov.tex",
            //    @"d:\Dropbox\INFO_BASE\DOCS\000 DOC SRW\Rasul\Статьи\Скорость сходимости сумм Фурье-Хаара в весовых пространствах Лебега\Оформление\processing\Magomed-Kasumov.tex",
            //    Encoding.GetEncoding("windows-1251"));

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

        private static void MergeBib(string[] filenames, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = new UTF8Encoding();

            List<string> sources = filenames.Select(f => File.ReadAllText(f, encoding)).ToList();
            sources = CommonProcessor.MergeBibitemsAndReplaceCites(sources);
            for (int i = 0; i < sources.Count; i++)
            {
                File.WriteAllText(GetProcessedFilename(filenames[i]), sources[i]);
            }
        }



        private static void Process3(string sourceFilename, Encoding encoding = null)
        {
            string destinationFilename = Path.Combine(
                Path.GetDirectoryName(sourceFilename),
                Path.GetFileNameWithoutExtension(sourceFilename) + "_processed" +
                Path.GetExtension(sourceFilename));
            if (encoding == null)
                encoding = new UTF8Encoding();
            var sb = new StringBuilder(File.ReadAllText(sourceFilename, encoding));
            Utils.RenameLabels(ref sb, "chap1-mmg-#name#");
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
                Path.GetFileNameWithoutExtension(sourceFilename) + "_processed"+
                Path.GetExtension(sourceFilename));
            if (encoding == null)
                encoding = new UTF8Encoding();
            var source = File.ReadAllText(sourceFilename, encoding);
            var text = CommonProcessor.MakeEquationWithLabelsFromDollars(source,"eq");
            //var text = CommonProcessor.MakeDollarsFromEquationWithLabels(source);
            //var text = CommonProcessor.ArrangeCites(source);
            //var text = CommonProcessor.WrapInEnvironment(source, @"\\textbf{Замечание (\d*).*}", "%e", "замечани", "remark", n => "kad-ito:"+n);
            //var text = CommonProcessor.WrapInEnvironment(source, @"\\textbf{Определение ((\d|\.)*).*?}", "%e", "определени", "definition", n => "sirazh2:" + n);
            File.WriteAllText(destinationFilename, text, encoding);
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
