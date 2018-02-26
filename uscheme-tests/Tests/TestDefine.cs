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
    }
}
