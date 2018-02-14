using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace UScheme {

    // TODO: define-syntax, strings
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

        static UList ReadList(TextReader input) {
            var ret = new UList();
            int open = input.Read();
            if (open != '(')
                throw new ParseException("Expected '('");
            
            int next = input.Peek();
            while (next != ')') {
                ret.Add(ReadForm(input));
                next = ConsumeSpaces(input);
            }
            if (input.Read() != ')')
                throw new ParseException("Expected ')'");
            
            return ret;
        }

        static int ConsumeSpaces(TextReader input) {
            int ch = input.Peek();
            while (ch != -1 && Char.IsWhiteSpace(Convert.ToChar(ch))) {
                input.Read();
                ch = input.Peek();
            }
            return ch;
        }

        static string ReadAllUntil(TextReader input, char[] endChars) {
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

        static Exp ReadAtom(TextReader input) {
            int next = input.Peek();

            if (next == '\"') {
                return ReadString(input);
            }
            string token = ReadAllUntil(input, ATOM_END_CHARS);
            return Atom(token);
        }

        static Exp ReadString(TextReader input) {
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
            using (var sr = new StreamReader(filename)) {
                while (ConsumeSpaces(sr) != -1) {
                    var exp = ReadForm(sr);
                    Eval(exp, env);
                }
            }
            return env;
        }

        public static void Loop(TextReader textIn, TextWriter textOut, Env environment = null) {
            var env = environment ?? Env.InitialEnv();
            bool exit = false;
            while (!exit) {
                try {
                    textOut.Write("eval> ");
                    var lineStream = new StringReader(textIn.ReadLine());
                    var expression = Eval(ReadForm(lineStream), env);
                    textOut.WriteLine(expression.ToString());
                } catch (IOException) {
                    exit = true;
                } catch (Exception e) {
                    textOut.WriteLine("Error: " + e.Message);
                }
            }
        }
    }
}