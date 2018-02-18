using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UScheme {

    public delegate Exp EvalProc(UList argumentValues, Env env);

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
            Tracer.Eval(exp);

            if (exp is Symbol)    // env-defined variables
                return env.Get(exp.ToString());

            if (exp is UList)
                return EvalList(exp as UList, env);

            return exp; // atoms like integer, float, etc.
        }

        static Exp EvalSequential(UList expressions, Env env) {
            Exp ret = null;
            foreach (var e in expressions)
                ret = Eval(e, env);

            return ret;
        }

        static Dictionary<Exp, EvalProc> predefinedProcedures
            = new Dictionary<Exp, EvalProc> {
                { Symbol.DEFINE, EvalDefine },
                { Symbol.COND, EvalCond},
                { Symbol.SET, EvalSet },
                { Symbol.LAMBDA, EvalLambda },
                { Symbol.AND, EvalAnd },
                { Symbol.OR, EvalOr },
                { Symbol.BEGIN, EvalSequential},
                { Symbol.LET, EvalLet},
                { Symbol.IF, EvalIf },
                { Symbol.QUOTE, EvalQuote },
            };

        static Exp EvalList(UList list, Env env) {
            Exp first = list.First;

            if (predefinedProcedures.TryGetValue(first, out EvalProc evalProc))
                return evalProc(list.Tail(), env);

            var evaluatedParameters = EvalEach(list, env);
            var procedure = evaluatedParameters.First as Procedure;
            return procedure.Eval(evaluatedParameters.Tail());
        }

        private static UList EvalEach(UList parameters, Env env) {
            return new UList(parameters.Select(e => UScheme.Eval(e, env)));
        }

        private static Exp EvalQuote(UList parameters, Env env) {
            return parameters.First;
        }

        private static Exp EvalIf(UList parameters, Env env) {
            return Eval(Boolean.IsTrue(Eval(parameters.First, env)) ? parameters.Second : parameters.Third, env);
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

        private static Exp DefineFunc(UList defineParameters, Exp body, Env env) {
            var name = defineParameters.First.ToString();
            var procParameters = defineParameters.Tail().ToStrings();
            return env.Bind(name, new Procedure(procParameters, body, env));
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
    }
}