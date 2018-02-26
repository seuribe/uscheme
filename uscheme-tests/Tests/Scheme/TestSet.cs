using NUnit.Framework;

namespace UScheme.Tests {
    [TestFixture]
    public class TestSet : TestEval {
        [Test]
        public void SetSetsValue() {
            WhenEvaluating("(define x 10)");
            WhenEvaluating("x");
            ThenIntegerResultIs(10);
            WhenEvaluating("(set! x 9)");
            WhenEvaluating("x");
            ThenIntegerResultIs(9);
        }

        [Test]
        public void CannotSetUndefinedValues() {
            ThrowsEvalExceptionWhenEvaluating("(set! x 78)");
        }
    }
}
