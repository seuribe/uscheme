using System;
using System.Collections.Generic;

namespace UScheme {
    public class Procedure : Exp {
        private List<string> argumentNames;
        private Exp body;
        private Env env;

        public EvalProc EvalProc { get; protected set; }

        public Procedure() { }

        public Procedure(List<string> argumentNames, Exp body, Env env) 
        {
            this.argumentNames = argumentNames;
            this.body = body;
            this.env = env;
            this.EvalProc = EvalBody;
        }

        public Procedure(EvalProc evalProc) {
            this.EvalProc = evalProc;
        }

        public Exp Eval(Cell values) {
            return EvalProc(values, env);
        }

        public Exp Eval(Exp first) {
            return EvalProc(Cell.BuildList(first), env);
        }

        public Exp Eval(Exp first, Exp second) {
            return EvalProc(Cell.BuildList(first, second), env);
        }

        // externalEnv parameter is needed only for complying with EvalProc delegate
        private Exp EvalBody(Cell values, Env externalEnv) {
            var callEnvironment = CreateCallEnvironment(values, env);
            return UScheme.Eval(body, callEnvironment);
        }

        Env CreateCallEnvironment(Cell callValues, Env outerEnv) {
            StdLib.EnsureArity(callValues, argumentNames.Count);
            var evalEnv = new Env(outerEnv);
            for (int i = 0; i < argumentNames.Count; i++)
                evalEnv.Bind(argumentNames[i], callValues[i]);

            return evalEnv;
        }

        public bool UEquals(Exp other) {
            return (other == this) ||
                    (other is Procedure &&
                     body.UEquals((other as Procedure).body) &&
                     argumentNames.Equals((other as Procedure).argumentNames));
        }

    }
}