using System;
using System.Collections.Generic;
using System.Text;

namespace UScheme {
    public class Tokenizer : CharConstants {

        public List<string> Tokens { private set; get; }

        public Tokenizer(string input) {
            Tokens = new List<string>();
            ProcessInput(input);
        }

        Dictionary<int, String> Conversions = new Dictionary<int, string> {
            { ParensOpen, "(" },
            { BracketOpen, "(" },
            { ParensClose, ")" },
            { BracketClose, ")" },
            { Quote, "'" },
            { Dot, "." },
            { Hash, "#" }
        };

        void ProcessInput(string input) {
            var reader = new SimpleStringReader(input);

            while (reader.Current != -1) {
                if (reader.Current == SemiColon)
                    DiscardWhile(reader, IsNotNewline);
                else if (Conversions.ContainsKey(reader.Current))
                    Emit(Conversions[reader.Current]);
                else if (reader.Current == DoubleQuote)
                    Emit(ReadString(reader));
                else if (IsWhitespace(reader.Current)) {
                    DiscardWhile(reader, IsWhitespace);
                    continue;
                } else {
                    Emit(ReadAtom(reader));
                    continue;
                }

                reader.Advance();
            }
        }

        static void DiscardWhile(SimpleStringReader reader, Func<int, bool> predicate) {
            while (reader.Available && predicate(reader.Current))
                reader.Advance();
        }

        static string ReadAtom(SimpleStringReader reader) {
            var buffer = new StringBuilder();
            while (reader.Available && !IsAtomEnd(reader.Current)) {
                buffer.Append(Convert.ToChar(reader.Current));
                reader.Advance();
            }
            return buffer.ToString();
        }

        static string ReadString(SimpleStringReader reader) {
            var buffer = new StringBuilder();

            buffer.Append(DoubleQuote);
            reader.Advance();

            while (reader.Current != DoubleQuote) {
                int ch = reader.Current;
                if (ch == -1)
                    throw new ParseException("Unexpectic EOF while reading string '" + buffer.ToString() + "'");

                if (ch == Slash) {
                    reader.Advance();
                    ch = EscapeChar(reader.Current);
                }
                buffer.Append(Convert.ToChar(ch));

                reader.Advance();
            }
            buffer.Append(DoubleQuote);

            return buffer.ToString();
        }

        static char EscapeChar(int ch) {
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

        void Emit(string token) {
            Tokens.Add(token);
        }
    }
}