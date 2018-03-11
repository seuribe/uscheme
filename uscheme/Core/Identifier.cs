using System;

namespace UScheme {
    public class Identifier : Exp {
        public static readonly Identifier IF = new Identifier("if");
        public static readonly Identifier COND = new Identifier("cond");
        public static readonly Identifier DEFINE = new Identifier("define");
        public static readonly Identifier LAMBDA = new Identifier("lambda");
        public static readonly Identifier SET = new Identifier("set!");
        public static readonly Identifier QUOTE = new Identifier("quote");
        public static readonly Identifier BEGIN = new Identifier("begin");
        public static readonly Identifier LET = new Identifier("let");
        public static readonly Identifier AND = new Identifier("and");
        public static readonly Identifier OR = new Identifier("or");
        public static readonly Identifier ELSE = new Identifier("else");

        public static readonly Identifier[] SyntacticKeywords = new Identifier[] {
            IF, COND, DEFINE, SET, LAMBDA, QUOTE, BEGIN, LET, AND, OR
        };

        public static Identifier From(string token) {
            foreach (Identifier sym in SyntacticKeywords)
                if (sym.Matches(token))
                    return sym;

            return new Identifier(token);
        }

        public readonly string str;

        Identifier(string str) {
            this.str = str;
        }

        public override string ToString() {
            return str;
        }

        public bool Matches(String token) {
            return str.Equals(token);
        }

        public Exp Clone() {
            return this;
        }

        public bool UEquals(Exp other) {
            return other is Identifier && str.Equals((other as Identifier).str);
        }
    }
}