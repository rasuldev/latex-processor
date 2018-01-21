using System.Xml.Schema;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextProcessor.Latex;

namespace Tests
{
    [TestFixture]
    public class UtilsTests
    {
        [Test]
        public void GetEnvironmentsTest()
        {
            string text =
@"\begin{equation*}
\begin{env1}text\end{env1}
\begin{env2}
    text
    %\begin{envCommented}
    \begin{env3}
        text
    \end{env3}
    text
\end{env2}      
\|f\|_{p(\cdot),w} \le r_{p,q}^w \|f\|_{q(\cdot),w},
\end{equation*}";
            var actual = Utils.GetEnvironments(text).ToArray();
            var expected = new TextProcessor.Latex.Environment[]
            {
                new TextProcessor.Latex.Environment(19,@"\begin{env1}",35,@"\end{env1}",@"env1"),
                new TextProcessor.Latex.Environment(102,@"\begin{env3}",134,@"\end{env3}",@"env3"),
                new TextProcessor.Latex.Environment(47,@"\begin{env2}",156,@"\end{env2}",@"env2"),
                new TextProcessor.Latex.Environment(0,@"\begin{equation*}",228,@"\end{equation*}",@"equation*"),
            };
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GetEnvironmentTest()
        {
            string text = @"\begin{equation*}
                              \|f\|_{p(\cdot),w} \le r_{p,q}^w \|f\|_{q(\cdot),w},
                            \end{equation*}";
            string actual = Utils.GetEnvironmentName(text, 20).Content;
            string expected = "equation*";

            Assert.AreEqual(expected, actual);
        }


        [Test]
        public void GetEnvironmentWithInnerTest()
        {
            string text =
@"\begin{equation*}
\begin{env1}text\end{env1}
\begin{env2}
    text
    %\begin{envCommented}
    \begin{env3}
        text
    \end{env3}
    text
\end{env2}      
\|f\|_{p(\cdot),w} \le r_{p,q}^w \|f\|_{q(\cdot),w},
\end{equation*}";
            string actual = Utils.GetEnvironmentName(text, 181).Content;
            string expected = "equation*";
            Assert.AreEqual(expected, actual);

            actual = Utils.GetEnvironmentName(text, 151).Content;
            expected = "env2";
            Assert.AreEqual(expected, actual);

            actual = Utils.GetEnvironmentName(text, 125).Content;
            expected = "env3";
            Assert.AreEqual(expected, actual);

        }


        [Test]
        public void GetLabelsTest()
        {
            string text =
@"\begin{lemma}\label{lemmaDensity}
Множество непрерывных функций $C[0,1]$ всюду плотно в $L^{p(x)}_w$.
\end{lemma}

Пусть $f(x,t)$ -- измеримая функция, заданная на декартовом произведении $E_1 \times E_2$ множеств $E_1$ и $E_2$, на которых заданы конечные меры $\mu_1$ и $\mu_2$ соответственно. Тогда справедливо неравенство
\begin{equation}\label{normUnderInt}
  \Bigl\|\int\limits_{E_2}|f(\cdot,x)|\mu_2(dx)\Bigr\|_{p(\cdot),w}(E_1) \le
  r_p\int\limits_{E_2} \norm{f(\cdot,x)}(E_1)\mu_2(dx),
\end{equation}
Заметим также [6-7], что для функций $f \in L^{p(x)}_w$ при $w \in \mathcal{H}(p)$ имеет место неравенство
\begin{equation}\label{fL1Finite}
  \int\limits_E |f(x)|dx \le
  c(p,w) \cdot \|f\|_{p(\cdot),w}.
\end{equation}

В [6-7] были получены достаточные условия, при которых система Хаара образует базис в $L^{p(x)}_w$. Приведем соответствующую теорему из упомянутой статьи. Для этого сначала введем некоторые обозначения.
";
            var actual = Utils.GetLabels(text);
            var expected = new Label[]
            {
                new Label(13, @"\label{lemmaDensity}"){Name="lemmaDensity", EnvironmentName = "lemma"},
                new Label(346, @"\label{normUnderInt}"){Name="normUnderInt", EnvironmentName = "equation"},
                new Label(643, @"\label{fL1Finite}"){Name="fL1Finite", EnvironmentName = "equation"},
            };

            Assert.AreEqual(expected, actual);

        }

        [Test]
        public void GetEqrefsTest()
        {
            string text =
                @"Поскольку $f'(x) \in L^{p(x)}_w$ и $w(x) \in \mathcal{H}(p)$, то можно применить неравенство \eqref{fL1Finite}. Поэтому, используя условие \eqref{logHolderCond}  и неравенство \eqref{normLess1}, для одного из множителей под интегралом в последнем выражении цепочки соотношений \eqref{sobolevProofMain} получим";
            var eqrefs = Utils.GetEqrefs(text).ToArray();

            var expected = new Eqref[]
            {
                new Eqref(93,@"\eqref{fL1Finite}"),
                new Eqref(139,@"\eqref{logHolderCond}"),
                new Eqref(176,@"\eqref{normLess1}"),
                new Eqref(277,@"\eqref{sobolevProofMain}")
            };

            Assert.AreEqual(expected, eqrefs);
        }

        [Test]
        public void ReplaceEnvsTest()
        {
            string text =
@"Справедлива следующая лемма
\begin{lemma}\label{first}
AAA
\end{lemma}
Самостоятельный интерес представляет также
\begin{lemma}\label{second}
BBB
\end{lemma}
В дальнейшем нам также понадобится, которая вытекает из лемм \ref{first} и \ref{second}.
\begin{lemma}\label{third}
CCC
\end{lemma}
Леммы \ref{first} и \ref{third} используются при доказательстве теоремы.
\begin{theorem}
DDD
\end{theorem}";
            string expected =
@"Справедлива следующая лемма
\textbf{Лемма 1.}\textit{
AAA
}
Самостоятельный интерес представляет также
\textbf{Лемма 2.}\textit{
BBB
}
В дальнейшем нам также понадобится, которая вытекает из лемм 1 и 2.
\textbf{Лемма 3.}\textit{
CCC
}
Леммы 1 и 3 используются при доказательстве теоремы.
\begin{theorem}
DDD
\end{theorem}";
            var sb = new StringBuilder(text);
            Utils.RenameEnvs(ref sb, "lemma", @"\textbf{Лемма #counter#.}\textit{", "}");
            Assert.AreEqual(expected, sb.ToString());

        }

        [Test]
        public void ReplaceEnvsATest()
        {
            string text =
@"Справедлива следующая лемма
\begin{lemma}\label{first}
AAA
\end{lemma}
Самостоятельный интерес представляет также
\begin{lemma}\label{second}
BBB
\end{lemma}
В дальнейшем нам также понадобится, которая вытекает из лемм \ref{first} и \ref{second}.
\begin{lemma}\label{third}
CCC
\end{lemma}
Леммы \ref{first} и \ref{third} используются при доказательстве теоремы.
\begin{theorem}
DDD
\end{theorem}";
            string expected =
@"Справедлива следующая лемма
\textbf{Лемма A.}\textit{
AAA
}
Самостоятельный интерес представляет также
\textbf{Лемма B.}\textit{
BBB
}
В дальнейшем нам также понадобится, которая вытекает из лемм A и B.
\textbf{Лемма C.}\textit{
CCC
}
Леммы A и C используются при доказательстве теоремы.
\begin{theorem}
DDD
\end{theorem}";
            var sb = new StringBuilder(text);
            Utils.RenameEnvs(ref sb, "lemma", @"\textbf{Лемма #counter#.}\textit{", "}", i => new[] { "A", "B", "C" }[i - 1]);
            Assert.AreEqual(expected, sb.ToString());

        }

        [Test]
        public void RemoveLineTest()
        {
            string text =
@"1abc
2abc
3abc
4abc
5abc";
            var sb = new StringBuilder(text);
            Utils.RemoveLine(sb, 1);
            string expected =
@"1abc
3abc
4abc
5abc";
            Assert.AreEqual(expected, sb.ToString());
        }

        [Test]
        public void FindLineTest()
        {
            string text = "1abc\r\n2abc\r\n3abc\r\n4abc";
            int line = Utils.FindLine(text, 6);
            Assert.AreEqual(1, line);

            line = Utils.FindLine(text, 1);
            Assert.AreEqual(0, line);

            line = Utils.FindLine(text, 12);
            Assert.AreEqual(2, line);
        }

        [Test]
        public void ExtractBlockTest()
        {
            string text = "Используя теоремы 1, 2 и 5, мы можем получить";
            string startRegexp = " |~";
            string endRegexp = @" |\d|,|~|и|-";
            var tb = Utils.ExtractBlock(text, 15, startRegexp, endRegexp);
            Assert.AreEqual(tb.StartPos, 17);
            Assert.AreEqual(tb.EndPos, 27);
        }

        [Test]
        public void ExtractBlockTest2()
        {
            string text = "Используя теоремы~1, 2 и 5 - 8, мы можем получить";
            string startRegexp = " |~";
            string endRegexp = @" |\d|,|~|и|-";
            var tb = Utils.ExtractBlock(text, 15, startRegexp, endRegexp);
            Assert.AreEqual(tb.StartPos, 17);
            Assert.AreEqual(tb.EndPos, 31);
        }

        [Test]
        public void FindEnvTest()
        {
            string text =
                @"Some text
\begin{theorem}{12}\label{th}
Theorem statement

\end{theorem}#

Another text...
";
            var env = Utils.FindEnv(text, "theorem");
            Assert.AreEqual(text.IndexOf(@"\begin"),env.OpeningBlock.StartPos);
            Assert.AreEqual(text.IndexOf(@"{12}")-1, env.OpeningBlock.EndPos);
            Assert.AreEqual(text.IndexOf(@"\end"), env.ClosingBlock.StartPos);
            Assert.AreEqual(text.IndexOf(@"#")-1, env.ClosingBlock.EndPos);
        }

        [Test]
        public void GetBibitemsTest()
        {
            string text =
                @"
Text before...
\begin{thebibliography}{46}

\bibitem{1} Malvar H.S. Signal processing with lapped transforms. Artech House. Boston{ $\cdot$} London. 1992.

\bibitem{2}  Mukundan R., Ramakrishnan K.R. Moment functions in image analysis. Theory and Applications.World Scientific. Pablishing Co. Pte. Ltd. 1998.


\bibitem{3} Дедус Ф.Ф., Махортых С.А., Устинин М.Н., Дедус А.Ф. Обобщенный спектрально-аналитический метод обработки информационных массивов. Задачи анализа изображений и распознавания образов. -- М. ""Машиностроение"". 1999.

\bibitem{4}
            Фрейзер М. Введение в вэйвлеты в свете линейной алгебры.БИНОМ.Лаборатория знаний. --М. 2008.

\end{thebibliography}
Text after...
";

            var env = Utils.FindEnv(text, "thebibliography");
            var bibitems = Utils.GetBibitems(text, env);

            Assert.AreEqual(4, bibitems.Count);
            for (int i = 0; i < 3; i++)
            {
                Assert.That(bibitems[i].Key == (i+1).ToString());
            }

            Assert.AreEqual(
@" Malvar H.S. Signal processing with lapped transforms. Artech House. Boston{ $\cdot$} London. 1992.

", bibitems[0].FullTitle);

            Assert.AreEqual(
@"
            Фрейзер М. Введение в вэйвлеты в свете линейной алгебры.БИНОМ.Лаборатория знаний. --М. 2008.

", bibitems[3].FullTitle);

        }

        [Test]
        public void GetCitesTest()
        {
            var text =
                @"
\vspace{0.5cm}
{\bf 4.1. Цель и задачи фундаментального исследования}
\vspace{0.3cm}

Проект направлен на решение фундаментальной проблемы об исследовании новых методов теории приближения функций,
связанных с решением ряда актуальных современных задач, возникающих в таких областях, как обработка и сжатие временных рядов и изображений \cite{1,2,3,4}, приближенное решение систем нелинейных дифференциальных  и разностных уравнений численно-аналитическими методами \cite{5,6,7,8}, численное обращение преобразования Радона \cite{9,10},  идентификация линейных и нелинейных систем автоматического регулирования и управления \cite{11,12}  и других. (Числа в квадратных скобках означают ссылки на литературу, приведенную для удобства чтения в конце настоящего пункта.)

";
            var cites = Utils.GetCites(text);
            Assert.That(cites.Count == 4);

            Assert.AreEqual(4, cites[0].Keys.Count);
            Assert.AreEqual(4, cites[1].Keys.Count);
            Assert.AreEqual(2, cites[2].Keys.Count);
            Assert.AreEqual(2, cites[3].Keys.Count);

            CollectionAssert.AreEqual(new[] {"1","2","3","4"}, cites[0].Keys);
            CollectionAssert.AreEqual(new[] { "11", "12" }, cites[3].Keys);

            Assert.AreEqual(text.IndexOf(@"\cite"),cites[0].Block.StartPos);

        }

        [Test]
        public void RenameCitesTest()
        {
            var sb = new StringBuilder(@"
\vspace{0.5cm}
{\bf 4.1. Цель и задачи фундаментального исследования}
\vspace{0.3cm}
Проект направлен на решение фундаментальной проблемы об исследовании новых методов теории приближения функций,
связанных с решением ряда актуальных современных задач, возникающих в таких областях, как обработка и сжатие 
временных рядов и изображений \cite{1,2,3,4}, приближенное решение систем нелинейных дифференциальных  и 
разностных уравнений численно-аналитическими методами \cite{5,6,7,8}, численное обращение преобразования 
Радона \cite{9,10},  идентификация линейных и нелинейных систем автоматического регулирования и управления 
\cite{11,12}, \cite{kolmogorov} и других. (Числа в квадратных скобках означают ссылки на литературу, 
приведенную для удобства чтения в конце настоящего пункта.)
");
            Utils.RenameCites(ref sb, key => $"prefix-{key}");
            var newText = sb.ToString();
            var cites = Utils.GetCites(newText);
            Assert.AreEqual(5, cites.Count);
            foreach (var cite in cites)
            {
                Assert.That(cite.Keys.All(k => k.StartsWith("prefix-")));
            }

            Assert.AreEqual(13, cites.Sum(c => c.Keys.Count));
        }

        [Test]
        public void RenameBibitemsTest()
        {
            var sb = new StringBuilder(@"
\begin{thebibliography}{999}



\bibitem{Shar1}
Iserles~A., Koch~P.~E., Norsett~S.~P., Sanz-Serna~J.~M. On polynomials orthogonal with respect to certain Sobolev inner products~// J. Approx. Theory. 1991. Vol.~65. Iss.~2. Pp.~151--175. DOI: 10.1016/0021-9045(91)90100-O.
\bibitem{Shar2}
Marcellan~F., Alfaro~M., Rezola~M.~L. Orthogonal polynomials on Sobolev spaces: old and new directions~// J. Comput. Appl. Math. 1993. Vol.~48. Iss.~1--2. Pp.~113--131. DOI: 10.1016/0377-0427(93)90318-6.
\bibitem{Shar3}
Meijer~H.~G. Laguerre polynomials generalized to a certain discrete Sobolev inner product space~// J. Approx. Theory. 1993. Vol.~73. Iss.~1. Pp.~1--16. DOI: 10.1006/jath.1993.1029.
\bibitem{Shar4}
Kwon~K.~H., Littlejohn~L.~L. The orthogonality of the Laguerre polynomials $\{L_n^{(-k)}(x)\}$ for positive integers $k$~// Ann. Numer. Anal. 1995. Vol.~2. Pp.~289--303.
\bibitem{Shar5}
Kwon~K.~H., Littlejohn~L.~L. Sobolev orthogonal polynomials and second-order differential equations~// Rocky Mountain J. Math. 1998. Vol.~28. Pp.~547--594. DOI: 10.1216/rmjm/1181071786
\bibitem{Shar6}
Marcellan~F., Xu~Y. On Sobolev orthogonal polynomials~// arXiv:1403.6249v1 [math.CA]. 25~Mar~2014. 40~p.
\bibitem{Shar7}
Шарапудинов~И.~И., Гаджиева~З.~Д. Полиномы, ортогональные по Соболеву, порожденные многочленами Мейкснера~// Изв. Сарат. ун-та. Нов. cер. Сер. Математика. Механика. Информатика. 2016. Т.~16. Вып.~3. С.~310--321. DOI: 10.18500/1816-9791-2016-16-3-310-321.
\bibitem{Shar8}
Шарапудинов~И.~И. Смешанные ряды по ортогональным полиномам. Махачкалаю. Изд-во ДНЦ РАН. 2004.
\bibitem{Shar9}
Шарапудинов~И.~И. Многочлены, ортогональные на сетках. Махачкала. Изд-во Даг. гос. пед. ун-та. 1997.	
\bibitem{Shar10}
Бейтмен~Г., Эрдейи~А. Высшие трансцендентные функции. Том 2. Москва. Наука. 1974.


\bibitem{Shar11}
Ширяев~А.~Н. Вероятность-1. Москва. Изд-во МЦНМО. 2007.

\end{thebibliography}

");
            Utils.RenameBibitems(ref sb, key => $"prefix-{key}");
            var newText = sb.ToString();
            var bibitems = Utils.GetBibitems(newText, Utils.FindEnv(newText, "thebibliography"));
            Assert.AreEqual(11, bibitems.Count);
            Assert.That(bibitems.All(b => b.Key.StartsWith("prefix-")));
        }

        [Test]
        public void RenameRBibitemsTest()
        {
            var sb = new StringBuilder(@"

\RBibitem{TEL}
\by С.\, А. Теляковский
\paper Две теоремы о приближении функций алгебраическими многочленами
\inbook Математический сборник
\vol 70
\issue 2
\yr 1966
\pages 252 -- 265


\RBibitem{GOP}
\by И.\, З. Гопенгауз
\paper К теореме А. Ф. Тимана о приближении функций многочленами на
конечном отрезке
\inbook Математические  заметки
\vol 1
\issue 2
\yr 1967
\pages 163 -- 172





\RBibitem{OSK}
\by К.\, И. Осколков
\paper К неравенству Лебега в равномерной метрике и на множестве полной меры
\inbook Математические  заметки
\vol 18
\issue 4
\yr 1975
\pages 515 -- 526

\RBibitem{sharap1}
\by I.\,I. Sharapudinov
\paper On the best approximation and polinomial of the least quadratic deviation
\inbook Analysis Mathematica
\vol 9
\issue 3
\yr 1983
\pages 223 -- 234


\RBibitem{sharap2}
\by И.\,И. Шарапудинов
\paper О наилучшем приближении и суммах Фурье-Якоби
\inbook Математические заметки
\vol 34
\issue 5
\yr 1983
\pages 651 -- 661



\RBibitem{Timan}
\by А.Ф. Тиман
\paper
\inbook  Теория приближения функций действительного переменного
\publ Физматгиз
\yr 1960
\pages
\publaddr Москва



");
            Utils.RenameRBibitems(ref sb, key => $"prefix-{key}");
            var newText = sb.ToString();
            Assert.AreEqual(6, newText.Split('\n').Count(l => l.Contains("RBibitem")));
            Assert.AreEqual(6, newText.Split('\n').Count(l => l.Contains("prefix-")));

        }
    }
}
