using NUnit.Framework;

namespace UScheme.Tests {
    [TestFixture]
    public class TestParseByteVector: TestEval {
        [Test]
        public void CreateVector() {
            WhenEvaluating("#vu8(1 2 3 4)");
            ThenResultIs<ByteVector>();
        }

        [TestCase("(byte-vector? #vu8(1 2 3 4))", true)]
        [TestCase("(byte-vector? 1)", false)]
        [TestCase("(byte-vector? 'a)", false)]
        [TestCase("(byte-vector? \"aa\")", false)]
        [TestCase("(byte-vector? (list 1 2))", false)]
        [TestCase("(byte-vector? #(1 2 3))", false)]
        public void IsByteVector(string expression, bool expected) {
            WhenEvaluating(expression);
            ThenBooleanResultIs(expected);
        }
    }
}
