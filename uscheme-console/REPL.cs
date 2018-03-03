using System;
using System.IO;
using System.Text;

namespace UScheme {
    public class REPL : CharConstants {
        readonly TextReader textIn;
        readonly TextWriter textOut;
        readonly Env environment;
        readonly StringBuilder buffer = new StringBuilder();

        bool exit = false;

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
                if (result != null)
                    textOut.WriteLine(result.ToString());
            } catch (UException e) {
                textOut.WriteLine("Error: " + e.Message);
            } finally {
                buffer.Clear();
            }
        }

        bool CanEvaluateString() {
            var chars = buffer.ToString().ToCharArray();
            return HasBalancedParens(chars) &&
                   (IsQuoted(chars) || StartAndEndCoherentParens(chars));
        }

        bool StartAndEndCoherentParens(char[] chars) {
            return IsParensOpen(chars[0]) == IsParensClose(chars[chars.Length - 1]);
        }

        bool HasBalancedParens(char[] chars) {
            int openParens = 0;

            for (int i = 0 ; i < chars.Length && openParens >= 0 ; i++)
                if (IsParensOpen(chars[i]))
                    openParens++;
                else if (IsParensClose(chars[i]))
                    openParens--;

            return openParens == 0;
        }
    }
}
