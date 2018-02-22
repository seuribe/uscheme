using NUnit.Framework;

namespace UScheme.Tests {

    [TestFixture]
    public class TestParser : TestConstants {
        [Test]
        public void ParseAtom() {
            var expression = Parser.Parse("4");
            Assert.IsTrue(expression.UEquals(Number4));
        }

        [Test]
        public void ParseSingletonList() {
            var expression = Parser.Parse("(f)");
            Assert.IsTrue(expression.UEquals(new UList { Symbol.From("f") }));
        }

        [Test]
        public void ParseSimpleForm() {
            var expression = Parser.Parse(SimpleSum);
            Assert.IsTrue(SimpleSumForm.UEquals(expression));
        }

        [Test]
        public void ParseNestedForm() {
            var expression = Parser.Parse(NestedSum);
            Assert.IsTrue(NestedSumForm.UEquals(expression));
        }

        [Test]
        public void ParseQuote() {
            var expression = Parser.Parse("'a");
            Assert.IsTrue(expression.UEquals(new UList { Symbol.QUOTE, Symbol.From("a") }));
        }

        [TestCase("\"a\"", "\"a\"")]
        [TestCase("\"a cow\"", "\"a cow\"")]
        public void ParseStrings(string input, string expectedAtomString) {
            var expression = Parser.Parse(input);
            Assert.AreEqual(expectedAtomString, expression.ToString());

        }
    }
}
