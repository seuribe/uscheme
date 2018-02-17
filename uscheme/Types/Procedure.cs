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

        public Exp Eval(UList values, Env env) {
            return EvalProc(values, env);
        }

        public Exp Eval(Exp first, Env env) {
            return EvalProc(new UList { first }, env);
        }

        public Exp Eval(Exp first, Exp second, Env env) {
            return EvalProc(new UList { first, second }, env);
        }

        private Exp EvalBody(UList values, Env outerEnv) {
            var callEnvironment = CreateCallEnvironment(values, outerEnv);
            return UScheme.Eval(body, callEnvironment);
        }

        Env CreateCallEnvironment(UList callValues, Env outerEnv) {
            var evalEnv = new Env(outerEnv);
            for (int i = 0; i < argumentNames.Count; i++)
                evalEnv.Bind(argumentNames[i], UScheme.Eval(callValues[i], outerEnv));

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