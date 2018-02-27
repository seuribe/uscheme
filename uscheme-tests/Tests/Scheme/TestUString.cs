using NUnit.Framework;

namespace UScheme.Tests {
    [TestFixture]
    public class TestUString : TestEval {

        [TestCase("(string? \"lalala\")")]
        [TestCase("(string? ((lambda () \"lalala\")))")]
        public void IsString(string expression) {
            WhenEvaluating(expression);
            ThenBooleanResultIs(true);
        }

        [TestCase("lala", "lele", "lalalele")]
        [TestCase("", "lele", "lele")]
        [TestCase("lolo", "", "lolo")]
        public void StringAppend(string a, string b, string c) {
            a = SurroundInQuotes(a);
            b = SurroundInQuotes(b);
            c = SurroundInQuotes(c);
            WhenEvaluating("(string-append " + a + " " + b + ")");
            ThenResultIs(c);
        }

        [TestCase("\"lalala\"", 6)]
        [TestCase("(make-string 10)", 10)]
        [TestCase("(string-append \"1234\" \"567\")", 7)]
        public void StringLength(string exp, int length) {
            WhenEvaluating("(string-length " + exp + ")");
            ThenIntegerResultIs(length);
        }

        [Test]
        public void MakeStringWithChar() {
            WhenEvaluating(@"(make-string 5 #\a)");
            ThenStringResultIs("aaaaa");
        }

        string SurroundInQuotes(string str) {
            return "\"" + str + "\"";
        }
    }
}
