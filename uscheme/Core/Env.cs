using System;
using System.Collections.Generic;

namespace UScheme {

    public class Env {
        private readonly Dictionary<string, Exp> values = new Dictionary<string, Exp>();
        private readonly Env outer;

        public Port CurrentInput;
        public Port CurrentOutput;

        public static Env Global {
            get {
                if (globalEnv == null)
                    globalEnv = InitialEnv();

                return globalEnv;
            }
        }
        static Env globalEnv = null;

        Env() { outer = null; }

        public static Env InitialEnv() {
            Env env = new Env();
            StdLib.AddLibrary(env);
            MathLib.AddLibrary(env);
            StringLib.AddLibrary(env);
            IOLib.AddLibrary(env);
            CharLib.AddLibrary(env);
            VectorLib.AddLibrary(env);
            env.CurrentInput = new TextReaderPort(Console.In);
            env.CurrentOutput = new TextWriterPort(Console.Out);
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

        public bool Has(string name) {
            return values.ContainsKey(name) || (outer != null && outer.Has(name));
        }

        public Exp Set(string name, Exp value) {
            return Find(name).Bind(name, value);
        }

        public Exp Get(string name) {
            return Find(name).values[name];
        }

        public void BindDefinitions(Cell definitions) {
            foreach (Cell def in definitions.Iterate())
                Bind(def.First.ToString(), UScheme.Eval(def.Second, this));
        }

        public Exp Bind(string name, Exp value) {
            Tracer.Bind(name, value);
            values[name] = value;
            return value;
        }
    }
}