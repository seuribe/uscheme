using System.Collections.Generic;

namespace UScheme {

    // Not thread safe
    public class StackEvaluator : Evaluator {
        class Frame {
            public Exp exp;
            public Env env;
            public bool paramsEvaluated = false;
            public Cell destination;

            public override string ToString() {
                return exp.ToString();
            }
        }

        readonly Stack<Frame> stack = new Stack<Frame>();
        Exp result = null;

        void SetResultAndPop(Exp result) {
            this.result = result;
            stack.Pop();
        }

        void Push(Exp exp, Env env, Cell destination = null) {
            stack.Push(new Frame { exp = exp, env = env, destination = destination });
        }

        public Exp Eval(Exp exp, Env env) {
            stack.Push(new Frame { exp = exp, env = env });

            while (stack.Count > 0) {
                var current = stack.Peek();
                exp = current.exp;
                env = current.env;
                var list = exp as Cell;

                if (exp is Symbol) { // env-defined variables
                    SetResultAndPop(env.Get(exp.ToString()));
                } else if (!(exp is Cell)) { // atoms like integer, float, etc.
                    SetResultAndPop(exp);
                } else if (list.First == Symbol.QUOTE) {
                    SetResultAndPop(list.Second);
                } else if (list.First == Symbol.DEFINE) {
                    if (list.Second is Cell) { // (define (f x y z) ... )
                        var declaration = list.Second as Cell;
                        var name = declaration.First.ToString();
                        var argNames = declaration.Rest().ToStringList();
                        var body = list.Third;
                        SetResultAndPop(env.Bind(name, new SchemeProcedure(argNames, body, env)));
                    } else { // (define f ...)
                        if (current.paramsEvaluated) {
                            var name = list.Second.ToString();
                            SetResultAndPop(env.Bind(name, list.Third));
                        } else {
                            current.paramsEvaluated = true;
                            Push(list.Third, env, list.Rest().Rest());
                            continue;
                        }
                    }
                } else if (list.First == Symbol.SET) {
                    if (current.paramsEvaluated) {
                        var name = list.Second.ToString();
                        SetResultAndPop(env.Find(name).Bind(name, list.Third));
                    } else {
                        current.paramsEvaluated = true;
                        Push(list.Third, env, list.Rest().Rest());
                    }
                } else if (list.First == Symbol.BEGIN) {
                    stack.Pop();
                    foreach (var seqExp in list.Rest().Reverse().Iterate())
                        Push(seqExp, env, current.destination);

                    continue;
                } else if (list.First == Symbol.IF) {
                    if (list.Second is Boolean)
                        current.exp = Boolean.IsTrue(list.Second) ? list.Third : list.Fourth;
                    else
                        Push(list.Second, env, list.cdr as Cell);
                    continue;
                } else if (list.First == Symbol.LAMBDA) {
                    var argNames = (list.Second as Cell).ToStringList();
                    var body = list.Third;
                    SetResultAndPop(new SchemeProcedure(argNames, body, env));
                } else if (list.First == Symbol.LET) { // (let ((x ...) (y ...)) ... )
                    var definitions = list.Second as Cell;
                    var body = list.Third;
                    current.env = new Env(env);
                    current.exp = body;
                    foreach (Cell definition in definitions.Iterate()) 
                        Push(Cell.BuildList(Symbol.DEFINE, Symbol.From(definition.First.ToString()), definition.Second),
                            current.env);
                    continue;
                } else if (list.First == Symbol.AND) {
                    if (list.cdr == Cell.Null) {
                        SetResultAndPop(Boolean.TRUE);
                    } else if (list.Second is Boolean) {
                        if (Boolean.IsFalse(list.Second)) {
                            SetResultAndPop(Boolean.FALSE);
                        } else {
                            list.cdr = list.Skip(2);
                            continue;
                        }
                    } else {
                        Push(list.Second, env, list.Rest());
                        continue;
                    }
                } else if (list.First == Symbol.OR) {
                    if (list.cdr == Cell.Null) {
                        SetResultAndPop(Boolean.FALSE);
                    } else if (list.Second is Boolean) {
                        if (Boolean.IsTrue(list.Second)) {
                            SetResultAndPop(Boolean.TRUE);
                        } else {
                            list.cdr = list.Skip(2);
                            continue;
                        }
                    } else {
                        Push(list.Second, env, list.Rest());
                        continue;
                    }
                } else if (list.First == Symbol.COND) { // (cond c1 e2 c2 e3 ... cn en)
                    if (list.cdr == Cell.Null) { // if none is true behavior is undefined. return false, like OR
                        SetResultAndPop(Boolean.FALSE);
                    } else if (list.Second is Boolean) {
                        if (Boolean.IsTrue(list.Second)) {
                            current.exp = list.Third;
                            continue;
                        } else {
                            list.cdr = list.Skip(3);
                            continue;
                        }
                    } else {
                        // second is not a boolean
                        Push(list.Second, env, list.Rest());
                        continue;
                    }
                } else if (list.First is CSharpProcedure) {
                    if (current.paramsEvaluated)
                        current.exp = (list.First as CSharpProcedure).Apply(list.Rest());
                    else {
                        current.paramsEvaluated = true;
                        var cell = list.Rest();
                        while (cell != Cell.Null) {
                            Push(cell.car, env, cell);
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
                            Push(cell.car, env, cell);
                            cell = cell.cdr as Cell;
                        }
                    }
                    continue;
                } else {
                    Push(list.First, env, list);
                    continue;
                }

                if (current.destination != null)
                    current.destination.car = result;
            }
            return result;
        }

        static Env CreateCallEnvironment(SchemeProcedure procedure, Cell callValues, Env outerEnv) {
            StdLib.EnsureArity(callValues, procedure.ArgumentNames.Count);
            var evalEnv = new Env(outerEnv);
            for (int i = 0 ; i < procedure.ArgumentNames.Count ; i++)
                evalEnv.Bind(procedure.ArgumentNames[i], callValues[i]);

            return evalEnv;
        }
    }
}