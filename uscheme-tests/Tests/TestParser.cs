using System;
using NUnit.Framework;
using System.Collections.Generic;

namespace UScheme.Tests {
    [TestFixture]
    public class TestParser {

        static readonly string SimpleSum = "(+ 1 2)";
        static readonly List<string> SimpleSumTokens = new List<string> { "(", "+", "1", "2", ")"};
        static readonly string NestedSum = "(+ 3 (+ 1 2) 4)";
        static readonly List<string> NestedSumTokens = new List<string> { "(", "+", "3", "(", "+", "1", "2", ")", "4", ")" };

        static readonly UList SimpleSumForm = new UList { new Symbol("+"), new IntegerNumber(1), new IntegerNumber(2) };
        static readonly UList NestedSumForm = new UList {
            new Symbol("+"), new IntegerNumber(3), new UList {
                        new Symbol("+"), new IntegerNumber(1), new IntegerNumber(2) },
            new IntegerNumber(4) };

        [TestCase("(", " ( ")]
        [TestCase(")", " ) ")]
        [TestCase("'", " ' ")]
        [TestCase("(((", " (  (  ( ")]
        [TestCase("()", " (  ) ")]
        [TestCase(")(", " )  ( ")]
        [TestCase(")a(", " ) a ( ")]
        [TestCase("'()", " '  (  ) ")]
        public void PreProcessAddsSpacesWhenNeeded(string input, string expected) {
            var output = UReader.PreProcessInput(input);
            Assert.AreEqual(expected, output);
        }

        [TestCase("(;", " ( ")]
        [TestCase("(; fdjfdskf ds fds flds", " ( ")]
        public void PreProcessRemovesComments(string input, string expected) {
            var output = UReader.PreProcessInput(input);
            Assert.AreEqual(expected, output);
        }

        [Test]
        public void TokenizeLeavesAtomAlone() {
            var output = UReader.Tokenize("atom");
            Assert.AreEqual(1, output.Count);
            Assert.AreEqual("atom", output[0]);
        }

        [Test]
        public void TokenizeSimpleForm() {
            var output = UReader.Tokenize(SimpleSum);
            CollectionAssert.AreEqual(SimpleSumTokens, output);
        }

        [Test]
        public void TokenizeNestedForm() {
            var output = UReader.Tokenize(NestedSum);
            CollectionAssert.AreEqual(NestedSumTokens, output);
        }

        [Test]
        public void ParseAtom() {
            var expression = UReader.Parse("4");
            Assert.IsTrue(expression.UEquals(new IntegerNumber(4)));
        }

        [Test]
        public void ParseSingletonList() {
            var expression = UReader.Parse("(f)");
            Assert.IsTrue(expression.UEquals(new UList { new Symbol("f") }));
        }

        [Test]
        public void ParseSimpleForm() {
            var expression = UReader.Parse(SimpleSum);
            Assert.IsTrue(SimpleSumForm.UEquals(expression));
        }

        [Test]
        public void ParseNestedForm() {
            var expression = UReader.Parse(NestedSum);
            Assert.IsTrue(NestedSumForm.UEquals(expression));
        }

        [Test]
        public void ParseQuote() {
            var expression = UReader.Parse("'a");
            Assert.IsTrue(expression.UEquals(new UList { Symbol.QUOTE, new Symbol("a") }));
        }
    }
}
