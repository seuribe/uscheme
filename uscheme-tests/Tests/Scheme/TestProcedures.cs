using NUnit.Framework;

namespace UScheme.Tests {
    [TestFixture]
    public class TestProcedures : TestEval {

        [Test]
        public void ProcedureWithMultipleFormsInBody() {
            WhenEvaluating("(define (f x) x x x (- 1 x))");
            WhenEvaluating("(f 7)");
            ThenIntegerResultIs(-6);
        }

        [Test]
        public void ProcedureWithIf() {
            WhenEvaluating("(define (f x) (if (> x 1) (* 2 x) x))");
            WhenEvaluating("(f 1)");
            ThenIntegerResultIs(1);
            WhenEvaluating("(f 2)");
            ThenIntegerResultIs(4);
        }

        [Test]
        public void ProcedureComposition() {
            WhenEvaluating("(define (f x) (* 2 x))");
            WhenEvaluating("(define (g x) (+ (f x) (f (f x))))");
            WhenEvaluating("(g 11)");
            ThenIntegerResultIs(66);

        }

        [TestCase(50000)]
        public void TestTailCall(int depth) {
            WhenEvaluating("(define (f x) (if (<= x 0) x (f (- x 1))))");
            WhenEvaluating("(f " + depth + ")");
            ThenIntegerResultIs(0);
        }
    }
}
