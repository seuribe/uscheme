﻿using System;
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

            return Identifier.From(token);
        }

        public static Exp Parse(string input) {
            var tokenizer = new Tokenizer(input);
            var tokens = tokenizer.Tokens.GetEnumerator();
            tokens.MoveNext();
            return ReadThunk(tokens);
        }

        static Exp ReadThunk(IEnumerator<string> tokens) {
            var forms = new List<Exp>();
            try {
                while (true)
                    forms.Add(ReadNextForm(tokens));
            } catch (Exception) {
            }

            return (forms.Count == 1) ? forms[0] : new Sequence(forms);
        }

        // Expects the enumerator to already be at the first element
        static Exp ReadNextForm(IEnumerator<string> tokens) {
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
                    return Vector.FromList(list);
                } else if (tokens.Current == "vu8") {
                    tokens.MoveNext(); // 'vu8'
                    if (tokens.Current != "(")
                        throw new ParseException("expected '(' after #vu8");
                    tokens.MoveNext(); // '('
                    var list = ReadUntilClosingParens(tokens);
                    tokens.MoveNext();
                    return ByteVector.FromList(list);
                } else { // append next token for dealing with #t & #f
                    token += tokens.Current;
                    tokens.MoveNext();
                }

            if (token == ")")
                throw new ParseException("Misplaced ')'");
            if (token == "'")
                return Cell.BuildList(Identifier.QUOTE, ReadNextForm(tokens));

            return Atom(token);
        }

        static void ExpectAndSkip(IEnumerator<string> tokens, string expected) {
            if (tokens.Current != expected)
                throw new ParseException("Expected " + expected + " but instead found " + tokens.Current);

        }

        static List<Exp> ReadUntilClosingParens(IEnumerator<string> tokens) {
            var list = new List<Exp>();
            while (tokens.Current != ")") {
                list.Add(ReadNextForm(tokens));
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