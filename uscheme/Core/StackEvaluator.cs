using System;
using System.Collections.Generic;

namespace UScheme {

    // Not thread safe!
    public class StackEvaluator : Evaluator {
        class Frame {
            public Exp exp;
            public Env env;
            public bool paramsEvaluated = false;
            public Cell destination;

            public Cell AsList => exp as Cell;
            public Exp First => AsList.First;
            public Exp Second => AsList.Second;
            public Exp Third => AsList.Third;
            public Exp Fourth => AsList.Fourth;

            public override string ToString() {
                return exp.ToString();
            }
        }

        readonly Stack<Frame> stack = new Stack<Frame>();
        Frame current;
        Exp result = null;

        public Exp Eval(Exp exp, Env env) {
            Push(exp, env);

            while (stack.Count > 0) {
                current = stack.Peek();
                exp = current.exp;
                env = current.env;
                var list = exp as Cell;

                if (exp is Symbol) { // env-defined variables
                    SetResultAndPop(env.Get(exp.ToString()));
                } else if (!(exp is Cell)) { // atoms like integer, float, etc.
                    SetResultAndPop(exp);
                } else if (!list.IsList) {
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
                        }
                    }
                } else if (list.First == Symbol.SET) {
                    EvalSet();
                } else if (list.First == Symbol.BEGIN) {
                    EvalBegin();
                } else if (list.First == Symbol.IF) {
                    if (list.Second is Boolean)
                        ReplaceCurrent(Boolean.IsTrue(list.Second) ? list.Third : list.Fourth);
                    else
                        Push(list.Second, env, list.cdr as Cell);
                } else if (list.First == Symbol.LAMBDA) {
                    EvalLambda();
                } else if (list.First == Symbol.LET) { // (let ((x ...) (y ...)) ... )
                    EvalLet();
                } else if (list.First == Symbol.AND) {
                    EvalAnd();
                } else if (list.First == Symbol.OR) {
                    EvalOr();
                } else if (list.First == Symbol.COND) { // (cond c1 e2 c2 e3 ... cn en)
                    EvalCond();
                } else if (list.First is CSharpProcedure) {
                    if (current.paramsEvaluated)
                        ReplaceCurrent((list.First as CSharpProcedure).Apply(list.Rest()));
                    else
                        EvalParametersInPlace(list.Rest(), env);
                } else if (list.First is SchemeProcedure) {
                    var proc = list.First as SchemeProcedure;
                    if (current.paramsEvaluated)
                        ReplaceCurrent(proc.Body, CreateCallEnvironment(proc, list.Rest(), proc.Env));
                    else
                        EvalParametersInPlace(list.Rest(), env);
                } else {
                    Push(list.First, env, list);
                }

            }
            return result;
        }


        void EvalBegin() {
            stack.Pop();
            foreach (var seqExp in current.AsList.Rest().Reverse().Iterate())
                Push(seqExp, current.env, current.destination);
        }

        void EvalSet() {
            if (current.paramsEvaluated) {
                var name = current.Second.ToString();
                SetResultAndPop(current.env.Find(name).Bind(name, current.Third));
            } else {
                current.paramsEvaluated = true;
                Push(current.Third, current.env, current.AsList.Rest().Rest());
            }
        }

        void EvalLambda() {
            var argNames = (current.Second as Cell).ToStringList();
            var body = current.Third;
            SetResultAndPop(new SchemeProcedure(argNames, body, current.env));
        }

        void EvalLet() {
            var definitions = current.Second as Cell;
            var body = current.Third;
            var letEnv = new Env(current.env);
            ReplaceCurrent(body, letEnv);
            foreach (Cell definition in definitions.Iterate())
                Push(Cell.BuildList(Symbol.DEFINE, Symbol.From(definition.First.ToString()), definition.Second),
                    letEnv);
        }

        bool IsValue(Exp exp) {
            return exp is Symbol || !(exp is Cell) || !(exp as Cell).IsList;
        }

        void EvalBooleanOperation(Exp initialValue, Action ifFalse, Action ifTrue) {
            if (!current.paramsEvaluated) {
                current.paramsEvaluated = true;
                result = initialValue;
            }

            if (current.AsList.cdr == Cell.Null) {
                SetResultAndPop(result);
            } else if (IsValue(current.Second)) {
                (Boolean.IsTrue(current.Second) ? ifTrue : ifFalse)();
            } else {
                Push(current.Second, current.env, current.AsList.Rest());
            }
        }

        void EvalCond() {
            if (current.AsList.cdr == Cell.Null) {
                SetResultAndPop(Boolean.FALSE);
                return;
            }

            var test = (current.Second as Cell).First;
            var expression = (current.Second as Cell).Second;
            if (test.UEquals(Symbol.ELSE)) {
                ReplaceCurrent(expression);
            } else if (!IsValue(test)) {
                Push((current.Second as Cell).First, current.env, current.Second as Cell);
            } else if (Boolean.IsTrue(test)) {
                ReplaceCurrent(expression);
            } else {
                SkipParameters(1);
            }
        }

        void EvalOr() {
            EvalBooleanOperation(Boolean.FALSE,
                ifFalse: () => SkipParameters(1),
                ifTrue: () => SetResultAndPop(current.Second)
                );
        }

        void EvalAnd() {
            EvalBooleanOperation(Boolean.TRUE,
                ifFalse: () => SetResultAndPop(current.Second),
                ifTrue: () => { result = current.Second; SkipParameters(1); }
                );
        }

        void SkipParameters(int numParams) {
            var list = current.exp as Cell;
            list.cdr = list.Skip(numParams + 1);
        }

        void ReplaceCurrent(Exp exp, Env env = null) {
            current.paramsEvaluated = false;
            current.exp = exp.Clone();
            current.env = env ?? current.env;
        }

        void SetResultAndPop(Exp result) {
            this.result = result;
            var current = stack.Peek();
            if (current.destination != null)
                current.destination.car = result;
            stack.Pop();
        }

        void Push(Exp exp, Env env, Cell destination = null) {
            stack.Push(new Frame { exp = exp.Clone(), env = env, destination = destination });
        }

        void EvalParametersInPlace(Cell cell, Env env) {
            current.paramsEvaluated = true;
            while (cell != Cell.Null) {
                Push(cell.car, env, cell);
                cell = cell.cdr as Cell;
            }
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