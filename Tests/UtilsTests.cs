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
    }
}
