using NUnit.Framework;

namespace UScheme.Tests {
    [TestFixture]

    public class TestStackEval : TestEval {

        [Test]
        public void StackEvalConstant() {
            var exp = UScheme.StackEval(Number1, Env.Global);
            Assert.AreSame(Number1, exp);
        }

        [Test]
        public void StackEvalIfWithExplicitBooleanTrue() {
            WhenEvaluating("(if #t 1 2)");
            ThenResultIs<IntegerNumber>();
            ThenIntegerResultIs(1);
        }

        [Test]
        public void StackEvalIfWithExplicitBooleanFalse() {
            WhenEvaluating("(if #f 1 2)");
            ThenResultIs<IntegerNumber>();
            ThenIntegerResultIs(2);
        }

        [Test]
        public void StackEvalCSharpProcedure() {
            WhenEvaluating("(< 0 1)");
            ThenBooleanResultIs(true);
        }

        [Test]
        public void StackEvalIfWithExpressionInCondition() {
            WhenEvaluating("(if (< 0 1) 1 2)");
            ThenResultIs<IntegerNumber>();
            ThenIntegerResultIs(1);
        }

        protected new void WhenEvaluating(string str) {
            evalResult = UScheme.StackEval(Parser.Parse(str), initialEnv);
        }

    }
}
