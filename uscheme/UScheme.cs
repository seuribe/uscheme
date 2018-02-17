using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace UScheme {

    // TODO: define-syntax, strings
    public class UScheme {

        public static readonly Symbol[] KEYWORDS = new Symbol[] {
            Symbol.IF,
            Symbol.COND,
            Symbol.DEFINE,
            Symbol.SET,
            Symbol.LAMBDA,
            Symbol.QUOTE,
            Symbol.BEGIN,
            Symbol.LET,
            Symbol.AND,
            Symbol.OR,
        };

        public static string Eval(string input, Env env) {
            using (var reader = new StringReader(input)) {
                return Eval(UReader.ReadForm(reader), env).ToString();
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
            return env.Bind(name, new Procedure(paramNames, body, env));
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

            if (first == Symbol.COND)
                return EvalCond(list.Tail(), env);

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

        private static Exp EvalCond(UList parameters, Env env) {
            for (int i = 0; i < parameters.Count/2; i++) {
                var condition = parameters[i * 2];
                if ((Eval(condition, env) as Boolean).Value)
                    return Eval(parameters[i * 2 + 1], env);
            }
            return Boolean.FALSE;
        }

        private static Exp EvalLambda(UList parameters, Env env) {
            var argNames = (parameters.First as UList).ToStrings();
            var body = parameters.Second;
            return new Procedure(argNames, body, env);
        }

        private static Exp EvalSet(UList parameters, Env env) {
            var name = parameters.First.ToString();
            var value = Eval(parameters.Second, env);
            return env.Find(name).Bind(name, value);
        }

        private static Exp EvalDefine(UList parameters, Env env) {
            if (parameters.First is UList)
                return DefineFunc(parameters.First as UList, parameters.Second, env);

            string name = parameters.First.ToString();
            Exp value = Eval(parameters.Second, env);
            return env.Bind(name, value);
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

        public static void Repl(TextReader textIn, TextWriter textOut, Env environment) {
            while (true) {
                try {
                    textOut.Write("uscheme > ");

                    var line = textIn.ReadLine();
                    if (line.Equals("!quit"))
                        break;

                    using (var lineStream = new StringReader(line)) {
                        var expression = Eval(UReader.ReadForm(lineStream), environment);
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