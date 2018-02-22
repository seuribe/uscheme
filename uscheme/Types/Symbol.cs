using System;

namespace UScheme {
    public class Symbol : Exp {
        public static readonly Symbol IF = new Symbol("if");
        public static readonly Symbol COND = new Symbol("cond");
        public static readonly Symbol DEFINE = new Symbol("define");
        public static readonly Symbol LAMBDA = new Symbol("lambda");
        public static readonly Symbol SET = new Symbol("set!");
        public static readonly Symbol QUOTE = new Symbol("quote");
        public static readonly Symbol BEGIN = new Symbol("begin");
        public static readonly Symbol LET = new Symbol("let");
        public static readonly Symbol AND = new Symbol("and");
        public static readonly Symbol OR = new Symbol("or");

        public static readonly Symbol[] ReservedSymbols = new Symbol[] {
            IF, COND, DEFINE, SET, LAMBDA, QUOTE, BEGIN, LET, AND, OR
        };

        public static Symbol From(string token) {
            foreach (Symbol sym in ReservedSymbols)
                if (sym.Matches(token))
                    return sym;

            return new Symbol(token);
        }

        readonly string str;

        Symbol(string str) {
            this.str = str;
        }

        public override string ToString() {
            return str;
        }

        public bool Matches(String token) {
            return str.Equals(token);
        }

        public bool UEquals(Exp other) {
            return other is Symbol && str.Equals((other as Symbol).str);
        }
    }
}