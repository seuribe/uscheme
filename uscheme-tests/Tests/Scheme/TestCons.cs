using NUnit.Framework;

namespace UScheme.Tests {
    [TestFixture]
    public class TestCons : TestEval {

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
    }
}
