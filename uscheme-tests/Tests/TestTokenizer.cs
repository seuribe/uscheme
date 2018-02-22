using NUnit.Framework;

namespace UScheme.Tests {
    [TestFixture]
    public class TestTokenizer : TestConstants {
        [TestCase(' ', true)]
        [TestCase('\t', true)]
        [TestCase('\n', true)]
        [TestCase('\r', true)]
        [TestCase(')', true)]
        [TestCase(']', true)]
        public void IdentifiesAtomsEnd(char ch, bool expected) {
            Assert.AreEqual(expected, Tokenizer.IsAtomEnd(ch));
        }

        [Test]
        public void TokenizeLeavesAtomAlone() {
            var output = new Tokenizer("atom").Tokens;
            Assert.AreEqual(1, output.Count);
            Assert.AreEqual("atom", output[0]);
        }

        [Test]
        public void TokenizeSimpleForm() {
            var output = new Tokenizer(SimpleSum).Tokens;
            CollectionAssert.AreEqual(SimpleSumTokens, output);
        }

        [Test]
        public void TokenizeNestedForm() {
            var output = new Tokenizer(NestedSum).Tokens;
            CollectionAssert.AreEqual(NestedSumTokens, output);
        }
    }
}
