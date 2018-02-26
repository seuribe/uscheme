using NUnit.Framework;

namespace UScheme.Tests {
    [TestFixture]
    public class TestLambda : TestEval {
        [Test]
        public void FunctionLambdaApplication() {
            WhenEvaluating("((lambda (x) (+ 1 x)) 4)");
            ThenIntegerResultIs(5);
        }
    }
}
