using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace UScheme {

    public class Parser : CharConstants {

        private static Exp Atom(string token) {
            Tracer.Atom(token);

            if (Number.TryParse(token, out Number number))
                return number;

            if (Boolean.TryParse(token, out Boolean boolean))
                return boolean;

            if (Character.TryParse(token, out Character ch))
                return ch;

            if (UString.TryParse(token, out UString strValue))
                return strValue;

            return Symbol.From(token);
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
                var list = ReadUntilClosingParens(tokens);
                tokens.MoveNext();
                return Cell.BuildList(list);
            }
            if (token == "#")
                if (tokens.Current == "(") {
                    tokens.MoveNext(); // '('
                    var list = ReadUntilClosingParens(tokens);
                    tokens.MoveNext();
                    return new Vector(list);
                } else { // append next token for dealing with #t & #f
                    token += tokens.Current;
                    tokens.MoveNext();
                }

            if (token == ")")
                throw new ParseException("Misplaced ')'");
            if (token == "'")
                return Cell.BuildList(Symbol.QUOTE, ReadFromTokens(tokens));

            return Atom(token);
        }

        static List<Exp> ReadUntilClosingParens(IEnumerator<string> tokens) {
            var list = new List<Exp>();
            while (tokens.Current != ")") {
                list.Add(ReadFromTokens(tokens));
            }
            return list;
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