using NUnit.Framework;

namespace UScheme.Tests {

    [TestFixture]
    public class MiscelaneousEval : TestEval {

        [Test]
        public void EvalConstant() {
            var exp = UScheme.Eval(Number1, Env.Global);
            Assert.AreSame(Number1, exp);
        }

        [Test]
        [Ignore("Meant to 'prove' that it does not fill the stack when doing tail call optimization. Can take long to execute!")]
        public void TailCallOptimized() {
            WhenEvaluating("(define (f x) (if (<= x 0) x (f (- x 1))))");
            WhenEvaluating("(f 100000)");
            ThenIntegerResultIs(0);
        }

        [Test]
        public void ListsStartingWithSymbolThrow() {
            ThrowsEvalExceptionWhenEvaluating("(1 2)");
        }
    }
}
