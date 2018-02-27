using System;
using System.Collections.Generic;
using System.Text;

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

        void Reset() {
            stack.Clear();
            result = null;
            current = null;
        }

        public Exp Eval(Exp exp, Env env) {
            Reset();
            Push(exp, env);

            while (stack.Count > 0) {
                current = stack.Peek();
                var list = current.exp as Cell;

                if (IsSelfEvaluating(current.exp))
                    SetResultAndPop(current.exp);
                else if (current.exp is Symbol) {
                    SetResultAndPop(current.env.Get(current.exp.ToString()));
                } else if (!(current.exp is Cell) || !list.IsList) {
                    SetResultAndPop(current.exp);
                } else if (list.First == Symbol.QUOTE) {
                    SetResultAndPop(list.Second);
                } else if (list.First == Symbol.DEFINE) {
                    EvalDefine();
                } else if (list.First == Symbol.SET) {
                    EvalSet();
                } else if (list.First == Symbol.BEGIN) {
                    EvalBegin();
                } else if (list.First == Symbol.IF) {
                    EvalIf();
                } else if (list.First == Symbol.LAMBDA) {
                    EvalLambda();
                } else if (list.First == Symbol.LET) {
                    EvalLet();
                } else if (list.First == Symbol.AND) {
                    EvalAnd();
                } else if (list.First == Symbol.OR) {
                    EvalOr();
                } else if (list.First == Symbol.COND) {
                    EvalCond();
                } else
                    EvalProcedure();
            }
            return result;
        }

        bool IsSelfEvaluating(Exp exp) {
            return exp is Number || exp is UString || exp is Boolean || IsPurePair(exp) || exp is Procedure;
        }

        bool IsPurePair(Exp exp) {
            return exp is Cell && !(exp as Cell).IsList;
        }

        void EvalProcedure() {
            if (!current.paramsEvaluated)
                EvalAllInPlace(current.AsList, current.env);
            else if (current.First is CSharpProcedure)
                SetResultAndPop((current.First as CSharpProcedure).Apply(current.AsList.Rest()));
            else if (current.First is SchemeProcedure) {
                EvalSchemeProcedure();
            } else
                Error("first element of list is not a procedure: " + current.First);
        }

        void EvalSchemeProcedure() {
            var proc = current.First as SchemeProcedure;
            var bodyEnv = CreateCallEnvironment(proc, current.AsList.Rest(), proc.Env);
            stack.Pop();
            foreach (var bodyExp in Cell.Duplicate(proc.Body).Reverse().Iterate())
                Push(bodyExp, bodyEnv, current.destination);
        }

        bool IsValue(Exp exp) {
            return exp is Symbol || !(exp is Cell) || !(exp as Cell).IsList;
        }

        void EvalDefine() {
            if (current.Second is Cell) { // (define (f x y z) ... )
                var declaration = current.Second as Cell;
                var name = declaration.First.ToString();
                var argNames = declaration.Rest().ToStringList();
                var body = current.AsList.Skip(2);
                SetResultAndPop(current.env.Bind(name, new SchemeProcedure(argNames, body, current.env)));
            } else { // (define f ...)
                if (current.paramsEvaluated) {
                    var name = current.Second.ToString();
                    SetResultAndPop(current.env.Bind(name, current.Third));
                } else {
                    current.paramsEvaluated = true;
                    Push(current.Third, current.env, current.AsList.Rest().Rest());
                }
            }

        }

        void EvalIf() {
            if (!current.paramsEvaluated) {
                current.paramsEvaluated = true;
                Push(current.Second, current.env, current.AsList.cdr as Cell);
            } else
                ReplaceCurrent(Boolean.IsTrue(current.Second) ? current.Third : current.Fourth);
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
            var body = current.AsList.Skip(2);
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

        void EvalAllInPlace(Cell cell, Env env) {
            current.paramsEvaluated = true;
            while (cell != Cell.Null) {
                if (!IsSelfEvaluating(cell.car))
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

        void Error(string message) {
            var stackTrace = BuildStackTrace();
            throw new EvalException(string.Format("Error evaluating expression: {0}\nStack Trace:\n{1}", message, stackTrace));
        }

        string BuildStackTrace() {
            var trace = new StringBuilder();
            foreach (var frame in stack) {
                trace.Append(frame.ToString()).Append("\n");
            }
            return trace.ToString();
        }
    }
}