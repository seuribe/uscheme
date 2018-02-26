using NUnit.Framework;
using System.Collections.Generic;

namespace UScheme.Tests {

    [TestFixture]
    public class TestLambda : TestEval {

        [Test]
        public void LambdaParameters() {
            WhenEvaluating("(lambda (x) (+ 1 x))");
            ThenResultIsProcedureAnd(p => ProcedureArgumentNamesAre(p, new List<string> { "x" }));
        }

        [Test]
        public void LambdaEvaluation() {
            WhenEvaluating("((lambda (x) (+ 1 x)) 1)");
            ThenIntegerResultIs(2);
        }

        [Test]
        public void FunctionLambdaApplication() {
            WhenEvaluating("((lambda (x) (+ 1 x)) 4)");
            ThenIntegerResultIs(5);
        }
    }
}
