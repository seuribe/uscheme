using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace UScheme.Tests {

    [TestFixture]
    public class TestStackEval : TestEval {

        [SetUp]
        public new void SetUp() {
            UScheme.SetEvaluator(new StackEvaluator());
        }

        [Test]
        public void StackEvalConstant() {
            var exp = UScheme.Eval(Number1, Env.Global);
            Assert.AreSame(Number1, exp);
        }

        [Test]
        public void StackEvalCSharpProcedure() {
            WhenEvaluating("(< 0 1)");
            ThenBooleanResultIs(true);
        }


        [Test]
        public void StackEvalQuote() {
            WhenEvaluating("'(1 2 3)");
            ThenResultIsExp(Cell.BuildList(Number1, Number2, Number3));
            WhenEvaluating("(quote (1 2 3))");
            ThenResultIsExp(Cell.BuildList(Number1, Number2, Number3));
        }

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
        public void BeginReturnsLastValue() {
            WhenEvaluating("(begin 1 2 3)");
            ThenIntegerResultIs(3);
        }

        [Test]
        public void BeginWithNestedExpressions() {
            WhenEvaluating("(begin 1 2 (> 0 1))");
            ThenBooleanResultIs(false);
        }

        [Test]
        public void BeginWithinOtherExpression() {
            WhenEvaluating("(< (begin 1 2 (+ 1 2)) 4)");
            ThenBooleanResultIs(true);
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

        [Test]
        public void SetSetsValue() {
            WhenEvaluating("(define x 10)");
            WhenEvaluating("x");
            ThenIntegerResultIs(10);
            WhenEvaluating("(set! x 9)");
            WhenEvaluating("x");
            ThenIntegerResultIs(9);
        }

        [Test]
        public void LetDefinesValues() {
            WhenEvaluating("(let ((x 10) (y 20)) (+ x y))");
            ThenIntegerResultIs(30);
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

        [Test]
        public void CannotSetUndefinedValues() {
            ThrowsEvalExceptionWhenEvaluating("(set! x 78)");
        }

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


        [Test]
        public void TailCallOptimized() {
            WhenEvaluating("(define (f x) (if (<= x 0) x (f (- x 1))))");
            WhenEvaluating("(f 10)");
            ThenIntegerResultIs(0);
        }

        [Test]
        public void ConsBuildsAPair() {
            WhenEvaluating("(cons 1 2)");
            ThenResultIs<Cell>();
            ThenResultIsExp(new Cell(Number1, Number2));
        }

        [Test]
        public void ConsPairsAreNotLists() {
            WhenEvaluating("(cons 1 2)");
            ThenResultSatisfies(exp => (exp is Cell) && !(exp as Cell).IsList);
        }

        [Test]
        public void ListsStartingWithSymbolThrow() {
            ThrowsEvalExceptionWhenEvaluating("(1 2)");
        }

        protected void ProcedureArgumentNamesAre(SchemeProcedure proc, IEnumerable<string> argumentNames) {
            CollectionAssert.AreEqual(argumentNames, proc.ArgumentNames);
        }

        protected new void WhenEvaluating(string str) {
            evalResult = UScheme.Eval(Parser.Parse(str), initialEnv);
        }
    }
}
