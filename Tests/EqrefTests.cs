using NUnit.Framework;
using TextProcessor.Latex;

namespace Tests
{
    [TestFixture]
    public class EqrefTests
    {
        [Test]
        public void EqrefCtorTest()
        {
            var eqref = new Eqref(23, @"\eqref{labelToFormula}");
            Assert.AreEqual("labelToFormula", eqref.Value);
        }
    }
}