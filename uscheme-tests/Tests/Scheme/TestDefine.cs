using NUnit.Framework;

namespace UScheme.Tests {
    [TestFixture]
    public class TestDefine : TestEval {
        [Test]
        public void DefineAndCallProcedure() {
            WhenEvaluating("(define (f x) (+ 1 x))");
            ThenResultIs<SchemeProcedure>();
            WhenEvaluating("(f 2)");
            ThenIntegerResultIs(3);
        }

        [Test]
        public void DefineSimpleValue() {
            WhenEvaluating("(define x 10)");
            WhenEvaluating("x");
            ThenIntegerResultIs(10);
        }

        [Test]
        public void CanRedefineValues() {
            WhenEvaluating("(define x 10)");
            WhenEvaluating("x");
            ThenIntegerResultIs(10);
            WhenEvaluating("(define x 11)");
            WhenEvaluating("x");
            ThenIntegerResultIs(11);
        }

        [Test]
        public void DefineProcedure() {
            WhenEvaluating("(define (f x) (+ 1 x))");
            ThenResultIs<SchemeProcedure>();
            WhenEvaluating("(f 7)");
            ThenIntegerResultIs(8);
        }

        [Test]
        public void DefineOnlyInCurrentScope() {
            WhenEvaluating("(define (f x) (begin (define k 10) (+ x k)))");
            WhenEvaluating("(f 7)");
            ThenIntegerResultIs(17);
            ThrowsEvalExceptionWhenEvaluating("k");
        }
    }
}
