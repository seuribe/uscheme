using System.Collections.Generic;

namespace UScheme {
    public class Procedure : Exp {
        private List<string> argumentNames;
        private Exp body;
        private Env env;

        readonly ProcedureBody applyBody;

        public Procedure() { }

        public Procedure(List<string> argumentNames, Exp body, Env env) {
            this.argumentNames = argumentNames;
            this.body = body;
            this.env = env;
            applyBody = UserDefinedBody;
        }

        public Procedure(ProcedureBody body) {
            applyBody = body;
        }

        public Exp Apply(Cell values) {
            return applyBody(values);
        }

        public Exp Apply(Exp first) {
            return applyBody(Cell.BuildList(first));
        }

        public Exp Apply(Exp first, Exp second) {
            return applyBody(Cell.BuildList(first, second));
        }

        // externalEnv parameter is needed only for complying with EvalProc delegate
        private Exp UserDefinedBody(Cell values) {
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