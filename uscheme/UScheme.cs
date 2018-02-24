using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UScheme {
    public delegate Exp ProcedureBody(Cell argumentList);
    public delegate Exp InternalForm(Cell argumentList, Env env);

    public class UScheme {

        readonly static Dictionary<Exp, InternalForm> SyntacticKeywords
            = new Dictionary<Exp, InternalForm> {
                { Symbol.DEFINE, EvalDefine },
                { Symbol.COND, EvalCond },
                { Symbol.SET, EvalSet },
                { Symbol.LAMBDA, EvalLambda },
                { Symbol.AND, EvalAnd },
                { Symbol.OR, EvalOr },
                { Symbol.BEGIN, EvalSequential} ,
                { Symbol.LET, EvalLet },
                { Symbol.IF, EvalIf },
                { Symbol.QUOTE, EvalQuote }};


        class Frame {
            public Exp exp;
            public Env env;
            public bool paramsEvaluated = false;
            public Cell destination;
            public override string ToString() {
                return exp.ToString();
            }
        }

        public static Exp StackEval(Exp exp, Env env) {
            var stack = new Stack<Frame>();
            stack.Push(new Frame { exp = exp, env = env });

            Exp result = null;
            while (stack.Count > 0) {
                var current = stack.Peek();
                exp = current.exp;
                env = current.env;
                var list = exp as Cell;

                if (exp is Symbol) { // env-defined variables
                    result = env.Get(exp.ToString());
                    stack.Pop();
                } else if (!(exp is Cell)) { // atoms like integer, float, etc.
                    result = exp;
                    stack.Pop();
                } else if (list.First == Symbol.QUOTE) {
                    result = list.Second;
                    stack.Pop();
                } else if (list.First == Symbol.DEFINE) {
                    if (list.Second is Cell) { // (define (f x y z) ... )
                        var declaration = list.Second as Cell;
                        var name = declaration.First.ToString();
                        var argNames = declaration.Rest().ToStringList();
                        var body = list.Third;
                        result = env.Bind(name, new SchemeProcedure(argNames, body, env));
                        stack.Pop();
                    } else { // (define f ...)
                        if (current.paramsEvaluated) {
                            var name = list.Second.ToString();
                            result = env.Bind(name, list.Third);
                            stack.Pop();
                        } else {
                            current.paramsEvaluated = true;
                            stack.Push(new Frame { exp = list.Third, env = env, destination = list.Rest().Rest() });
                            continue;
                        }
                    }
                } else if (list.First == Symbol.SET) {
                    if (current.paramsEvaluated) {
                        var name = list.Second.ToString();
                        result = env.Find(name).Bind(name, list.Third);
                        stack.Pop();
                    } else {
                        current.paramsEvaluated = true;
                        stack.Push(new Frame { exp = list.Third, env = env, destination = list.Rest().Rest() });
                    }
                } else if (list.First == Symbol.BEGIN) {
                    stack.Pop();
                    foreach (var seqExp in list.Rest().Reverse().Iterate())
                        stack.Push(new Frame { exp = seqExp, env = env, destination = current.destination });
                    continue;
                } else if (list.First == Symbol.IF) {
                    if (list.Second is Boolean)
                        current.exp = Boolean.IsTrue(list.Second) ? list.Third : list.Fourth;
                    else
                        stack.Push(new Frame {
                            exp = list.Second,
                            env = env,
                            destination = list.cdr as Cell
                        });
                    continue;
                } else if (list.First == Symbol.LAMBDA) {
                    var argNames = (list.Second as Cell).ToStringList();
                    var body = list.Third;
                    result = new SchemeProcedure(argNames, body, env);
                    stack.Pop();
                } else if (list.First == Symbol.LET) { // (let ((x ...) (y ...)) ... )
                    var definitions = list.Second as Cell;
                    var body = list.Third;
                    current.env = new Env(env);
                    current.exp = body;
                    foreach (Cell definition in definitions.Iterate()) // replace each definitions with a define form
                        stack.Push(new Frame {
                            exp = Cell.BuildList(Symbol.DEFINE, Symbol.From(definition.First.ToString()), definition.Second),
                            env = current.env });
                    continue;
                } else if (list.First == Symbol.AND) {
                    if (list.cdr == Cell.Null) {
                        result = Boolean.TRUE;
                        stack.Pop();
                    } else if (list.Second is Boolean) {
                        if (Boolean.IsFalse(list.Second)) {
                            result = Boolean.FALSE;
                            stack.Pop();
                        } else {
                            var third = list.Rest().Rest(); // second is true, remove it and try third
                            list.cdr = third;
                            continue;
                        }
                    } else {
                        // second is not a boolean
                        stack.Push(new Frame { exp = list.Second, env = env, destination = list.Rest() });
                        continue;
                    }
                } else if (list.First == Symbol.OR) {
                } else if (list.First == Symbol.COND) {
                } else if (list.First is CSharpProcedure) {
                    if (current.paramsEvaluated)
                        current.exp = (list.First as CSharpProcedure).Apply(list.Rest());
                    else {
                        current.paramsEvaluated = true;
                        var cell = list.Rest();
                        while (cell != Cell.Null) {
                            stack.Push(new Frame { exp = cell.car, env = env, destination = cell });
                            cell = cell.cdr as Cell;
                        }
                    }
                    continue;
                } else if (list.First is SchemeProcedure) {
                    var proc = list.First as SchemeProcedure;
                    if (current.paramsEvaluated) {
                        current.paramsEvaluated = false;
                        current.exp = proc.Body;
                        current.env = CreateCallEnvironment(proc, list.Rest(), proc.Env);
                    } else {
                        current.paramsEvaluated = true;
                        var cell = list.Rest();
                        while (cell != Cell.Null) {
                            stack.Push(new Frame { exp = cell.car, env = env, destination = cell });
                            cell = cell.cdr as Cell;
                        }
                    }
                    continue;
                } else {
                    stack.Push(new Frame { exp = list.First, env = env, destination = list });
                    continue;
                }

                if (current.destination != null)
                    current.destination.car = result;
            }
            return result;
        }

        public static Exp Eval(Exp exp, Env env) {
            Tracer.Eval(exp);

            if (exp is Symbol) // env-defined variables
                return env.Get(exp.ToString());

            if (!(exp is Cell)) // atoms like integer, float, etc.
                return exp;

            var list = exp as Cell;
            if (SyntacticKeywords.TryGetValue(list.First, out InternalForm internalForm))
                return internalForm(list.Rest(), env);

            var evaluatedParameters = EvalEach(list, env);
            var procedure = evaluatedParameters.First;
            var parameters = evaluatedParameters.Rest();

            if (procedure is CSharpProcedure)
                return (procedure as CSharpProcedure).Apply(parameters);

            return Eval((procedure as SchemeProcedure).Body, CreateCallEnvironment(procedure as SchemeProcedure, parameters, env));
        }

        public static Exp Apply(Procedure proc, Cell parameters) {
            return Eval(Cell.BuildList(proc, parameters), proc.Env);
        }

        static Env CreateCallEnvironment(SchemeProcedure procedure, Cell callValues, Env outerEnv) {
            StdLib.EnsureArity(callValues, procedure.ArgumentNames.Count);
            var evalEnv = new Env(outerEnv);
            for (int i = 0 ; i < procedure.ArgumentNames.Count ; i++)
                evalEnv.Bind(procedure.ArgumentNames[i], callValues[i]);

            return evalEnv;
        }

        static Exp EvalSequential(Cell expressions, Env env) {
            Exp ret = null;
            foreach (var e in expressions.Iterate())
                ret = Eval(e, env);

            return ret;
        }

        private static Cell EvalEach(Cell parameters, Env env) {
            return Cell.BuildList(parameters.Iterate().Select(exp => Eval(exp, env)).ToList());
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
            return new SchemeProcedure(argNames, body, env);
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
            return env.Bind(name, new SchemeProcedure(procParameters, body, env));
        }

        private static Exp EvalLet(Cell parameters, Env env) {
            var letEnv = new Env(env);
            letEnv.BindDefinitions(parameters.First as Cell);
            return Eval(parameters.Second, letEnv);
        }

        private static Exp EvalAnd(Cell expressions, Env env) {
            return Boolean.Get(expressions.Iterate().All(exp => Boolean.IsTrue(Eval(exp, env))));
        }

        private static Exp EvalOr(Cell expressions, Env env) {
            return Boolean.Get(expressions.Iterate().Any(exp => Boolean.IsTrue(Eval(exp, env))));
        }
    }
}