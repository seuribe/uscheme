using NUnit.Framework;

namespace UScheme.Tests {
    [TestFixture]
    public class TestQuote : TestEval {
        [Test]
        public void StackEvalQuote() {
            WhenEvaluating("'(1 2 3)");
            ThenResultIsExp(Cell.BuildList(Number1, Number2, Number3));
            WhenEvaluating("(quote (1 2 3))");
            ThenResultIsExp(Cell.BuildList(Number1, Number2, Number3));
        }
    }
}
