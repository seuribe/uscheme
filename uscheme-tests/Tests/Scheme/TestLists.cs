using NUnit.Framework;

namespace UScheme.Tests {
    [TestFixture]
    public class TestLists : TestEval {
        [TestCase("'(1 2 3 4)", 3, "4")]
        [TestCase("'(1 2 3 4 5 6)", 0, "1")]
        public void Nth(string listExpression, int index, string expected) {
            WhenEvaluating("(nth " + index + " " + listExpression + ")");
            ThenResultIsExp(Parser.Parse(expected));
        }

        [Test]
        public void BuildListWithElements() {
            WhenEvaluating("(list 1 2 3)");
            ThenResultIsExp(Cell.BuildList(Number1, Number2, Number3));
        }

        [Test]
        public void BuildEmptyList() {
            WhenEvaluating("(list)");
            ThenResultIsExp(Cell.BuildList());
        }
    }
}
