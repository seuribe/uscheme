using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace UScheme {
    public class UReader {

        const int EndOfInput = -1;

        public const char ParensOpen = '(';
        public const char BracketOpen = '[';
        public const char ParensClose = ')';
        public const char BracketClose = ']';
        public const char Space = ' ';
        public const char SemiColon = ';';
        public const char DoubleQuote = '"';
        public const char Quote = '\'';
        public const char Slash = '\\';
        public const char Tab = '\t';
        public const char Newline = '\n';
        public const char CarriageReturn = '\r';

        private static readonly char[] AtomEndChars = new char[] {
            Space, Tab, Newline, CarriageReturn, ParensClose, BracketClose };

        private static readonly char[] WhitespaceChars = new char[] {
            Space, Tab, Newline, CarriageReturn
        };

        private static Exp Atom(string token) {
            Tracer.Atom(token);

            if (int.TryParse(token, out int intValue))
                return new IntegerNumber(intValue);
            
            if (float.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out float floatValue))
                return new RealNumber(floatValue);
            
            foreach (Symbol sym in UScheme.KEYWORDS)
                if (sym.Matches(token))
                    return sym;

            if (Boolean.TryParse(token, out Boolean value))
                return value;

            return new Symbol(token);
        }

        static UList ReadList(TextReader input) {
            input.Read(); // consume '('

            var ret = new UList();
            while (!IsParensClose(input.Peek())) {
                ret.Add(ReadForm(input));
                ConsumeSpaces(input);
            }
            if (!IsParensClose(input.Read()))
                throw new ParseException("Expected closing ) or ]");

            return ret;
        }

        public static bool IsParensClose(int ch) {
            return ch == ParensClose || ch == BracketClose;
        }

        public static bool IsParensOpen(int ch) {
            return ch == ParensOpen || ch == BracketOpen;
        }

        static int ConsumeSpaces(TextReader input) {
            int ch = -1;
            while ((ch = input.Peek()) != -1 && WhitespaceChars.Contains(Convert.ToChar(ch)))
                input.Read();

            return ch;
        }

        static string ReadAllUntil(TextReader input, char[] endChars) {
            var sb = new StringBuilder();
            int ch = -1;

            while ((ch = input.Peek()) != -1 && !endChars.Contains(Convert.ToChar(ch))) {
                sb.Append(Convert.ToChar(ch));
                input.Read();
            }

            return sb.ToString();
        }


        static Exp ReadAtom(TextReader input) {
            if (input.Peek() == DoubleQuote)
                return ReadString(input);
            
            var token = ReadAllUntil(input, AtomEndChars);
            return Atom(token);
        }

        static Exp ReadString(TextReader input) {
            input.Read(); // consume '"'

            var sb = new StringBuilder();
            int ch = -1;
            while ((ch = input.Read()) != DoubleQuote) {
                if (ch == Slash)
                    ch = EscapeChar(input.Read());

                sb.Append(Convert.ToChar(ch));
            }
            return new UString(sb.ToString());
        }

        static int EscapeChar(int ch) {
            switch (ch) {
                case 'n':
                    return Newline;
                case 'r':
                    return CarriageReturn;
                case 't':
                    return Tab;
                case DoubleQuote:
                    return DoubleQuote;
                case Slash:
                    return Slash;
                default:
                    throw new ParseException("Invalid character combination \\" + ch);
            }
        }

        public static Exp ReadForm(TextReader input) {

            while (true) {
                int first = input.Peek();
                switch (first) {
                    case SemiColon:
                        DiscardRestOfLine(input);
                        break;
                    case BracketOpen:
                        goto case ParensOpen;
                    case ParensOpen:
                        return ReadList(input);
                    case Quote:
                        input.Read();
                        return new UList { Symbol.QUOTE, ReadForm(input) };
                    default:
                        return ReadAtom(input);
                }
            }
        }

        static void DiscardRestOfLine(TextReader input) {
            while (input.Peek() != Newline)
                input.Read();
        }

        public static void Load(string filename, Env environment) {
            using (var sr = new StreamReader(filename)) {
                while (ConsumeSpaces(sr) != EndOfInput) {
                    var exp = ReadForm(sr);
                    UScheme.Eval(exp, environment);
                }
            }
        }
    }
}