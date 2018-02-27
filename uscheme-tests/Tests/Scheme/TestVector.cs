using NUnit.Framework;

namespace UScheme.Tests {
    [TestFixture]
    public class TestVector : TestEval {

        [TestCase("'#(1 2 3)", 3)]
        [TestCase("'#((list 1 2 3 4) 'a)", 2)]
        public void VectorLength(string expression, int length) {
            WhenEvaluating("(vector-length " + expression + ")");
            ThenIntegerResultIs(length);
        }
    }
}
