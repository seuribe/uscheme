using System;
using System.IO;
using System.Text;

namespace UScheme {
    public class REPL {
        readonly TextReader textIn;
        readonly TextWriter textOut;
        readonly Env environment;

        bool exit = false;
        StringBuilder buffer = new StringBuilder();

        public REPL(TextReader textIn, TextWriter textOut, Env environment) {
            this.textIn = textIn;
            this.textOut = textOut;
            this.environment = environment;
        }

        bool ProcessCommand(string line) {
            if (line.Equals("!quit"))
                exit = true;
            else if (line.Equals("!buffer"))
                textOut.WriteLine(buffer.ToString());
            else if (line.Equals("!clear"))
                buffer.Clear();
            else
                return false;

            return true;
        }

        public void Start() {
            while (!exit) {
                textOut.Write("uscheme > ");

                var line = textIn.ReadLine();
                if (ProcessCommand(line))
                    continue;

                buffer.Append(line);

                if (BufferParensMatch())
                    ProcessBuffer();
            }
        }

        void ProcessBuffer() {
            try {
                using (var lineStream = new StringReader(buffer.ToString())) {
                    var form = UReader.ReadForm(lineStream);
                    var expression = UScheme.Eval(form, environment);
                    textOut.WriteLine(expression.ToString());
                }
            } catch (UException e) {
                textOut.WriteLine("Error: " + e.Message);
            } finally {
                buffer.Clear();
            }
        }

        bool BufferParensMatch() {
            var str = buffer.ToString();
            var chars = str.ToCharArray();
            int openParens = 0;

            for (int i = 0 ; i < chars.Length && openParens >= 0 ; i++) {
                if (chars[i] == '(')
                    openParens++;
                else if (chars[i] == ')')
                    openParens--;
            }

            var firstIsQuote = chars[0] == '\'';
            var firstIsOpenParens = chars[0] == '(';
            var lastIsCloseParens = chars[chars.Length - 1] == ')';

            return openParens == 0 && (firstIsQuote || (firstIsOpenParens == lastIsCloseParens));
        }
    }
}
