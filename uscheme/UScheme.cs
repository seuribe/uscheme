using System;
using System.Collections.Generic;
using System.Globalization;
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
            if (int.TryParse(token, out int intValue)) {
                return new IntegerNumber(intValue);
            }
            if (float.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out float floatValue))
            {
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
        
        public static string Eval(string input, Env env) {
            using (var reader = new StringReader(input)) {
                return Eval(ReadForm(reader), env).ToString();
            }
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
            Exp first = list.First;

            if (first == Symbol.DEFINE)
                return EvalDefine(list.Tail(), env);

            if (first == Symbol.IF)
                return Eval(Boolean.IsTrue(Eval(list.Second, env)) ? list.Third : list.Fourth, env);

            if (first == Symbol.SET)
                return EvalSet(list.Tail(), env);

            if (first == Symbol.LAMBDA)
                return EvalLambda(list.Tail(), env);

            if (first == Symbol.QUOTE)
                return list.Second;
            
            if (first == Symbol.AND)
                return EvalAnd(list.Tail(), env);
            
            if (first == Symbol.OR)
                return EvalOr(list.Tail(), env);
            
            if (first == Symbol.BEGIN)
                return EvalSequential(list.Tail(), env);

            if (first == Symbol.LET)
                return EvalLet(list.Tail(), env);

            var proc = Eval(first, env) as Procedure;
            return proc.Eval(list.Tail(), env);
        }

        private static Exp EvalLambda(UList parameters, Env env) {
            var argNames = (parameters.First as UList).ToStrings();
            var body = parameters.Second;
            return new Procedure(argNames, body, env);
        }

        private static Exp EvalSet(UList parameters, Env env) {
            var name = parameters.First.ToString();
            var value = Eval(parameters.Second, env);
            return env.Find(name).Set(name, value);
        }

        private static Exp EvalDefine(UList parameters, Env env) {
            if (parameters.First is UList)
                return DefineFunc(parameters.First as UList, parameters.Second, env);

            string name = parameters.First.ToString();
            Exp value = Eval(parameters.Second, env);
            return env.Put(name, value);
        }

        private static Exp EvalLet(UList parameters, Env env) {
            var letEnv = new Env(env);
            letEnv.BindDefinitions(parameters.First as UList);
            return Eval(parameters.Second, letEnv);
        }

        private static Exp EvalAnd(UList expressions, Env env) {
            return Boolean.Get(expressions.All(exp => Boolean.IsTrue(Eval(exp, env))));
        }

        private static Exp EvalOr(UList expressions, Env env) {
            return Boolean.Get(expressions.Any(exp => Boolean.IsTrue(Eval(exp, env))));
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
            environment = environment ?? Env.InitialEnv();

            while (true) {
                try {
                    textOut.Write("eval> ");

                    var line = textIn.ReadLine();
                    if (line.Equals("!quit"))
                        break;

                    using (var lineStream = new StringReader(line)) {
                        var expression = Eval(ReadForm(lineStream), environment);
                        textOut.WriteLine(expression.ToString());
                    }
                } catch (IOException) {
                    break;
                } catch (Exception e) {
                    textOut.WriteLine("Error: " + e.Message);
                }
            }
        }
    }
}