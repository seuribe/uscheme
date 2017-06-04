using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace UScheme {

    class ParseException : Exception
    {
        private string p;

        public ParseException(string p)
        {
            this.p = p;
        }
    }

    public class Pair {
        Exp car;
        Exp cdr;
    }

    public class Env
    {
        private Dictionary<string, Exp> values = new Dictionary<string, Exp>();

        private Env outer = null;
        public Env() { }

        public static Env InitialEnv()
        {
            Env env = new Env();
            env.Put("+", Number.ADD);
            env.Put("-", Number.SUB);
            env.Put("=", Number.EQUALS);
            env.Put("<", Number.LESSTHAN);
            env.Put("<=", Number.LESSOREQUALTHAN);
            env.Put(">", Number.GREATERTHAN);
            env.Put(">=", Number.GREATEROREQUALTHAN);

            StdLib.AddProcedures(env);

            return env;
        }

        public Exp Eval(Exp exp) {
            return UScheme.Eval(exp, this);
        }

        public string Eval(string input) {
            return UScheme.Eval(input, this);
        }

        public Env(Env outer)
        {
            this.outer = outer;
        }

        public Env Find(string name)
        {
            if (values.ContainsKey(name))
            {
                return this;
            }
            if (outer != null)
            {
                return outer.Find(name);
            }
            throw new Exception("symbol '" + name + "' not found");
        }

        public Exp Get(string name)
        {
            Exp value;
            if (values.TryGetValue(name, out value))
            {
                return value;
            }
            if (outer != null)
            {
                return outer.Get(name);
            }
            throw new Exception("symbol '" + name + "' not found");
        }

        public Exp Put(string name, Exp value)
        {
            values.Add(name, value);
            return value;
        }

        public Exp Set(string name, Exp value)
        {
            values[name] = value;
            return value;
        }
    }

    public interface Exp {
        bool UEquals(Exp other);
    }

    class UList : List<Exp>, Exp {

        public UList() { }

        public UList(IEnumerable<Exp> l) : base(l) { }

        public UList Tail()
        {
            return new UList(this.Skip(1));
        }

        public override string ToString() {
            
            return "(" + string.Join(" ", ToStrings()) + ")";
        }

        public List<string> ToStrings()
        {
            List<string> ret = new List<string>();
            foreach (Exp e in this) {
                ret.Add(e.ToString());
            }
            return ret;
        }

        public bool UEquals(Exp other) {
            if (!(other is UList)) {
                return false;
            }
            UList b = other as UList;
            if (Count != b.Count) {
                return false;
            }
            for (int i = 0 ; i < Count ; i++) {
                if (!this[i].UEquals(b[i])) {
                    return false;
                }
            }
            return true;
        }
    }

    class UString : Exp {
        public readonly string str;
        public UString(string str) {
            this.str = str;
        }
        public override string ToString() {
            return "\"" + str + "\"";
        }
        public bool UEquals(Exp other) {
            return (other is UString) && str.Equals((other as UString).str);
        }

    }

    class Symbol : Exp {
        protected readonly string str;
        public Symbol(string str) {
            this.str = str;
        }

        public override string ToString() {
            return str;
        }

        public bool matches(String token) {
            return str.Equals(token);
        }

        public bool UEquals(Exp other) {
            return other is Symbol && str.Equals((other as Symbol).str);
        }

        public static readonly Symbol IF = new Symbol("if");
//        public static readonly Symbol COND = new Symbol("cond");
        public static readonly Symbol DEFINE = new Symbol("define");
        public static readonly Symbol LAMBDA = new Symbol("lambda");
        public static readonly Symbol SET = new Symbol("set!");
        public static readonly Symbol QUOTE = new Symbol("quote");
        public static readonly Symbol BEGIN = new Symbol("begin");
        public static readonly Symbol LET = new Symbol("let");
        public static readonly Symbol AND = new Symbol("and");
        public static readonly Symbol OR = new Symbol("or");
    }


    class Procedure : Exp {
        private List<string> args;
        private Exp body;
        private Env env;

        public delegate Exp EvalProc(UList args, Env env);

        private EvalProc evalProc;

        public Procedure(List<string> args, Exp body, Env env)
        {
            this.args = args;
            this.body = body;
            this.env = env;
            this.evalProc = DefaultEval;
        }

        public Procedure(EvalProc evalProc)
        {
            this.evalProc = evalProc;
        }

        public Exp Eval(UList values, Env env)
        {
            return evalProc(values, env);
        }

        private Exp DefaultEval(UList values, Env outerEnv)
        {
            Console.Out.WriteLine("Eval proc with " + args.Count + " params using " + values.Count + " values");
            Env evalEnv = new Env(outerEnv);
            for (int i = 0; i < args.Count; i++)
            {
                evalEnv.Put(args[i], UScheme.Eval(values[i], outerEnv));
            }
            return UScheme.Eval(body, evalEnv);
        }

        public bool UEquals(Exp other) {
            if (other == this) {
                return true;
            }
            return other is Procedure &&
                body.UEquals((other as Procedure).body) &&
                args.Equals((other as Procedure).args);
        }

    }

    // TODO: define-syntax, strings, Vector3f
    public class UScheme {
        private static readonly Symbol[] KEYWORDS = new Symbol[] {
            Symbol.IF,
            Symbol.DEFINE,
            Symbol.SET,
            Symbol.LAMBDA,
            Symbol.QUOTE,
            Symbol.BEGIN,
            Symbol.LET,
            Symbol.AND,
            Symbol.OR,
        };
/*
        private static List<string> Tokenize(string str) {
            Console.Out.WriteLine("Tokenize " + str);
            string[] tokens = str.Replace("(", " ( ").Replace(")", " ) ").Replace("'", "' ").Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < tokens.Length; i++) {
                tokens[i] = tokens[i].Trim();
            }
            return new List<string>(tokens);
        }
*/
        private static Exp Atom(string token) {
            Console.Out.WriteLine("Atom from " + token);
            int intValue;
            if (int.TryParse(token, out intValue)) {
                return new IntegerNumber(intValue);
            }
            float floatValue;
            if (float.TryParse(token, out floatValue)) {
                return new RealNumber(floatValue);
            }
            foreach (Symbol sym in KEYWORDS) {
                if (sym.matches(token)) {
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
/*
        private static Exp ReadList(List<string> tokens) {
            ExpList l = new ExpList();
            while (!tokens[0].Equals(")")) {
                l.Add(ReadTokens(tokens));
            }
            tokens.RemoveAt(0);
            return l;
        }
 */ 
/*
        private static Exp ReadTokens(List<string> tokens) {
            if (tokens.Count == 0) {
                throw new ParseException("Empty list");
            }
            string token = tokens.First();
            tokens.RemoveAt(0);

            if (token == "'") {
                ExpList l = new ExpList();
                l.Add(Symbol.QUOTE);
                l.Add(ReadTokens(tokens));
                return l;
            }

            if (token.Equals("(")) {
                return ReadList(tokens);
            } else if (token.Equals(")")) {
                throw new ParseException("Unexpected ')'");
            } else {
                return Atom(token);
            }
        }
        */
        static UList ReadList(System.IO.TextReader input) {
            UList ret = new UList();
            int open = input.Read();
            if (open != '(') {
                throw new ParseException("Expected '('");
            }
            int next = input.Peek();
            while (next != ')') {
                ret.Add(ReadForm(input));
                next = ConsumeSpaces(input);
            }
            if (input.Read() != ')') {
                throw new ParseException("Expected ')'");
            }
            return ret;
        }

        static int ConsumeSpaces(System.IO.TextReader input) {
            int ch = input.Peek();
            while (ch != -1 && Char.IsWhiteSpace(Convert.ToChar(ch))) {
                input.Read();
                ch = input.Peek();
            }
            return ch;
        }

        static string ReadAllUntil(System.IO.TextReader input, char[] endChars) {
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

        static Exp ReadAtom(System.IO.TextReader input) {
            int next = input.Peek();

            if (next == '\"') {
                return ReadString(input);
            }
            string token = ReadAllUntil(input, ATOM_END_CHARS);
            return Atom(token);
        }

        static Exp ReadString(System.IO.TextReader input) {
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

        static Exp ReadForm(System.IO.TextReader input) {

            int first = input.Peek();
            switch (first) {
                case '(':
                    return ReadList(input);
                case '\'':
                    input.Read();
                    UList l = new UList();
                    l.Add(Symbol.QUOTE);
                    l.Add(ReadForm(input));
                    return l;
                default:
                    return ReadAtom(input);
            }
/*
            Console.Out.WriteLine("Read " + input);
            return ReadTokens(Tokenize(input));
*/
        }

        static Exp Car(UList list)
        {
            return list[0];
        }

        static Exp Cdr(UList list)
        {
            return list.Tail();
        }

        public static string Eval(string input)
        {
            return Eval(input, Env.InitialEnv());
        }

        public static string Eval(string input, Env env) {

            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(input);
            writer.Flush();
            stream.Position = 0;

            return Eval(ReadForm(new StreamReader(stream)), env).ToString();
        }

        public static Exp Eval(Exp exp, Env env) {
            Console.Out.WriteLine("Eval " + exp.ToString());

            if (!(exp is UList)) {
                if (exp is Symbol) {    // env-defined variables
                    return env.Get(exp.ToString());
                }
                return exp; // atoms like integer, float, etc.
            }
            // from here on, it's a list
            UList list = exp as UList;

            Exp first = list[0];

            if (first == Symbol.DEFINE) {
                if (list[1] is UList) {
                    UList defSig = list[1] as UList;
                    string name = defSig[0].ToString();
                    List<string> args = defSig.Tail().ToStrings();
                    Exp body = list[2];
                    return env.Put(name, new Procedure(args, body, env));
                } else {
                    string var = list[1].ToString();
                    Exp value = Eval(list[2], env);
                    return env.Put(var, value);
                }
            }
            if (first == Symbol.IF) {
                Exp test = Eval(list[1], env);
                return Eval(Boolean.IsTrue(test) ? list[2] : list[3], env);
            }
            if (first == Symbol.SET) {
                string var = list[1].ToString();
                Exp val = Eval(list[2], env);
                Console.Out.WriteLine("set! '" + var + "' <- " + val.ToString());
                return env.Find(var).Set(var, val);
            }
            if (first == Symbol.LAMBDA) {
                List<string> args = (list[1] as UList).ToStrings();
                Exp body = list[2];
                return new Procedure(args, body, env);
            }
            if (first == Symbol.QUOTE) {
                return list[1];
            }
            if (first == Symbol.AND) {
                return AND(list.Tail(), env);
            }
            if (first == Symbol.OR) {
                return OR(list.Tail(), env);
            }
            if (first == Symbol.BEGIN) {
                Exp ret = null;
                for (int i = 1; i < list.Count; i++) {
                    ret = Eval(list[i], env);
                }
                return ret;
            }
            if (first == Symbol.LET) {
                UList defs = list[1] as UList;
                Exp body = list[2];
                Env letEnv = new Env(env);

                foreach (Exp def in defs) {
                    UList defList = def as UList;
                    letEnv.Put(defList[0].ToString(), Eval(defList[1], env));
                }
                return Eval(body, letEnv);
            }

            // procedure call
            Procedure proc = Eval(first, env) as Procedure;
            return proc.Eval(list.Tail(), env);
        }

        private static Exp AND(UList l, Env env) {
            Exp val = Boolean.TRUE;
            foreach (Exp exp in l) {
                if (Boolean.IsFalse(val = Eval(exp, env))) {
                    break;
                }
            }
            return val;
        }
        private static Exp OR(UList l, Env env) {
            Exp val = Boolean.FALSE;
            foreach (Exp exp in l) {
                if (Boolean.IsTrue(val = Eval(exp, env))) {
                    break;
                }
            }
            return val;
        }

        public static Env Load(string filename) {
            Env env = Env.InitialEnv();
            StreamReader sr = new StreamReader(filename);
            while (ConsumeSpaces(sr) != -1) {
                Exp exp = ReadForm(sr);
                Eval(exp, env);
            }
            return env;
        }

        public static void Loop(System.IO.TextReader textIn, System.IO.TextWriter textOut) {
            Env env = Env.InitialEnv();
            textOut.Write("eval> ");
            while (true) {
                try {
                    int p = textIn.Peek();
                    if (p == '\n' || p == '\r') {
                        textIn.Read();
                        continue;
                    }

                    if (p == -1) {
                        continue;
                    }

                    Exp ret = Eval(ReadForm(textIn), env);
                    textOut.WriteLine(ret.ToString());
                    textOut.Write("eval> ");
                } catch (Exception e) {
                    textOut.WriteLine("Error: " + e.Message);
                }
            }
        }
    }
}