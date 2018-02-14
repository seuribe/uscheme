using System;
using System.Collections.Generic;

namespace UScheme {
    class Procedure : Exp {
        private List<string> args;
        private Exp body;
        private Env env;

        public delegate Exp EvalProc(UList args, Env env);

        protected EvalProc evalProc;

        public Procedure() { }

        public Procedure(List<string> args, Exp body, Env env)
        {
            this.args = args;
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
            Console.Out.WriteLine("Eval proc with " + args.Count + " params using " + values.Count + " values");
            Env evalEnv = new Env(outerEnv);
            for (int i = 0; i < args.Count; i++)
            {
                evalEnv.Put(args[i], UScheme.Eval(values[i], outerEnv));
            }
            return UScheme.Eval(body, evalEnv);
        }

        public bool UEquals(Exp other) {
            if (other == this) {
                return true;
            }
            return other is Procedure &&
                body.UEquals((other as Procedure).body) &&
                args.Equals((other as Procedure).args);
        }

    }
}