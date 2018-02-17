using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace UScheme {
    public class UReader {

        private static Exp Atom(string token) {
            Console.Out.WriteLine("Atom from " + token);
            if (int.TryParse(token, out int intValue)) {
                return new IntegerNumber(intValue);
            }
            if (float.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out float floatValue)) {
                return new RealNumber(floatValue);
            }
            foreach (Symbol sym in UScheme.KEYWORDS) {
                if (sym.Matches(token)) {
                    return sym;
                }
            }
            if ("#t".Equals(token)) {
                return Boolean.TRUE;
            } else if ("#f".Equals(token)) {
                return Boolean.FALSE;
            }

            return new Symbol(token);
        }

        static UList ReadList(TextReader input) {
            var ret = new UList();
            int open = input.Read();
            if (open != '(')
                throw new ParseException("Expected '('");

            int next = input.Peek();
            while (next != ')') {
                ret.Add(ReadForm(input));
                next = ConsumeSpaces(input);
            }
            if (input.Read() != ')')
                throw new ParseException("Expected ')'");

            return ret;
        }

        static int ConsumeSpaces(TextReader input) {
            int ch = input.Peek();
            while (ch != -1 && Char.IsWhiteSpace(Convert.ToChar(ch))) {
                input.Read();
                ch = input.Peek();
            }
            return ch;
        }

        static string ReadAllUntil(TextReader input, char[] endChars) {
            StringBuilder sb = new StringBuilder();
            int ch = input.Peek();

            while (ch != -1 && !endChars.Contains(Convert.ToChar(ch))) {
                sb.Append(Convert.ToChar(ch));
                input.Read();
                ch = input.Peek();
            }
            return sb.ToString();
        }

        private static readonly char[] ATOM_END_CHARS = new char[] { ' ', '\t', '\n', '\r', ')' };

        static Exp ReadAtom(TextReader input) {
            int next = input.Peek();

            if (next == '\"') {
                return ReadString(input);
            }
            string token = ReadAllUntil(input, ATOM_END_CHARS);
            return Atom(token);
        }

        static Exp ReadString(TextReader input) {
            input.Read(); // consume the opening '"'
            StringBuilder sb = new StringBuilder();
            int ch = input.Read();
            while (ch != '\"') {
                if (ch == '\\') {
                    switch (input.Read()) {
                        case 'n':
                            sb.Append('\n');
                            break;
                        case '"':
                            sb.Append('"');
                            break;
                        case '\\':
                            sb.Append('\\');
                            break;
                        case '\r':
                            break;
                        case '\t':
                            sb.Append('\t');
                            break;
                    }
                } else {
                    sb.Append(Convert.ToChar(ch));
                }
                ch = input.Read();
            }
            return new UString(sb.ToString());
        }

        public static Exp ReadForm(TextReader input) {

            while (true) {
                int first = input.Peek();
                switch (first) {
                    case ';':
                        DiscardRestOfLine(input);
                        break;
                    case '(':
                        return ReadList(input);
                    case '\'':
                        input.Read();
                        return new UList { Symbol.QUOTE, ReadForm(input) };
                    default:
                        return ReadAtom(input);
                }
            }
        }

        static void DiscardRestOfLine(TextReader input) {
            while (input.Peek() != '\n') {
                input.Read();
            }
        }

        public static void Load(string filename, Env environment) {
            using (var sr = new StreamReader(filename)) {
                while (ConsumeSpaces(sr) != -1) {
                    var exp = ReadForm(sr);
                    UScheme.Eval(exp, environment);
                }
            }
        }
    }
}