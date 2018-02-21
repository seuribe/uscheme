using System;
using NUnit.Framework;

namespace UScheme.Tests {
    [TestFixture]
    public class TestParser {

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
    }
}
