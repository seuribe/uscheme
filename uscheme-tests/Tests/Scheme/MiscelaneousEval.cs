using NUnit.Framework;

namespace UScheme.Tests {

    [TestFixture]
    public class MiscelaneousEval : TestEval {

        [Test]
        public void EvalConstant() {
            var exp = UScheme.Eval(Number1, Env.Global);
            Assert.AreSame(Number1, exp);
        }

        [Test]
        public void ListsStartingWithSymbolThrow() {
            ThrowsEvalExceptionWhenEvaluating("(1 2)");
        }
    }
}
