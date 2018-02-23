using System.Collections.Generic;
using System.Linq;

namespace UScheme {

    public delegate Exp ProcedureBody(Cell argumentList);
    public delegate Exp InternalForm(Cell argumentList, Env env);

    public class UScheme {

        public static string Eval(string input, Env env) {
            return Eval(Parser.Parse(input), env).ToString();
        }

        public static Exp Eval(Exp exp, Env env) {
            Tracer.Eval(exp);

            if (exp is Symbol)    // env-defined variables
                return env.Get(exp.ToString());

            if (exp is Cell)
                return EvalList(exp as Cell, env);

            return exp; // atoms like integer, float, etc.
        }

        static Exp EvalSequential(Cell expressions, Env env) {
            Exp ret = null;
            foreach (var e in expressions.Iterate())
                ret = Eval(e, env);

            return ret;
        }

        static Dictionary<Exp, InternalForm> predefinedProcedures
            = new Dictionary<Exp, InternalForm> {
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

        static Exp EvalList(Cell list, Env env) {
            if (predefinedProcedures.TryGetValue(list.First, out InternalForm internalForm))
                return internalForm(list.Rest(), env);

            var evaluatedParameters = EvalEach(list, env);
            var procedure = evaluatedParameters.First as Procedure;
            return procedure.Apply(evaluatedParameters.Rest());
        }

        private static Cell EvalEach(Cell parameters, Env env) {
            var list = new List<Exp>();
            foreach (var exp in parameters.Iterate())
                list.Add(Eval(exp, env));

            return Cell.BuildList(list);
        }

        private static Exp EvalQuote(Cell parameters, Env env) {
            return parameters.First;
        }

        private static Exp EvalIf(Cell parameters, Env env) {
            return Eval(Boolean.IsTrue(Eval(parameters.First, env)) ? parameters.Second : parameters.Third, env);
        }

        private static Exp EvalCond(Cell parameters, Env env) {
            for (int i = 0; i < parameters.Length()/2; i++) {
                var condition = parameters[i * 2];
                if ((Eval(condition, env) as Boolean).Value)
                    return Eval(parameters[i * 2 + 1], env);
            }
            return Boolean.FALSE;
        }

        private static Exp EvalLambda(Cell parameters, Env env) {
            var argNames = (parameters.First as Cell).ToStringList();
            var body = parameters.Second;
            return new Procedure(argNames, body, env);
        }

        private static Exp EvalSet(Cell parameters, Env env) {
            var name = parameters.First.ToString();
            var value = Eval(parameters.Second, env);
            return env.Find(name).Bind(name, value);
        }

        private static Exp EvalDefine(Cell parameters, Env env) {
            if (parameters.First is Cell)
                return DefineFunc(parameters.First as Cell, parameters.Second, env);

            var name = parameters.First.ToString();
            var value = Eval(parameters.Second, env);
            return env.Bind(name, value);
        }

        private static Exp DefineFunc(Cell defineParameters, Exp body, Env env) {
            var name = defineParameters.First.ToString();
            var procParameters = defineParameters.Rest().ToStringList();
            return env.Bind(name, new Procedure(procParameters, body, env));
        }


        private static Exp EvalLet(Cell parameters, Env env) {
            var letEnv = new Env(env);
            letEnv.BindDefinitions(parameters.First as Cell);
            return Eval(parameters.Second, letEnv);
        }

        private static Exp EvalAnd(Cell expressions, Env env) {
            foreach (var exp in expressions.Iterate())
                if (!Boolean.IsTrue(Eval(exp, env)))
                    return Boolean.FALSE;
            return Boolean.TRUE;
        }

        private static Exp EvalOr(Cell expressions, Env env) {
            foreach (var exp in expressions.Iterate())
                if (Boolean.IsTrue(Eval(exp, env)))
                    return Boolean.TRUE;
            return Boolean.FALSE;
        }
    }
}