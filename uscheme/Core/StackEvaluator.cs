using System;
using System.Collections.Generic;
using System.Text;

namespace UScheme {

    // Not thread safe!
    public class StackEvaluator {
        class Frame {
            public Exp exp;
            public Env env;
            public bool ready = false;
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

        void PushAllInOrder(IEnumerable<Exp> expressions, Env env, Cell destination = null) {
            foreach (var seqExp in Reversed(expressions))
                Push(seqExp, env, destination);
        }

        void PushAllAndReplace(Cell cell, Env env) {
            current.ready = true;
            while (cell != Cell.Null) {
                if (!IsSelfEvaluating(cell.car))
                    Push(cell.car, env, cell);
                cell = cell.cdr as Cell;
            }
        }

        void PushNthAndReplace(int index) {
            current.ready = true;
            Push(current.AsList[index], current.env, current.AsList.Skip(index));
        }

        static IEnumerable<T> Reversed<T>(IEnumerable<T> elements) {
            var list = new List<T>(elements);
            for (int index = list.Count - 1 ; index >= 0 ; index--)
                yield return list[index];
        }

        public Exp Eval(Exp exp, Env env) {
            if (exp is Sequence)
                PushAllInOrder((exp as Sequence).forms, env);
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
            var first = current.First;

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
            else if (first == Identifier.APPLY)
                EvalApply();
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
            if (!current.ready)
                PushAllAndReplace(current.AsList, current.env);
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
            stack.Pop();
            foreach (var bodyExp in Cell.Duplicate(proc.Body).Reverse().Iterate())
                Push(bodyExp, bodyEnv, current.destination);
        }

        bool IsValue(Exp exp) {
            return exp is Identifier || !(exp is Cell) || !(exp as Cell).IsList;
        }

        void EvalDefine() {
            if (current.Second is Cell)
                DefineProcedure();
            else if (current.ready)
                SetResultAndPop(current.env.Bind(current.Second.ToString(), current.Third));
            else
                PushNthAndReplace(2);
        }

        void DefineProcedure() { // (define (f x y z . rest) ... )
            var declaration = current.Second as Cell;
            GetProcedureArguments(declaration.Rest().ToStringList(), out List<string> argNames, out string variadicName);
            var body = current.AsList.Skip(2);
            var proc = CreateProcedure(body, current.env, argNames, variadicName);
            var name = declaration.First.ToString();
            current.env.Bind(name, proc);
            SetResultAndPop(proc);
        }

        void EvalIf() {
            if (!current.ready)
                PushNthAndReplace(1);
            else
                ReplaceCurrent(Boolean.IsTrue(current.Second) ? current.Third : current.Fourth);
        }

        void EvalSequence() {
            stack.Pop();
            PushAllInOrder(current.Rest.Iterate(), current.env, current.destination);
        }

        void EvalSet() {
            if (current.ready)
                SetResultAndPop(current.env.Set(current.Second.ToString(), current.Third));
            else
                PushNthAndReplace(2);
        }

        void EvalLambda() {
            var body = current.AsList.Skip(2);
            var args = current.Second as Cell;
            List<string> argNames = null;
            string variadicName = null;
            if (args != null)
                GetProcedureArguments(args.ToStringList(), out argNames, out variadicName);
            else
                variadicName = current.Second.ToString();
            SetResultAndPop(CreateProcedure(body, current.env, argNames, variadicName));
        }

        void GetProcedureArguments(List<string> allArguments, out List<string> argNames, out string variadicName) {
            var count = allArguments.Count;
            if (count >= 3 && allArguments[count - 2] == ".") {
                variadicName = allArguments[count - 1];
                argNames = allArguments.GetRange(0, count - 2);
            } else {
                argNames = allArguments;
                variadicName = null;
            }
        }

        Exp CreateProcedure(Cell body, Env env, List<string> argNames = null, string variadicName = null) {
            return new SchemeProcedure(body, env, argNames, variadicName);
        }

        void EvalLet() {
            var definitions = current.Second as Cell;
            var body = current.AsList.Skip(2);
            var letEnv = new Env(current.env);
            stack.Pop();
            PushAllInOrder(body.Iterate(), letEnv, current.destination);
            foreach (Cell definition in definitions.Iterate())
                Push(Cell.BuildList(Identifier.DEFINE, Identifier.From(definition.First.ToString()), definition.Second),
                    letEnv);
        }

        void EvalBooleanOperation(Exp initialValue, Action ifFalse, Action ifTrue) {
            if (!current.ready) {
                current.ready = true;
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

        void EvalApply() {
            if (!current.ready) {
                PushAllAndReplace(current.Rest, current.env);
                return;
            }

            var op = current.Second;
            var parameters = current.AsList.Skip(2);
            var rest = parameters.Last() as Cell;
            parameters.LastCell().car = rest.First;
            parameters.LastCell().cdr = rest.Rest();
            ReplaceCurrent(new Cell(op, parameters));
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
            current.ready = false;
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