using NUnit.Framework;

namespace UScheme.Tests {
    [TestFixture]
    public class TestSequential : TestEval {
        [Test]
        public void BeginReturnsLastValue() {
            WhenEvaluating("(begin 1 2 3)");
            ThenIntegerResultIs(3);
        }

        [Test]
        public void BeginWithNestedExpressions() {
            WhenEvaluating("(begin 1 2 (> 0 1))");
            ThenBooleanResultIs(false);
        }

        [Test]
        public void BeginWithinOtherExpression() {
            WhenEvaluating("(< (begin 1 2 (+ 1 2)) 4)");
            ThenBooleanResultIs(true);
        }
    }
}
