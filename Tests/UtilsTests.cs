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

            Assert.AreEqual(expected,actual);
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

            Assert.AreEqual(expected,eqrefs);
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
            Utils.RenameEnvs(ref sb,"lemma",@"\textbf{Лемма #counter#.}\textit{","}");
            Assert.AreEqual(expected,sb.ToString());

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
            Utils.RenameEnvs(ref sb, "lemma", @"\textbf{Лемма #counter#.}\textit{", "}", i=>new[]{"A","B","C"}[i-1]);
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
            Assert.AreEqual(expected,sb.ToString());
        }

        [Test]
        public void FindLineTest()
        {
            string text = "1abc\r\n2abc\r\n3abc\r\n4abc";
            int line = Utils.FindLine(text, 6);
            Assert.AreEqual(1,line);

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
            Assert.AreEqual(tb.StartPos,17);
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

    }
}
