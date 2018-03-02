using NUnit.Framework;

namespace UScheme.Tests {

    [TestFixture]
    public class MiscelaneousEval : TestEval {

        [Test]
        public void ProcessesMultipleForms() {
            var code = @"(define a 10)
                         (define b 14)
                         (define c (+ a b))
                         c";
            WhenEvaluating(code);
            ThenIntegerResultIs(24);
            ThenEnvContains("a");
            ThenEnvContains("b");
            WhenEvaluating("a");
            ThenIntegerResultIs(10);
            WhenEvaluating("b");
            ThenIntegerResultIs(14);
        }

        [Test]
        public void EvalConstant() {
            var exp = UScheme.Eval(Number1, Env.Global);
            Assert.AreSame(Number1, exp);
        }

        [Test]
        public void ListsStartingWithSymbolThrow() {
            ThrowsEvalExceptionWhenEvaluating("(1 2)");
        }
    }
}
