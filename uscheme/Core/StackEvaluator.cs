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
            public Cell Rest => AsList.Rest();
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

        void Push(Exp exp, Env env, Cell destination = null) {
            stack.Push(new Frame { exp = exp.Clone(), env = env, destination = destination });
            current = stack.Peek();
        }

        void PushAll(IEnumerable<Exp> expressions, Env env, Cell destination = null) {
            var reversed = new List<Exp>(expressions);
            reversed.Reverse();
            foreach (var seqExp in reversed)
                Push(seqExp, env, destination);
            current = stack.Peek();
        }

        void PushProcedureParameters(Cell cell, Env env) {
            current.paramsEvaluated = true;
            while (cell != Cell.Null) {
                if (!IsSelfEvaluating(cell.car))
                    Push(cell.car, env, cell);
                cell = cell.cdr as Cell;
            }
        }

        public Exp Eval(Exp exp, Env env) {
            Reset();
            if (exp is Sequence)
                PushAll((exp as Sequence).forms, env);
            else
                Push(exp, env);

            while (stack.Count > 0) {
                var list = current.exp as Cell;

                if (IsSelfEvaluating(current.exp))
                    SetResultAndPop(current.exp);
                else if (current.exp is Symbol) {
                    SetResultAndPop(current.env.Get(current.exp.ToString()));
                } else if (list.First == Symbol.QUOTE) {
                    EvalQuote();
                } else if (list.First == Symbol.DEFINE) {
                    EvalDefine();
                } else if (list.First == Symbol.SET) {
                    EvalSet();
                } else if (list.First == Symbol.BEGIN) {
                    EvalSequence();
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
            return exp is Number || exp is UString || exp is Boolean ||
                   exp is Vector || exp is Character || IsPurePair(exp) ||
                   exp is Procedure;
        }

        bool IsPurePair(Exp exp) {
            return exp is Cell && !(exp as Cell).IsList;
        }

        void EvalQuote() {
            SetResultAndPop(current.Second);
        }

        void EvalProcedure() {
            if (!current.paramsEvaluated)
                PushProcedureParameters(current.AsList, current.env);
            else if (current.First is CSharpProcedure)
                SetResultAndPop((current.First as CSharpProcedure).Apply(current.Rest));
            else if (current.First is SchemeProcedure) {
                EvalSchemeProcedure();
            } else
                Error("first element of list is not a procedure: " + current.First);
        }

        void EvalSchemeProcedure() {
            var proc = current.First as SchemeProcedure;
            var bodyEnv = CreateCallEnvironment(proc, current.Rest, proc.Env);
            var destination = current.destination;
            stack.Pop();
            foreach (var bodyExp in Cell.Duplicate(proc.Body).Reverse().Iterate())
                Push(bodyExp, bodyEnv, destination);
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
            } else if (current.paramsEvaluated) { // (define f ...)
                var name = current.Second.ToString();
                SetResultAndPop(current.env.Bind(name, current.Third));
            } else
                PushParameter(2);
        }

        void PushParameter(int index) {
            current.paramsEvaluated = true;
            Push(current.AsList[index], current.env, current.AsList.Skip(index));
        }

        void EvalIf() {
            if (!current.paramsEvaluated)
                PushParameter(1);
            else
                ReplaceCurrent(Boolean.IsTrue(current.Second) ? current.Third : current.Fourth);
        }

        void EvalSequence() {
            stack.Pop();
            PushAll(current.Rest.Iterate(), current.env, current.destination);
        }

        void EvalSet() {
            if (current.paramsEvaluated)
                SetResultAndPop(current.env.Set(current.Second.ToString(), current.Third));
            else
                PushParameter(2);
        }

        void EvalLambda() {
            var argNames = (current.Second as Cell).ToStringList();
            var body = current.AsList.Skip(2);
            SetResultAndPop(new SchemeProcedure(argNames, body, current.env));
        }

        void EvalLet() {
            var definitions = current.Second as Cell;
            var body = current.AsList.Skip(2);
            var letEnv = new Env(current.env);
            stack.Pop();
            PushAll(body.Iterate(), letEnv, current.destination);
            foreach (Cell definition in definitions.Iterate())
                Push(Cell.BuildList(Symbol.DEFINE, Symbol.From(definition.First.ToString()), definition.Second),
                    letEnv);
        }

        void EvalBooleanOperation(Exp initialValue, Action ifFalse, Action ifTrue) {
            if (!current.paramsEvaluated) {
                current.paramsEvaluated = true;
                result = initialValue;
            }

            if (NoParameters()) {
                SetResultAndPop(result);
            } else if (IsValue(current.Second)) {
                (Boolean.IsTrue(current.Second) ? ifTrue : ifFalse)();
            } else {
                Push(current.Second, current.env, current.Rest);
            }
        }

        bool NoParameters() {
            return current.AsList.cdr == Cell.Null;
        }

        void EvalCond() {
            if (NoParameters()) {
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
                RemoveCurrentParameters(1);
            }
        }

        void EvalOr() {
            EvalBooleanOperation(Boolean.FALSE,
                ifFalse: () => RemoveCurrentParameters(1),
                ifTrue: () => SetResultAndPop(current.Second)
                );
        }

        void EvalAnd() {
            EvalBooleanOperation(Boolean.TRUE,
                ifFalse: () => SetResultAndPop(current.Second),
                ifTrue: () => { result = current.Second; RemoveCurrentParameters(1); }
                );
        }

        void RemoveCurrentParameters(int numParams) {
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
            if (current.destination != null)
                current.destination.car = result;
            stack.Pop();
            if (stack.Count > 0)
                current = stack.Peek();
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