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

                if (CanEvaluateString())
                    ProcessBuffer();
            }
        }

        void ProcessBuffer() {
            try {

                var expression = Parser.Parse(buffer.ToString());
                var result = UScheme.Eval(expression, environment);
                textOut.WriteLine(result.ToString());
/*
                using (var lineStream = new StringReader(buffer.ToString())) {
                    var form = UReader.ReadForm(lineStream);
                    var expression = UScheme.Eval(form, environment);
                    textOut.WriteLine(expression.ToString());
                }
*/
            } catch (UException e) {
                textOut.WriteLine("Error: " + e.Message);
            } finally {
                buffer.Clear();
            }
        }

        bool CanEvaluateString() {
            var chars = buffer.ToString().ToCharArray();

            return HasBalancedParens(chars) &&
                   (IsQuote(chars) || StartAndEndCoherentParens(chars));
        }

        bool StartAndEndCoherentParens(char[] chars) {
            return Parser.IsParensOpen(chars[0]) == Parser.IsParensClose(chars[chars.Length - 1]);
        }

        bool IsQuote(char[] chars) {
            return chars[0] == Parser.Quote;
        }

        bool HasBalancedParens(char[] chars) {
            int openParens = 0;

            for (int i = 0 ; i < chars.Length && openParens >= 0 ; i++)
                if (Parser.IsParensOpen(chars[i]))
                    openParens++;
                else if (Parser.IsParensClose(chars[i]))
                    openParens--;

            return openParens == 0;
        }
    }
}
