using NUnit.Framework;

namespace UScheme.Tests {
    [TestFixture]
    public class TestIf : TestEval {

        [Test]
        public void BasicIf() {
            WhenEvaluating("(if #t 1 2)");
            ThenIntegerResultIs(1);
        }

        [Test]
        public void IfExpandsCondition() {
            WhenEvaluating("(if (< 1 0) 1 2)");
            ThenIntegerResultIs(2);
        }

        [TestCase("(if 1 2 3)", "2")]
        [TestCase("(if 'a 1 2)", "1")]
        [TestCase("(if (cons 1 2) 3 4)", "3")]
        [TestCase("(if (list 1 2) 3 4)", "3")]
        public void AnyValueCanBeCondition(string expression, string expected) {
            ExpressionsAreEquivalent(expression, expected);
        }

        [Test]
        public void IfWithExplicitBooleanTrue() {
            WhenEvaluating("(if #t 1 2)");
            ThenResultIs<IntegerNumber>();
            ThenIntegerResultIs(1);
        }

        [Test]
        public void IfWithExplicitBooleanFalse() {
            WhenEvaluating("(if #f 1 2)");
            ThenResultIs<IntegerNumber>();
            ThenIntegerResultIs(2);
        }

        [Test]
        public void IfWithExpressionInCondition() {
            WhenEvaluating("(if (< 0 1) 1 2)");
            ThenResultIs<IntegerNumber>();
            ThenIntegerResultIs(1);
        }

        [Test]
        public void IfExpandsOnlyExpectedBranch() {
            WhenEvaluating("(define r 0)");
            WhenEvaluating("r");
            ThenIntegerResultIs(0);
            WhenEvaluating("(if #t (set! r 1) (set! r 2))");
            WhenEvaluating("r");
            ThenIntegerResultIs(1);
        }
    }
}
