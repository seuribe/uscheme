using NUnit.Framework;

namespace UScheme.Tests {
    [TestFixture]
    public class TestApply : TestEval {
        [Test]
        public void ApplyWithoutArgs() {
            WhenEvaluating("(apply + (list 3 4))");
            ThenIntegerResultIs(7);
            WhenEvaluating("(apply + '(3 4))");
            ThenIntegerResultIs(7);
        }

        [Test]
        public void ApplyWithArgs() {
            WhenEvaluating("(apply + 1 2 (list 3 4))");
            ThenIntegerResultIs(10);
        }

        [Test]
        public void ApplyWithVarOpArgs() {
            WhenEvaluating("(apply (if (> 0 1) + *) (list 3 4))");
            ThenIntegerResultIs(12);
        }


    }
}
