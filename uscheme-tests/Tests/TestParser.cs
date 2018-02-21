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

        [TestCase("(", " ( ")]
        [TestCase(")", " ) ")]
        [TestCase("(((", " (  (  ( ")]
        [TestCase("()", " (  ) ")]
        [TestCase(")(", " )  ( ")]
        [TestCase(")a(", " ) a ( ")]
        public void PreProcessAddsSpacesToOpenParens(string input, string expected) {
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
    }
}
