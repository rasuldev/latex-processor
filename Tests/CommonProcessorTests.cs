using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using TextProcessor;
using System.Text.RegularExpressions;
using TextProcessor.Latex;

namespace Tests
{
    [TestFixture]
    public class CommonProcessorTests
    {
        [Test]
        public void MergeBibitems2Test()
        {
            var source =
@"
\begin{thebibliography}{1}
\bibitem{Haar-Tcheb-Shar15}
 И.И. Шарапудинов. Аппроксимативные свойства смешанных рядов по полиномам Лежандра на классах $W^r$ // Математический сборник. 2006. Том 197. Вып. 3. С. 135–154.

\bibitem{meixner-13}
 Шарапудинов~И.~И. Аппроксимативные свойства смешанных рядов по полиномам Лежандра на классах $W^r$ // Математический сборник, 2006, т. 197, №~3, C.~135-154. DOI: 10.4213/sm1539

\end{thebibliography}
";

            var result = CommonProcessor.MergeBibitems(source);
        }

        [Test]
        public void MergeBibitemsTest()
        {
            var source =
@"
\begin{thebibliography}{1} %% здесь библиографический список
\bibitem{Haar-Tcheb-Shar11} И.И. Шарапудинов. Приближение функций с переменной гладкостью суммами Фурье Лежандра // Математический сборник. 2000. Том 191. Вып. 5. С.143-160.

\bibitem{Haar-Tcheb-Shar13} И.И. Шарапудинов. Смешанные ряды по ортогональным полиномам // Махачкала. Издательство Дагестанского научного центра. 2004.


\bibitem{Haar-Tcheb-Shar15} И.И. Шарапудинов. Аппроксимативные свойства смешанных рядов по полиномам Лежандра на классах $W^r$ // Математический сборник. 2006. Том 197. Вып. 3. С. 135–154.

\bibitem{Haar-Tcheb-Shar16} И.И. Шарапудинов. Аппроксимативные свойства средних типа Валле-Пуссена частичных сумм смешанных рядов по полиномам Лежандра // Математические заметки. 2008. Том 84. Вып. 3. С. 452-471

\bibitem{Haar-Tcheb-Shar18} И.И. Шарапудинов, Т.И. Шарапудинов. Смешанные ряды по полиномам Якоби и Чебышева и их дискретизация // Математические заметки. 2010. Том 88. Вып. 1. С. 116-147.

\bibitem{sob-jac-discrete-Shar11} И.И. Шарапудинов, Приближение функций с переменной гладкостью суммами Фурье Лежандра // Математический сборник, 2000, т. 191, вып. 5, стр. 143-160.

\bibitem{sob-jac-discrete-Shar15} И.И. Шарапудинов, Аппроксимативные свойства смешанных рядов по полиномам Лежандра на классах $W^r$ // Математический сборник, 2006, т. 197, вып. 3, стр. 135–154.
\end{thebibliography}
";
            var result = CommonProcessor.MergeBibitems(source);
            Assert.AreEqual(5,result.Item1.Split(new [] { @"\bibitem"}, StringSplitOptions.None).Length-1);
            var keys = result.Item2;
            Assert.AreEqual(keys["sob-jac-discrete-Shar15"], keys["Haar-Tcheb-Shar15"]);
            Assert.AreEqual(keys["sob-jac-discrete-Shar11"], keys["Haar-Tcheb-Shar11"]);
        }

        [Test]
        public void MergeBibitemsAndReplaceCitesTest()
        {
            var source1 =
@"
\begin{thebibliography}{1} %% здесь библиографический список
\bibitem{Haar-Tcheb-Shar11} И.И. Шарапудинов. Приближение функций с переменной гладкостью суммами Фурье Лежандра // Математический сборник. 2000. Том 191. Вып. 5. С.143-160.

\bibitem{Haar-Tcheb-Shar13} И.И. Шарапудинов. Смешанные ряды по ортогональным полиномам // Махачкала. Издательство Дагестанского научного центра. 2004.


\bibitem{Haar-Tcheb-Shar15} И.И. Шарапудинов. Аппроксимативные свойства смешанных рядов по полиномам Лежандра на классах $W^r$ // Математический сборник. 2006. Том 197. Вып. 3. С. 135–154.

\bibitem{Haar-Tcheb-Shar16} И.И. Шарапудинов. Аппроксимативные свойства средних типа Валле-Пуссена частичных сумм смешанных рядов по полиномам Лежандра // Математические заметки. 2008. Том 84. Вып. 3. С. 452-471

\bibitem{Haar-Tcheb-Shar18} И.И. Шарапудинов, Т.И. Шарапудинов. Смешанные ряды по полиномам Якоби и Чебышева и их дискретизация // Математические заметки. 2010. Том 88. Вып. 1. С. 116-147.

\bibitem{sob-jac-discrete-Shar11} И.И. Шарапудинов, Приближение функций с переменной гладкостью суммами Фурье Лежандра // Математический сборник, 2000, т. 191, вып. 5, стр. 143-160.

\bibitem{sob-jac-discrete-Shar15} И.И. Шарапудинов, Аппроксимативные свойства смешанных рядов по полиномам Лежандра на классах $W^r$ // Математический сборник, 2006, т. 197, вып. 3, стр. 135–154.
\end{thebibliography}
";
            var source2 =
@"
Данный результат был получен в работе \cite{Haar-Tcheb-Shar11,sob-jac-discrete-Shar15}. 
Кроме того, в \cite{sob-jac-discrete-Shar11} что-то сделано. Еще можно посмотреть \cite{Haar-Tcheb-Shar18}.
";
            var result = CommonProcessor.MergeBibitemsAndReplaceCites(new List<string>() {source1, source2});
            Assert.That(!result[1].Contains("sob-jac-discrete-Shar11") ||
                        !result[1].Contains("Haar-Tcheb-Shar11"));

            Assert.That(result[1].Contains("Haar-Tcheb-Shar18"));

        }

        [Test]
        public void ProcessRefsTest()
        {
            var sb = new StringBuilder("из теорем 1 и 3 выводится теорема 4, поэтому");
            Func<string, string> labelFor = num => "kad-ito:" + num;
            var refs = new[] { "1", "3", "4" };
            CommonProcessor.ProcessRefs(sb, "теорем", refs, labelFor);
            Assert.AreEqual(@"из теорем \ref{kad-ito:1} и \ref{kad-ito:3} выводится теорема \ref{kad-ito:4}, поэтому", sb.ToString());
        }

        [Test]
        public void BlankLinesRemoveTest()
        {
            var source =
@"1) eсли $t \in G_1=[0,\frac3{\theta_n}]$,  то
$$
l ^\alpha_{ r,n}(t) \leq c(\alpha, r)[\ln(n + 1)+n^{\alpha-r}];

\eqno(75)
$$

";
            var expected =
@"1) eсли $t \in G_1=[0,\frac3{\theta_n}]$,  то
$$
l ^\alpha_{ r,n}(t) \leq c(\alpha, r)[\ln(n + 1)+n^{\alpha-r}];
\eqno(75)
$$

";
            var cleaned = Regex.Replace(source, @"(?<=\$\$.*?)(\r?\n){2,}(?=.*?\$\$)", "\r\n");
            Assert.AreEqual(expected, cleaned);
        }

        [Test]
        public void ArrangeCitesTest()
        {
            var source = @"
В статье \cite{bib1} рассмотрены вопросы... Кроме того, в \cite{bib2} и \cite{bib3,bib4}...
\begin{thebibliography}{1} %% здесь библиографический список
\bibitem{bib1} И.И. Шарапудинов. Приближение функций с переменной гладкостью суммами Фурье Лежандра // Математический сборник. 2000. Том 191. Вып. 5. С.143-160.

\bibitem{bib4} И.И. Шарапудинов. Смешанные ряды по ортогональным полиномам // Махачкала. Издательство Дагестанского научного центра. 2004.


\bibitem{bib3} И.И. Шарапудинов. Аппроксимативные свойства смешанных рядов по полиномам Лежандра на классах $W^r$ // Математический сборник. 2006. Том 197. Вып. 3. С. 135–154.

\bibitem{bib2} И.И. Шарапудинов. Аппроксимативные свойства средних типа Валле-Пуссена частичных сумм смешанных рядов по полиномам Лежандра // Математические заметки. 2008. Том 84. Вып. 3. С. 452-471
\end{thebibliography}
";
            var (bibInd, mod) = CommonProcessor.ArrangeCites(source);
            Assert.AreEqual(0, bibInd);
            var bibitems = Utils.GetBibitems(mod, Utils.FindEnv(mod, "thebibliography"));
            Assert.AreEqual(4, bibitems.Count);
            for (int i = 0; i < bibitems.Count; i++)
            {
                Assert.AreEqual("bib"+(i+1), bibitems[i].Key);
            }

        }
    }
}