using NUnit.Framework;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;

namespace UScheme.Tests {
    [TestFixture]
    public class TestIO : TestEval {

        string filename;

        [SetUp]
        public new void SetUp() {
            base.SetUp();
            filename = Path.GetTempPath() + Guid.NewGuid().ToString() + ".scm";
        }

        [TearDown]
        public void TearDown() {
            File.Delete(filename);
        }

        [Test]
        public void TestReadFile() {
            var escapedFilename = EscapeSlashes(filename);
            WhenEvaluating("(define filename \"" + escapedFilename + "\")");
            WhenEvaluating("(define of (open-output-file filename))");
            WhenEvaluating("(write '(+ 1 2) of)");
            WhenEvaluating("(close-output-port of)");
            WhenEvaluating("(define if (open-input-file filename))");
            WhenEvaluating("(read if)");
            ThenResultIsExp(Cell.BuildList(Identifier.From("+"), Number1, Number2));
            WhenEvaluating("(close-input-port if)");
        }

        private string EscapeSlashes(string str) {
            return str.Replace("\\", "\\\\");
        }

        /// <summary>
        /// https://stackoverflow.com/questions/323640/can-i-convert-a-c-sharp-string-value-to-an-escaped-string-literal
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string ToLiteral(string input) {
            using (var writer = new StringWriter()) {
                using (var provider = CodeDomProvider.CreateProvider("CSharp")) {
                    provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, null);
                    return writer.ToString();
                }
            }
        }
    }
}
