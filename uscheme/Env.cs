using System;
using System.Collections.Generic;

namespace UScheme {

    public class Env {
        private Dictionary<string, Exp> values = new Dictionary<string, Exp>();

        private Env outer = null;

        public Env() { }

        public static Env InitialEnv() {
            Env env = new Env();
            StdLib.AddLibrary(env);
            return env;
        }

        public Env(Env outer) {
            this.outer = outer;
        }

        public Env Find(string name) {
            if (values.ContainsKey(name))
                return this;
            
            if (outer != null)
                return outer.Find(name);

            throw new EvalException("symbol '" + name + "' not found");
        }

        public Exp Get(string name) {
            return Find(name).values[name];
        }

        public void BindDefinitions(UList definitions) {
            foreach (UList def in definitions)
                Bind(def.First.ToString(), UScheme.Eval(def.Second, this));
        }

        public Exp Bind(string name, Exp value) {
            Tracer.Bind(name, value);
            values[name] = value;
            return value;
        }
    }
}