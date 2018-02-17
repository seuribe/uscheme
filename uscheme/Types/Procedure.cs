using System;
using System.Collections.Generic;

namespace UScheme {
    public class Procedure : Exp {
        private List<string> argumentNames;
        private Exp body;
        private Env env;

        public delegate Exp EvalProc(UList argumentValues, Env env);

        protected EvalProc evalProc;

        public Procedure() { }

        public Procedure(List<string> argumentNames, Exp body, Env env) 
        {
            this.argumentNames = argumentNames;
            this.body = body;
            this.env = env;
            this.evalProc = DefaultEval;
        }

        public Procedure(EvalProc evalProc)
        {
            this.evalProc = evalProc;
        }

        public Exp Eval(UList values, Env env)
        {
            return evalProc(values, env);
        }

        public Exp Eval(Exp value, Env env) {
            return evalProc(new UList { value }, env);
        }

        private Exp DefaultEval(UList values, Env outerEnv)
        {
            Console.Out.WriteLine("Eval proc with " + argumentNames.Count + " params using " + values.Count + " values");
            Env evalEnv = new Env(outerEnv);
            for (int i = 0; i < argumentNames.Count; i++) {
                evalEnv.Bind(argumentNames[i], UScheme.Eval(values[i], outerEnv));
            }
            return UScheme.Eval(body, evalEnv);
        }

        public bool UEquals(Exp other) {
            return (other == this) ||
                    (other is Procedure &&
                     body.UEquals((other as Procedure).body) &&
                     argumentNames.Equals((other as Procedure).argumentNames));
        }

    }
}