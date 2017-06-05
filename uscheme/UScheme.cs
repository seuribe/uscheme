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

        public UList Tail() {
            return new UList(this.Skip(1));
        }

        public override string ToString() {
            return "(" + string.Join(" ", ToStrings()) + ")";
        }

        public List<string> ToStrings() {
            return this.Select(e => e.ToString()).ToList();
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

        protected EvalProc evalProc;

        public Procedure() { }

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

        public Exp Eval(Exp value, Env env) {
            return evalProc(new UList { value }, env);
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

        static Exp ReadForm(TextReader input) {

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
                        UList l = new UList();
                        l.Add(Symbol.QUOTE);
                        l.Add(ReadForm(input));
                        return l;
                    default:
                        return ReadAtom(input);
                }
            }
/*
            Console.Out.WriteLine("Read " + input);
            return ReadTokens(Tokenize(input));
*/
        }

        static void DiscardRestOfLine(TextReader input) {
            while (input.Peek() != '\n') {
                input.Read();
            }
        }
        
        public static string Eval(string input) {
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

            if (exp is Symbol)    // env-defined variables
                return env.Get(exp.ToString());

            if (exp is UList)
                return EvalList(exp as UList, env);

            return exp; // atoms like integer, float, etc.
        }

        static Exp DefineFunc(UList head, Exp body, Env env) {
            var name = head[0].ToString();
            var paramNames = head.Tail().ToStrings();
            return env.Put(name, new Procedure(paramNames, body, env));
        }

        static Exp EvalSequential(UList expressions, Env env) {
            Exp ret = null;
            foreach (var e in expressions)
                ret = Eval(e, env);

            return ret;
        }

        static Exp EvalList(UList list, Env env) {
            Exp first = list[0];

            if (first == Symbol.DEFINE) {
                if (list[1] is UList) {
                    return DefineFunc(list[1] as UList, list[2], env);
                } else {
                    string var = list[1].ToString();
                    Exp value = Eval(list[2], env);
                    return env.Put(var, value);
                }
            }

            if (first == Symbol.IF)
                return Eval(Boolean.IsTrue(Eval(list[1], env)) ? list[2] : list[3], env);

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

            if (first == Symbol.QUOTE)
                return list[1];
            
            if (first == Symbol.AND)
                return AND(list.Tail(), env);
            
            if (first == Symbol.OR)
                return OR(list.Tail(), env);
            
            if (first == Symbol.BEGIN)
                return EvalSequential(list.Tail(), env);

            if (first == Symbol.LET)
                return Eval(list[2], SubEnv(list[1] as UList, env));

            // procedure call
            var proc = Eval(first, env) as Procedure;
            return proc.Eval(list.Tail(), env);
        }

        static Env SubEnv(UList definitions, Env env) {
            var subEnv = new Env(env);
            foreach (UList def in definitions)
                subEnv.Put(def[0].ToString(), Eval(def[1], env));

            return subEnv;
        }

        private static Exp AND(UList expressions, Env env) {
            return
                Boolean.Get(expressions.All(exp => Boolean.IsTrue(Eval(exp, env))));
        }

        private static Exp OR(UList expressions, Env env) {
            return
                Boolean.Get(expressions.Any(exp => Boolean.IsTrue(Eval(exp, env))));
        }

        public static Env Load(string filename) {
            var env = Env.InitialEnv();
            var sr = new StreamReader(filename);
            while (ConsumeSpaces(sr) != -1) {
                var exp = ReadForm(sr);
                Eval(exp, env);
            }
            return env;
        }

        public static void Loop(TextReader textIn, TextWriter textOut) {
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