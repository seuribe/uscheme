using NUnit.Framework;

namespace UScheme.Tests {
    [TestFixture]
    public class TestNumericConstants : TestEval {
        [TestCase("0", 0)]
        [TestCase("-1", -1)]
        [TestCase("1000", 1000)]
        public void EvalIntegerNumberFromString(string str, int number) {
            WhenEvaluating(str);
            ThenIntegerResultIs(number);
        }

        [TestCase("0.5478", 0.5478f)]
        [TestCase("-100.9", -100.9f)]
        [TestCase("4823089.43943", 4823089.43943f)]
        public void EvalRealNumberFromString(string str, float number) {
            WhenEvaluating(str);
            ThenRealResultIs(number);
        }
    }
}
