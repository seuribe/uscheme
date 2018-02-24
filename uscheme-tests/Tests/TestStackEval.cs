﻿using NUnit.Framework;
using System;
using System.Collections.Generic;

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

        protected void ProcedureArgumentNamesAre(SchemeProcedure proc, IEnumerable<string> argumentNames) {
            CollectionAssert.AreEqual(argumentNames, proc.ArgumentNames);
        }

        protected new void WhenEvaluating(string str) {
            evalResult = UScheme.StackEval(Parser.Parse(str), initialEnv);
        }
    }
}
