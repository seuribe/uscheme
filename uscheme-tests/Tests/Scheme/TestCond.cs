using NUnit.Framework;

namespace UScheme.Tests {
    [TestFixture]
    public class TestCond : TestEval {
        [SetUp]
        public new void SetUp() {
            UScheme.SetEvaluator(new StackEvaluator());
        }

        [Test]
        public void CondWithBooleans() {
            WhenEvaluating("(cond (#t 1) (#f 2))");
            ThenIntegerResultIs(1);
        }

        [Test]
        public void CondWithNestedExpressions() {
            WhenEvaluating("(cond ((> 1 2) 1) ((> 2 1) 2))");
            ThenIntegerResultIs(2);
        }

        [Test]
        public void CondWithAllFalse() {
            WhenEvaluating("(cond ((> 1 2) 1) ((> 2 3) 2))");
            ThenBooleanResultIs(false);
        }

        [Test]
        public void CondWithElse() {
            WhenEvaluating("(cond ((> 1 2) 1) ((> 2 3) 2) (else 'result))");
            ThenResultIsExp(Identifier.From("result"));
        }
    }
}
