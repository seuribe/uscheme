using NUnit.Framework;

namespace UScheme.Tests {

    [TestFixture]
    public class TestParser : TestConstants {

        Exp evalResult;

        [SetUp]
        public void SetUp() {
            evalResult = null;
        }

        [Test]
        [Ignore("not implemented correctly yet")]
        public void ParseIncompleteThrows() {
            Assert.Throws<ParseException>(() => Parser.Parse("(define"));
        }

        [Test]
        public void ParseAtom() {
            var expression = Parser.Parse("4");
            Assert.IsTrue(expression.UEquals(Number4));
        }

        [Test]
        public void ParseSingletonList() {
            var expression = Parser.Parse("(f)");
            Assert.IsTrue(expression.UEquals(Cell.BuildList(Symbol.From("f"))));
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
            Assert.IsTrue(expression.UEquals(Cell.BuildList(Symbol.QUOTE, Symbol.From("a"))));
        }

        [TestCase("a")]
        [TestCase("a cow")]
        [TestCase("a\tcow")]
        [TestCase("a (big) cow")]
        [TestCase("a (big big big) cow")]
        public void ParseStrings(string input) {
            var quoted = EncloseInQuotes(input);
            var expression = Parser.Parse(quoted);
            Assert.AreEqual(quoted, expression.ToString());
        }

        [TestCase("#(1)")]
        [TestCase("#((list 1 2) 'a 47)")]
        public void ParseVectors(string expression) {
            WhenParsing(expression);
            ThenResultIs<Vector>();
        }

        void WhenParsing(string str) {
            evalResult = Parser.Parse(str);
        }

        void ThenResultIs<T>() {
            Assert.IsInstanceOf<T>(evalResult);
        }

        string EncloseInQuotes(string str) {
            return CharConstants.DoubleQuote + str + CharConstants.DoubleQuote;
        }
    }
}
