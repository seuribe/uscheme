using NUnit.Framework;

namespace UScheme.Tests {
    [TestFixture]
    public class TestChar : TestEval {

        [TestCase(@"#\t", 116)]
        [TestCase(@"#\a", 97)]
        public void CharToInteger(string ch, int value) {
            WhenEvaluating("(char->integer " + ch + ")");
            ThenIntegerResultIs(value);
        }
    }
}
