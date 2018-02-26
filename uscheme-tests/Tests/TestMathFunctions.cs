using NUnit.Framework;

namespace UScheme.Tests {
    [TestFixture]
    public class TestMathFunctions : TestEval {
        [Test]
        public void EvalSimpleAddition() {
            WhenEvaluating(SimpleSum);
            ThenIntegerResultIs(SimpleSumResult);
        }

        [Test]
        public void EvalNestedAddition() {
            WhenEvaluating(NestedSum);
            ThenIntegerResultIs(NestedSumResult);
        }
    }
}
