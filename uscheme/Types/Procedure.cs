using System;
using System.Collections.Generic;

namespace UScheme {
    public class Procedure : Exp {
        private List<string> argumentNames;
        private Exp body;
        private Env env;

        public ProcedureBody ApplyBody { get; protected set; }

        public Procedure() { }

        public Procedure(List<string> argumentNames, Exp body, Env env) {
            this.argumentNames = argumentNames;
            this.body = body;
            this.env = env;
            this.ApplyBody = UserDefinedBody;
        }

        public Procedure(ProcedureBody body) {
            this.ApplyBody = body;
        }

        public Exp Apply(Cell values) {
            return ApplyBody(values, env);
        }

        public Exp Apply(Exp first) {
            return ApplyBody(Cell.BuildList(first), env);
        }

        public Exp Apply(Exp first, Exp second) {
            return ApplyBody(Cell.BuildList(first, second), env);
        }

        // externalEnv parameter is needed only for complying with EvalProc delegate
        private Exp UserDefinedBody(Cell values, Env externalEnv) {
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
            if (other == this)
                return true;

            var proc = other as Procedure;
            if (proc == null)
                return false;

            return body.UEquals(proc.body) && argumentNames.Equals(proc.argumentNames);
        }
    }
}