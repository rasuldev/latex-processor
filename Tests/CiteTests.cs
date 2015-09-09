using NUnit.Framework;
using TextProcessor.Latex;

namespace Tests
{
    [TestFixture]
    public class CiteTests
    {
        [Test]
        public void CiteKeysExtractingTest()
        {
            // Arrange
            var content = @"It is well known \cite{ref1, ref2 ,ref3 } that";
            var start = content.IndexOf(@"\cite");
            var end = content.LastIndexOf("}");

            // Act
            var cite = new Cite(start, content.Substring(start, end - start + 1));

            // Assert
            CollectionAssert.AreEqual(new[]{"ref1","ref2","ref3"}, cite.Keys);
        }

        [Test]
        public void CiteMarkupTest()
        {
            Assert.AreEqual(@"\cite{ref1, ref2, ref3}",
                Cite.GenerateMarkup(new[] {"ref1", "ref2", "ref3"}));
        }
    }
}