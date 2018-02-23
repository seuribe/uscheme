﻿using NUnit.Framework;

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

        string EncloseInQuotes(string str) {
            return CharConstants.DoubleQuote + str + CharConstants.DoubleQuote;
        }
    }
}
