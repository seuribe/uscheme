using System;
using NUnit.Framework;
using System.Collections.Generic;

namespace UScheme.Tests {

    [TestFixture]
    public abstract class TestEval : TestConstants {

        protected Env initialEnv;
        protected Exp evalResult;

        [SetUp]
        public void SetUp() {
            initialEnv = Env.InitialEnv();
        }

        protected void WhenEvaluating(string str) {
            evalResult = UScheme.Eval(Parser.Parse(str), initialEnv);
        }

        protected void ThenResultIs<T>() where T : Exp {
            Assert.IsInstanceOf<T>(evalResult);
        }

        protected void ThenRealResultIs(RealNumber number) {
            ThenRealResultIs(number.FloatValue);
        }

        protected void ThenRealResultIs(float result) {
            Assert.IsInstanceOf<RealNumber>(evalResult);
            Assert.AreEqual(result, (evalResult as RealNumber).FloatValue);
        }

        protected void ThenIntegerResultIs(IntegerNumber number) {
            ThenIntegerResultIs(number.IntValue);
        }

        protected void ThenIntegerResultIs(int result) {
            Assert.IsInstanceOf<IntegerNumber>(evalResult);
            Assert.AreEqual(result, (evalResult as IntegerNumber).IntValue);
        }

        protected void ThenBooleanResultIs(bool value) {
            Assert.IsInstanceOf<Boolean>(evalResult);
            Assert.AreEqual(value, (evalResult as Boolean).Value);
        }

        protected void ThenStringResultIs(string value) {
            Assert.IsInstanceOf<UString>(evalResult);
            Assert.AreEqual(value, (evalResult as UString).str);
        }

        protected void ThenResultIsExp(Exp expression) {
            Assert.IsTrue(expression.UEquals(evalResult));
        }

        protected void ThrowsEvalExceptionWhenEvaluating(string str) {
            Assert.Throws(typeof(EvalException), () => WhenEvaluating(str));
        }

        protected void ThenResultIsProcedureAnd(Action<SchemeProcedure> assertion) {
            ThenResultIs<SchemeProcedure>();
            var proc = evalResult as SchemeProcedure;
            assertion(proc);
        }

        protected void ThenResultIsSymbol(string str) {
            Assert.IsTrue(Symbol.From(str).UEquals(evalResult));
        }

        protected void ThenResultIs(string str) {
            var exp = Parser.Parse(str);
            Assert.IsTrue(exp.UEquals(evalResult));
        }

        protected void ThenResultSatisfies(Func<Exp, bool> predicate) {
            Assert.IsTrue(predicate(evalResult));
        }

        protected void ExpressionsAreEquivalent(string a, string b) {
            var expA = UScheme.Eval(Parser.Parse(a), initialEnv);
            var expB = UScheme.Eval(Parser.Parse(b), initialEnv);
            Assert.IsTrue(expA.UEquals(expB));
        }

        protected void ProcedureArgumentNamesAre(SchemeProcedure proc, IEnumerable<string> argumentNames) {
            CollectionAssert.AreEqual(argumentNames, proc.ArgumentNames);
        }
    }
}
