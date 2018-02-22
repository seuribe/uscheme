using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace UScheme {

    public class Parser : CharConstants {

        private static Exp Atom(string token) {
            Tracer.Atom(token);

            if (int.TryParse(token, out int intValue))
                return new IntegerNumber(intValue);
            
            if (float.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out float floatValue))
                return new RealNumber(floatValue);

            if (Boolean.TryParse(token, out Boolean value))
                return value;

            if (IsString(token))
                return new UString(token.Substring(1, token.Length - 2));

            return Symbol.From(token);
        }

        static bool IsString(string token) {
            return token[0] == DoubleQuote;
        }

        public static bool IsParensClose(int ch) {
            return ch == ParensClose || ch == BracketClose;
        }

        public static bool IsParensOpen(int ch) {
            return ch == ParensOpen || ch == BracketOpen;
        }


        public static Exp Parse(string input) {
            var tokenizer = new Tokenizer(input);
            var tokens = tokenizer.Tokens.GetEnumerator();
            tokens.MoveNext();
            return ReadFromTokens(tokens);
        }

        // Expects the enumerator to already be at the first element
        static Exp ReadFromTokens(IEnumerator<string> tokens) {
            var token = tokens.Current;
            tokens.MoveNext();
            if (token == "(") {
                var list = new UList();
                while (tokens.Current != ")") {
                    list.Add(ReadFromTokens(tokens));
                }
                tokens.MoveNext();
                return list;
            }
            if (token == ")")
                throw new ParseException("Misplaced ')'");
            if (token == "'")
                return new UList { Symbol.QUOTE, ReadFromTokens(tokens) };

            return Atom(token);
        }


        public static void Load(string filename, Env environment) {
            using (var sr = new StreamReader(filename)) {
                var input = sr.ReadToEnd();
                var exp = Parse(input);
                UScheme.Eval(exp, environment);
            }
        }
    }
}