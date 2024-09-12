using System;
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
        private static string path = @"d:\Downloads\content\";
        private static readonly string[] Theme1Files = new[]
        {
            path+@"biblio.tex",
            path+@"chaps\akm.tex",
            path+@"chaps\ark.tex",
            path+@"chaps\bab.tex",
            path+@"chaps\grm.tex",
            path+@"chaps\kri.tex",
            path+@"chaps\mma.tex",
            path+@"chaps\mmg.tex",
            path+@"chaps\msr.tex",
            path+@"chaps\mzg.tex",
            path+@"chaps\rmk.tex",
            path+@"chaps\smm.tex",
            path+@"chaps\stn.tex"
        };

        private static readonly string[] Theme3Files = new[]
        {
            @"h:\Dropbox\INFO_BASE\000 Делопроизводство\000 НОР\Планы и отчеты ДНЦ\Отчёты\2017\reportnir2017\chapters\intros\intro3.tex",
            @"h:\Dropbox\INFO_BASE\000 Делопроизводство\000 НОР\Планы и отчеты ДНЦ\Отчёты\2017\reportnir2017\chapters\section3-smm.tex",
            @"h:\Dropbox\INFO_BASE\000 Делопроизводство\000 НОР\Планы и отчеты ДНЦ\Отчёты\2017\reportnir2017\chapters\section3-kri.tex",
            @"h:\Dropbox\INFO_BASE\000 Делопроизводство\000 НОР\Планы и отчеты ДНЦ\Отчёты\2017\reportnir2017\chapters\section3-ei.tex",
            @"h:\Dropbox\INFO_BASE\000 Делопроизводство\000 НОР\Планы и отчеты ДНЦ\Отчёты\2017\reportnir2017\chapters\section3-mzg.tex",
            @"h:\Dropbox\INFO_BASE\000 Делопроизводство\000 НОР\Планы и отчеты ДНЦ\Отчёты\2017\reportnir2017\biblios\biblio3.tex"
        };
        private static readonly string[] Theme4Files = new[]
        {
            @"h:\Dropbox\INFO_BASE\000 Делопроизводство\000 НОР\Планы и отчеты ДНЦ\Отчёты\2017\reportnir2017\chapters\intros\intro4.tex",
            @"h:\Dropbox\INFO_BASE\000 Делопроизводство\000 НОР\Планы и отчеты ДНЦ\Отчёты\2017\reportnir2017\chapters\section4-valle.tex",
            @"h:\Dropbox\INFO_BASE\000 Делопроизводство\000 НОР\Планы и отчеты ДНЦ\Отчёты\2017\reportnir2017\chapters\section-sob-ode.tex",
            @"h:\Dropbox\INFO_BASE\000 Делопроизводство\000 НОР\Планы и отчеты ДНЦ\Отчёты\2017\reportnir2017\chapters\section-sob-ode-numerical.tex",
            @"h:\Dropbox\INFO_BASE\000 Делопроизводство\000 НОР\Планы и отчеты ДНЦ\Отчёты\2017\reportnir2017\chapters\section4-akm.tex",
            @"h:\Dropbox\INFO_BASE\000 Делопроизводство\000 НОР\Планы и отчеты ДНЦ\Отчёты\2017\reportnir2017\biblios\biblio4.tex"
        };
        static void Main(string[] args)
        {
            //ProcessVladThesis(@"d:\Dropbox\INFO_BASE_EXT\000 DOC SRW\Rasul\Конференции\Владикавказ 2023\Magomed-KasumovMG.tex");
            var tex = @"e:\GoogleDriveR\Научная работа\Равномерная сходимость рядов Соболева-Якоби\Статья равн сходимость при положительных показателях\Ультрасферический случай\Оформление СМЖ\Magomed-KasumovMG_2024.tex";
            ProcessFileForSMZ(tex, Encoding.GetEncoding("windows-1251"));
            // TODO: skip commented lines
            //ArrangeBiblio(new []{ "path-to-tex-file" }, Encoding.GetEncoding("windows-1251"));

            //MergeBib(new[] { @"d:\Dropbox\INFO_BASE\000 Делопроизводство\001 Grants\РНФ 2022 2\Форма 4_en.tex" }, Encoding.GetEncoding("utf-8"));
            //ArrangeBiblio(new []{ @"d:\Dropbox\INFO_BASE\000 Делопроизводство\001 Grants\РНФ 2022 2\Форма 4_en.tex" }, Encoding.GetEncoding("utf-8"));
            //MergeBib(new[] { @"d:\Dropbox\INFO_BASE\000 Делопроизводство\001 Grants\РНФ 2022 2\Форма 4.tex" }, Encoding.GetEncoding("utf-8"));
            //ArrangeBiblio(new[] { @"e:\GoogleDriveR\Научная работа\Равномерная сходимость рядов Соболева-Якоби\Статья равн сходимость при положительных показателях\Заметки.tex" }, Encoding.GetEncoding("utf-8"));

            //ArrangeBiblio(new[] { @"e:\GoogleDrive\Научная работа\Равномерная сходимость рядов Соболева-Якоби\Статья равн сходимость при неположительных показателях\UniConvSob.tex" });
            //, Encoding.GetEncoding("windows-1251"));

            //ArticlePreProcessing(@"d:\Dropbox\INFO_BASE_EXT\000 DOC SRW\Rasul\Планы и отчеты\2021\ГОСТ\GOST.FullReport.tex", "mmg-", Encoding.GetEncoding("utf-8"));
            //ArrangeBiblio(Theme1Files);

            //RenameCitesAndBiblio(@"h:\Dropbox\INFO_BASE\000 Делопроизводство\000 НОР\Планы и отчеты ДНЦ\Отчёты\2017\reportnir2017\chapters\section-charlier.tex", "charlier-");


            //Process2(@"d:\Downloads\Саратов 04.2014\SharapudinovII_AknievGG.tex",
            //         @"d:\Downloads\Саратов 04.2014\SharapudinovII_AknievGG_p.tex",
            //    Encoding.GetEncoding("windows-1251"));

            //ProcessFile(
            //    @"h:\Dropbox\INFO_BASE_EXT\000 DOC SRW\ИИ\Повторные средние\2\VallePoussenMeans.tex",
            //    Encoding.GetEncoding("windows-1251"));

            //ProcessFile(@"D:\Downloads\GadzhimirzaevRM.tex", Encoding.GetEncoding("windows-1251"));

            //ConvertRbibToBib(@"d:\Dropbox\INFO_BASE_EXT\000 DOC SRW\Rasul\Отчеты\2019\bib.txt", Encoding.GetEncoding("windows-1251"));

            //MergeBib(new []{@"d:\Dropbox\INFO_BASE\000 Делопроизводство\001 Grants\Проект РФФИ мол_а 2017\2018\Отчет\Форма503 - пункты 3.1-3.5.tex"}, Encoding.GetEncoding("windows-1251"));
            //ArrangeBiblio(new[] { @"d:\Dropbox\INFO_BASE\000 Делопроизводство\001 Grants\Проект РФФИ мол_а 2017\2018\Отчет\Форма503 - пункты 3.1-3.5.tex" }, Encoding.GetEncoding("windows-1251"));

            //ProcessFile(@"d:\Dropbox\INFO_BASE\DOCS\000 DOC SRW\Tadg\Shakh-Emirov\Ограниченность операторов свертки main — копия.tex", @"D:\Dropbox\INFO_BASE\DOCS\000 DOC SRW\Tadg\Shakh-Emirov\Ограниченность операторов свертки mainEq.tex", Encoding.GetEncoding("windows-1251"));

            //return;

            // Choose encoding of your file: default is UTF-8
            // var encoding = Encoding.GetEncoding("windows-1251");
            //var encoding = Encoding.GetEncoding("utf-8");
            //ProcessFile(@"..\..\test.txt", @"..\..\test_processed.txt",encoding);
            //ProcessFile(@"..\..\test_processed.txt", @"..\..\test2.txt", encoding);

            //var keys = GetBibitemKeysInRange(@"h:\Dropbox\Private\Отчет2018\RepNIR2018\T2\content\biblio.tex",
            //    "Shar11", "SharSMJ2017");
        }

        private static string GetBibitemKeysInRange(string fileWithBibEnv, string startKey, string endKey, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = new UTF8Encoding();
            var content = File.ReadAllText(fileWithBibEnv, encoding);
            var bibitems = Utils.GetBibitemsRange(content, startKey, endKey);
            return string.Join(",", bibitems.Select(b => b.Key));
        }

        private static void ArrangeBiblio(string[] filenames, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = new UTF8Encoding();
            var sources = filenames.Select(f => File.ReadAllText(f, encoding)).ToArray();
            var (bibInd, mod) = CommonProcessor.ArrangeCites(sources);
            File.Copy(filenames[bibInd], GetBakFilename(filenames[bibInd]));
            File.WriteAllText(filenames[bibInd], mod, encoding);
        }
        private static void ArticlePreProcessing(string filename, string prefix, Encoding encoding = null)
        {
            // add prefixes to formulas
            if (encoding == null)
                encoding = new UTF8Encoding();
            
            var sb = new StringBuilder(File.ReadAllText(filename, encoding));
            Utils.RenameLabels(ref sb, $"{prefix}#name#");

            // add prefixes to cites and biblio
            Utils.RenameCites(ref sb, key => prefix + key);
            Utils.RenameBibitems(ref sb, key => prefix + key);
            Utils.RenameRBibitems(ref sb, key => prefix + key);

            // expand range cites (convert \cite{1} -- \cite{4} to \cite{1,2,3,4})
            // merge cites (convert \cite{1}, \cite{2} to \cite{1,2})
            // detect broken cites
            // detect and drop unused biblio

            File.Copy(filename, GetBakFilename(filename));
            File.WriteAllText(filename, sb.ToString(), encoding);
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

        private static void ProcessVladThesis(string sourceFilename, Encoding encoding = null)
        {
            File.Copy(sourceFilename, GetBakFilename(sourceFilename));
            if (encoding == null)
                encoding = Encoding.GetEncoding("windows-1251");
            var sb = new StringBuilder(File.ReadAllText(sourceFilename, encoding));

            // Macroses inlining
            //var macrosDef = @"\newcommand{\norm}[1]{\|#1\|_{p(\cdot),w}}";
            //sb.Replace(macrosDef, "");
            //var macros = new Macros(macrosDef);
            //macros.ApplyMacros(ref sb);

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


            File.WriteAllText(sourceFilename, sb.ToString(), encoding);
        }



        /// <summary>
        /// Replaces $$-blocks that have \eqno{num} by 
        /// \begin{equation}\label{num} ... \end{equation}
        /// </summary>
        /// <param name="sourceFilename">File to be processed</param>
        /// <param name="destinationFilename">Result will be saved in this file</param>
        static void ProcessFileForSMZ(string sourceFilename, Encoding encoding = null)
        {
            File.Copy(sourceFilename, GetBakFilename(sourceFilename));
            if (encoding == null)
                encoding = new UTF8Encoding();
            var source = File.ReadAllText(sourceFilename, encoding);
            //var text = CommonProcessor.MakeEquationWithLabelsFromDollars(source, "eq");
            source = CommonProcessor.MakeDollarsFromEquationWithLabels(source);
            source = CommonProcessor.ArrangeCitesAndReplaceCitesWithNumbers(source);

            var sb = new StringBuilder(source);
            Utils.RenameEnvs(ref sb, "lemma", "\r\n\r\n" + @"\textbf{Лемма #counter#.}\textit{", "}\r\n\r\n");
            Utils.RenameEnvs(ref sb, "theorem", "\r\n\r\n" + @"\textbf{Теорема #counter#.}\textit{", "}\r\n\r\n");
            Utils.RenameEnvs(ref sb, "theoremA", "\r\n\r\n" + @"\textbf{Теорема #counter#.}\textit{", "}\r\n\r\n", i => new[] { "A", "B", "C", "D", "E" }[i - 1]);
            Utils.RenameEnvs(ref sb, "statement", "\r\n\r\n" + @"\textbf{Утверждение #counter#.}\textit{", "}\r\n\r\n");

            
            //var text = CommonProcessor.ArrangeCites(source);
            //var text = CommonProcessor.WrapInEnvironment(source, @"\\textbf{Замечание (\d*).*}", "%e", "замечани", "remark", n => "kad-ito:"+n);
            //var text = CommonProcessor.WrapInEnvironment(source, @"\\textbf{Определение ((\d|\.)*).*?}", "%e", "определени", "definition", n => "sirazh2:" + n);
            File.WriteAllText(sourceFilename, sb.ToString(), encoding);
        }

        private static void ConvertRbibToBib(string filename, Encoding encoding = null)
        {
            // add prefixes to formulas
            if (encoding == null)
                encoding = new UTF8Encoding();

            var converted = CommonProcessor.ConvertRBibitemsToBibitems(File.ReadAllText(filename, encoding));

            File.Copy(filename, GetBakFilename(filename));
            File.WriteAllText(filename, converted, encoding);
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
