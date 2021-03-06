﻿using NUnit.Framework;
using System.Collections.Generic;

namespace UScheme.Tests {

    [TestFixture]
    public class TestLambda : TestEval {

        [Test]
        public void LambdaWithoutParameters() {
            WhenEvaluating("((lambda () 1))");
            ThenIntegerResultIs(1);
        }

        [Test]
        public void LambdaWithMultipleFormsInBody() {
            WhenEvaluating("(lambda (x) (set! x (+ 1 x)) x)");
            ThenResultIsProcedureAnd(p => ProcedureArgumentNamesAre(p, new List<string> { "x" }));
            WhenEvaluating("((lambda (x) (set! x (+ 1 x)) x) 4)");
            ThenIntegerResultIs(5);
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
        public void FunctionLambdaApplication() {
            WhenEvaluating("((lambda (x) (+ 1 x)) 4)");
            ThenIntegerResultIs(5);
        }

        [Test]
        public void LambdaWithListParameter() {
            WhenEvaluating("((lambda ll (length ll)) 1 2 3)");
            ThenIntegerResultIs(3);
        }

        [Test]
        public void LambdaWithVariadic() {
            WhenEvaluating("((lambda (a b . c) (+ a b (length c))) 1 2 3 4 5)");
            ThenIntegerResultIs(6);
        }
    }
}
