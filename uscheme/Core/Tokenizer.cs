using System;
using System.Collections.Generic;
using System.Text;

namespace UScheme {
    public class Tokenizer : CharConstants {
        
        private static readonly int[] AtomEndChars = new int[] {
            Space, Tab, Newline, CarriageReturn, ParensClose, BracketClose };

        private static readonly int[] WhitespaceChars = new int[] {
            Space, Tab, Newline, CarriageReturn
        };

        public List<string> Tokens { private set; get; }

        public Tokenizer(string input) {
            Tokens = new List<string>();
            ProcessInput(input);
        }

        void ProcessInput(string input) {
            var reader = new SimpleStringReader(input);

            while (reader.Current != -1) {
                if (reader.Current == BracketOpen || reader.Current == ParensOpen)
                    Emit(ParensOpen);
                else if (reader.Current == BracketClose || reader.Current == ParensClose)
                    Emit(ParensClose);
                else if (reader.Current == SemiColon)
                    DiscardUntil(reader, Newline);
                else if (reader.Current == Quote)
                    Emit(Quote);
                else if (reader.Current == DoubleQuote)
                    Emit(ReadString(reader));
                else if (reader.Current == Hash)
                    Emit(Hash);
                else if (IsWhitespace(reader.Current)) {
                    DiscardWhitespace(reader);
                    continue;
                } else {
                    Emit(ReadAtom(reader));
                    continue;
                }

                reader.Advance();
            }
        }

        public static void DiscardWhitespace(SimpleStringReader reader) {
            while (reader.Available && IsWhitespace(reader.Current))
                reader.Advance();
        }

        public static string ReadAtom(SimpleStringReader reader) {
            var buffer = new StringBuilder();
            while (reader.Available && !IsAtomEnd(reader.Current)) {
                buffer.Append(Convert.ToChar(reader.Current));
                reader.Advance();
            }
            return buffer.ToString();
        }

        public static bool IsWhitespace(int ch) {
            return Array.IndexOf(WhitespaceChars, ch) != -1;
        }

        public static bool IsAtomEnd(int ch) {
            return Array.IndexOf(AtomEndChars, ch) != -1;
        }

        public static string ReadString(SimpleStringReader reader) {
            var buffer = new StringBuilder();

            // Add quote and advance
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

        static void DiscardUntil(SimpleStringReader input, int ch) {
            while (input.Available && input.Current != ch)
                input.Advance();
        }

        void Emit(char ch) {
            Emit("" + ch);
        }

        void Emit(string token) {
            Tokens.Add(token);
        }
    }
}