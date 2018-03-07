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

        Stack<Frame> stack = new Stack<Frame>();
        Frame current;
        Exp result;

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
            if (exp is Sequence)
                PushAll((exp as Sequence).forms, env);
            else
                Push(exp, env);

            while (stack.Count > 0) {
                if (IsSelfEvaluating(current.exp))
                    SetResultAndPop(current.exp);
                else if (current.exp is Identifier)
                    SetResultAndPop(current.env.Get(current.exp.ToString()));
                else if (!EvaluatedSyntacticForm())
                    EvalProcedureCall();
            }
            return result;
        }

        bool EvaluatedSyntacticForm() {
            var first = (current.exp as Cell).First;

            if (first == Identifier.QUOTE)
                EvalQuote();
            else if (first == Identifier.DEFINE)
                EvalDefine();
            else if (first == Identifier.SET)
                EvalSet();
            else if (first == Identifier.BEGIN)
                EvalSequence();
            else if (first == Identifier.IF)
                EvalIf();
            else if (first == Identifier.LAMBDA)
                EvalLambda();
            else if (first == Identifier.LET)
                EvalLet();
            else if (first == Identifier.AND)
                EvalAnd();
            else if (first == Identifier.OR)
                EvalOr();
            else if (first == Identifier.COND)
                EvalCond();
            else
                return false;

            return true;
        }

        bool IsSelfEvaluating(Exp exp) {
            return exp is Number || exp is UString || exp is Boolean ||
                   exp is Vector || exp is Character || IsPurePair(exp) ||
                   exp is ByteVector || exp is Procedure;
        }

        bool IsPurePair(Exp exp) {
            return exp is Cell && !(exp as Cell).IsList;
        }

        void EvalQuote() {
            SetResultAndPop(current.Second);
        }

        void EvalProcedureCall() {
            if (!current.paramsEvaluated)
                PushProcedureParameters(current.AsList, current.env);
            else if (current.First is CSharpProcedure)
                SetResultAndPop((current.First as CSharpProcedure).Apply(current.Rest));
            else if (current.First is SchemeProcedure) {
                EvalSchemeProcedureCall();
            } else
                Error("first element of list is not a procedure: " + current.First);
        }

        void EvalSchemeProcedureCall() {
            var proc = current.First as SchemeProcedure;
            var bodyEnv = CreateCallEnvironment(proc, current.Rest, proc.Env);
            var destination = current.destination;
            stack.Pop();
            foreach (var bodyExp in Cell.Duplicate(proc.Body).Reverse().Iterate())
                Push(bodyExp, bodyEnv, destination);
        }

        bool IsValue(Exp exp) {
            return exp is Identifier || !(exp is Cell) || !(exp as Cell).IsList;
        }

        void EvalDefine() {
            if (current.Second is Cell)
                DefineProcedure();
            else if (current.paramsEvaluated)
                SetResultAndPop(current.env.Bind(current.Second.ToString(), current.Third));
            else
                PushParameter(2);
        }

        void DefineProcedure() { // (define (f x y z . rest) ... )
            var declaration = current.Second as Cell;
            var name = declaration.First.ToString();
            var argNames = declaration.Rest().ToStringList();
            var body = current.AsList.Skip(2);
            string variadicName = null;
            if (argNames.Count >= 3 && argNames[argNames.Count - 2] == ".") {
                variadicName = argNames[argNames.Count - 1];
                argNames.RemoveRange(argNames.Count - 2, 2);
            }
            var proc = CreateProcedure(body, current.env, argNames, variadicName);
            current.env.Bind(name, proc);
            SetResultAndPop(proc);
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
            var body = current.AsList.Skip(2);
            var args = current.Second;
            List<string> argNames = null;
            string variadicName = null;
            if (args is Cell) {
                argNames = (current.Second as Cell).ToStringList();
                if (argNames.Count >= 3 && argNames[argNames.Count - 2] == ".") {
                    variadicName = argNames[argNames.Count - 1];
                    argNames.RemoveRange(argNames.Count - 2, 2);
                }
            } else {
                variadicName = current.Second.ToString();
            }
            SetResultAndPop(CreateProcedure(body, current.env, argNames, variadicName));
        }

        Exp CreateProcedure(Cell body, Env env, List<string> argNames = null, string variadicName = null) {
            return new SchemeProcedure(body, env, argNames, variadicName);
        }

        void EvalLet() {
            var definitions = current.Second as Cell;
            var body = current.AsList.Skip(2);
            var letEnv = new Env(current.env);
            stack.Pop();
            PushAll(body.Iterate(), letEnv, current.destination);
            foreach (Cell definition in definitions.Iterate())
                Push(Cell.BuildList(Identifier.DEFINE, Identifier.From(definition.First.ToString()), definition.Second),
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
            if (test.UEquals(Identifier.ELSE)) {
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
//            StdLib.EnsureArity(callValues, procedure.ArgumentNames.Count);
            var evalEnv = new Env(outerEnv);
            for (int i = 0 ; i < procedure.NumberFixedArguments ; i++)
                evalEnv.Bind(procedure.ArgumentNames[i], callValues[i]);

            if (procedure.HasVariadicArguments)
                evalEnv.Bind(procedure.VariadicName, callValues.Skip(procedure.NumberFixedArguments));

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