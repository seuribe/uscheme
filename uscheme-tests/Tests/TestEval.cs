using System;
using NUnit.Framework;

namespace UScheme.Tests {
    [TestFixture]
    public class TestEval : TestConstants {

        Env initialEnv;
        [SetUp]
        public void SetUp() {
            initialEnv = Env.InitialEnv();
        }

        [TestCase("0", 0)]
        [TestCase("-1", -1)]
        [TestCase("1000", 1000)]
        public void EvalIntegerNumberFromString(string str, int number) {
            var exp = EvalString(str);
            Assert.IsInstanceOf<IntegerNumber>(exp);
            Assert.AreEqual(number, (exp as IntegerNumber).IntValue);
        }

        [TestCase("0.5478", 0.5478f)]
        [TestCase("-100.9", -100.9f)]
        [TestCase("4823089.43943", 4823089.43943f)]
        public void EvalRealNumberFromString(string str, float number) {
            var exp = EvalString(str);
            Assert.IsInstanceOf<RealNumber>(exp);
            Assert.AreEqual(number, (exp as RealNumber).FloatValue);
        }

        [Test]
        public void EvalSimpleAddition() {
            var exp = EvalString(SimpleSum);
            Assert.AreEqual(SimpleSumResult.IntValue, (exp as IntegerNumber).IntValue);
        }

        [Test]
        public void EvalNestedAddition() {
            var exp = EvalString(NestedSum);
            Assert.AreEqual(NestedSumResult.IntValue, (exp as IntegerNumber).IntValue);
        }

        Exp EvalString(string str) {
            var exp = Parser.Parse(str);
            return UScheme.Eval(exp, initialEnv);
        }
    }
}
