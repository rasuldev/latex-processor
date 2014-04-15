using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using TextProcessor.Latex;

namespace Tests
{
    [TestFixture]
    public class MacrosTests
    {
        [Test]
        public void ProcessTest()
        {
            var macros = new Macros(@"\newcommand{\norm}[1]{\|#1\|_{p(\cdot),w}}");
            var sb = new StringBuilder(@"Известно, что $\norm{f}$ определяется... Если $\norm{f}\le \norm{g} $, то...");
            macros.ApplyMacros(ref sb);
            string actual = sb.ToString();
            string expected =
                @"Известно, что $\|f\|_{p(\cdot),w}$ определяется... Если $\|f\|_{p(\cdot),w}\le \|g\|_{p(\cdot),w} $, то...";
            
            Assert.AreEqual(expected,actual);
        }
    }
}
