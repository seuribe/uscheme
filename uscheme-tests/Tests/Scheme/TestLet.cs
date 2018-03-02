using NUnit.Framework;

namespace UScheme.Tests {
    [TestFixture]
    public class TestLet : TestEval {

        [Test]
        public void LetDefinesValues() {
            WhenEvaluating("(let ((x 10) (y 20)) (+ x y))");
            ThenIntegerResultIs(30);
        }

        [Test]
        public void LetAcceptsMultipleFormsInBody() {
            WhenEvaluating("(let ((x 1)) 1 2)");
            ThenIntegerResultIs(2);
        }

        [Test]
        public void LetValuesNotInOutsideEnv() {
            WhenEvaluating("(define x \"sasasa\")");
            WhenEvaluating("(let ((x 10) (y 20)) (+ x y))");
            WhenEvaluating("x");
            ThenStringResultIs("sasasa");
            ThrowsEvalExceptionWhenEvaluating("y");
        }

        [Test]
        public void LetDoesNotOverrideOutsideScope() {
            WhenEvaluating("(let ((x 10) (y 20)) (+ x y))");
            ThrowsEvalExceptionWhenEvaluating("x");
            ThrowsEvalExceptionWhenEvaluating("y");
        }
    }
}
