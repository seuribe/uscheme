using NUnit.Framework;

namespace UScheme.Tests {
    [TestFixture]
    public class TestConditionalForms : TestEval {

        [Test]
        public void EmptyAndIsTrue() {
            WhenEvaluating("(and)");
            ThenBooleanResultIs(true);
        }

        [TestCase("(and #f)", false)]
        [TestCase("(and #t)", true)]
        [TestCase("(and #t #t #t)", true)]
        [TestCase("(and #t #f #t)", false)]
        public void AndWithValues(string expression, bool expected) {
            WhenEvaluating(expression);
            ThenBooleanResultIs(expected);
        }

        [TestCase("(and (< 0 1) (> 6 2) #t)", true)]
        [TestCase("(and (= 0 1) #t #t)", false)]
        public void AndWithNestedExpressions(string expression, bool expected) {
            WhenEvaluating(expression);
            ThenBooleanResultIs(expected);
        }

        [TestCase("(and #t 3)", "3")]
        [TestCase("(and #t 'd)", "'d")]
        public void AndReturnsLastExpression(string expression, string expectedString) {
            var expected = UScheme.Eval(Parser.Parse(expectedString), initialEnv);
            WhenEvaluating(expression);
            ThenResultIsExp(expected);
        }

        [Test]
        public void EmptyOrIsFalse() {
            WhenEvaluating("(or)");
            ThenBooleanResultIs(false);
        }

        [TestCase("(or #f)", false)]
        [TestCase("(or #t)", true)]
        [TestCase("(or #t #t #t)", true)]
        [TestCase("(or #t #f #t)", true)]
        [TestCase("(or #f #f #f)", false)]
        public void OrWithValues(string expression, bool expected) {
            WhenEvaluating(expression);
            ThenBooleanResultIs(expected);
        }

        [TestCase("(or (< 0 1) (> 6 2) #t)", true)]
        [TestCase("(or (= 0 1) #f #f)", false)]
        public void OrWithNestedExpressions(string expression, bool expected) {
            WhenEvaluating(expression);
            ThenBooleanResultIs(expected);
        }

        [TestCase("(or #t 3)", "#t")]
        [TestCase("(or 'a #t 'd)", "'a")]
        public void OrReturnsLastTrueExpression(string expression, string expectedString) {
            var expected = UScheme.Eval(Parser.Parse(expectedString), initialEnv);
            WhenEvaluating(expression);
            ThenResultIsExp(expected);
        }
    }
}
