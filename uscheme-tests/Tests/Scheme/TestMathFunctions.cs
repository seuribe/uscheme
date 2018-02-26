using NUnit.Framework;

namespace UScheme.Tests {
    [TestFixture]
    public class TestMathFunctions : TestEval {
        [TestCase("(+ 1 2)", 3)]
        [TestCase("(+ 1 2 3 4 5)", 15)]
        [TestCase("(+ 3 (+ 1 2) 4)", 10)]
        public void EvalSimpleAddition(string expression, int result) {
            WhenEvaluating(expression);
            ThenIntegerResultIs(result);
        }

        [Test]
        public void EvalCSharpProcedure() {
            WhenEvaluating("(< 0 1)");
            ThenBooleanResultIs(true);
        }

    }
}
