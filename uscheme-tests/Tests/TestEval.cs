using System;
using NUnit.Framework;

namespace UScheme.Tests {
    [TestFixture]
    public class TestEval : TestConstants {

        Env initialEnv;
        Exp evalResult;

        [SetUp]
        public void SetUp() {
            initialEnv = Env.InitialEnv();
        }

        [TestCase("0", 0)]
        [TestCase("-1", -1)]
        [TestCase("1000", 1000)]
        public void EvalIntegerNumberFromString(string str, int number) {
            WhenEvaluating(str);
            ThenIntegerResultIs(number);
        }

        [TestCase("0.5478", 0.5478f)]
        [TestCase("-100.9", -100.9f)]
        [TestCase("4823089.43943", 4823089.43943f)]
        public void EvalRealNumberFromString(string str, float number) {
            WhenEvaluating(str);
            ThenRealResultIs(number);
        }

        [Test]
        public void EvalSimpleAddition() {
            WhenEvaluating(SimpleSum);
            ThenIntegerResultIs(SimpleSumResult);
        }

        [Test]
        public void EvalNestedAddition() {
            WhenEvaluating(NestedSum);
            ThenIntegerResultIs(NestedSumResult);
        }

        [Test]
        public void FunctionLambdaApplication() {
            WhenEvaluating("((lambda (x) (+ 1 x)) 4)");
            ThenIntegerResultIs(5);
        }

        [Test]
        public void DefineAndCallProcedure() {
            WhenEvaluating("(define (f x) (+ 1 x))");
            ThenResultIs<SchemeProcedure>();
            WhenEvaluating("(f 2)");
            ThenIntegerResultIs(3);
        }

        [Test]
        public void BasicIf() {
            WhenEvaluating("(if #t 1 2)");
            ThenIntegerResultIs(1);
        }

        [Test]
        public void IfExpandsCondition() {
            WhenEvaluating("(if (< 1 0) 1 2)");
            ThenIntegerResultIs(2);
        }

        [Test]
        public void IfExpandsOnlyExpectedBranch() {
            WhenEvaluating("(define r 0)");
            WhenEvaluating("r");
            ThenIntegerResultIs(0);
            WhenEvaluating("(if #t (set! r 1) (set! r 2))");
            WhenEvaluating("r");
            ThenIntegerResultIs(1);
        }

        void WhenEvaluating(string str) {
            evalResult = UScheme.Eval(Parser.Parse(str), initialEnv);
        }

        void ThenResultIs<T>() where T : Exp {
            Assert.IsInstanceOf<T>(evalResult);
        }

        void ThenRealResultIs(RealNumber number) {
            ThenRealResultIs(number.FloatValue);
        }

        void ThenRealResultIs(float result) {
            Assert.IsInstanceOf<RealNumber>(evalResult);
            Assert.AreEqual(result, (evalResult as RealNumber).FloatValue);
        }

        void ThenIntegerResultIs(IntegerNumber number) {
            ThenIntegerResultIs(number.IntValue);
        }

        void ThenIntegerResultIs(int result) {
            Assert.IsInstanceOf<IntegerNumber>(evalResult);
            Assert.AreEqual(result, (evalResult as IntegerNumber).IntValue);
        }
    }
}
