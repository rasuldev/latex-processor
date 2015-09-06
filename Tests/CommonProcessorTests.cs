using System;
using System.Text;
using NUnit.Framework;
using TextProcessor;
using System.Text.RegularExpressions;

namespace Tests
{
    [TestFixture]
    public class CommonProcessorTests
    {
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
    }
}