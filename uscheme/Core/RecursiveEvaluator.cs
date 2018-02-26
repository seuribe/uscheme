﻿using System.Collections.Generic;
using System.Linq;

namespace UScheme {
    public class RecursiveEvaluator : Evaluator {

        readonly Dictionary<Exp, InternalForm> SyntacticKeywords;

        public RecursiveEvaluator() {
            SyntacticKeywords = new Dictionary<Exp, InternalForm> {
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
        }

        public Exp Eval(Exp exp, Env env) {
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

        public Exp Apply(Procedure proc, Cell parameters) {
            return Eval(Cell.BuildList(proc, parameters), proc.Env);
        }

        Env CreateCallEnvironment(SchemeProcedure procedure, Cell callValues, Env outerEnv) {
            StdLib.EnsureArity(callValues, procedure.ArgumentNames.Count);
            var evalEnv = new Env(outerEnv);
            for (int i = 0 ; i < procedure.ArgumentNames.Count ; i++)
                evalEnv.Bind(procedure.ArgumentNames[i], callValues[i]);

            return evalEnv;
        }

        Exp EvalSequential(Cell expressions, Env env) {
            Exp ret = null;
            foreach (var e in expressions.Iterate())
                ret = Eval(e, env);

            return ret;
        }

        private Cell EvalEach(Cell parameters, Env env) {
            return Cell.BuildList(parameters.Iterate().Select(exp => Eval(exp, env)).ToList());
        }

        private Exp EvalQuote(Cell parameters, Env env) {
            return parameters.First;
        }

        private Exp EvalIf(Cell parameters, Env env) {
            return Eval(Boolean.IsTrue(Eval(parameters.First, env)) ? parameters.Second : parameters.Third, env);
        }

        private Exp EvalCond(Cell parameters, Env env) {
            for (int i = 0 ; i < parameters.Length() / 2 ; i++) {
                var condition = parameters[i * 2];
                if ((Eval(condition, env) as Boolean).Value)
                    return Eval(parameters[i * 2 + 1], env);
            }
            return Boolean.FALSE;
        }

        private Exp EvalLambda(Cell parameters, Env env) {
            var argNames = (parameters.First as Cell).ToStringList();
            var body = parameters.Second;
            return new SchemeProcedure(argNames, body, env);
        }

        private Exp EvalSet(Cell parameters, Env env) {
            var name = parameters.First.ToString();
            var value = Eval(parameters.Second, env);
            return env.Find(name).Bind(name, value);
        }

        private Exp EvalDefine(Cell parameters, Env env) {
            if (parameters.First is Cell)
                return DefineFunc(parameters.First as Cell, parameters.Second, env);

            var name = parameters.First.ToString();
            var value = Eval(parameters.Second, env);
            return env.Bind(name, value);
        }

        private Exp DefineFunc(Cell defineParameters, Exp body, Env env) {
            var name = defineParameters.First.ToString();
            var procParameters = defineParameters.Rest().ToStringList();
            return env.Bind(name, new SchemeProcedure(procParameters, body, env));
        }

        private Exp EvalLet(Cell parameters, Env env) {
            var letEnv = new Env(env);
            letEnv.BindDefinitions(parameters.First as Cell);
            return Eval(parameters.Second, letEnv);
        }

        private Exp EvalAnd(Cell expressions, Env env) {
            return Boolean.Get(expressions.Iterate().All(exp => Boolean.IsTrue(Eval(exp, env))));
        }

        private Exp EvalOr(Cell expressions, Env env) {
            return Boolean.Get(expressions.Iterate().Any(exp => Boolean.IsTrue(Eval(exp, env))));
        }
    }
}