using System;
using System.Collections.Generic;

namespace UScheme {
    public class Env
    {
        private Dictionary<string, Exp> values = new Dictionary<string, Exp>();

        private Env outer = null;
        public Env() { }

        public static Env InitialEnv()
        {
            Env env = new Env();
            env.Put("+", Number.ADD);
            env.Put("-", Number.SUB);
            env.Put("=", Number.EQUALS);
            env.Put("<", Number.LESSTHAN);
            env.Put("<=", Number.LESSOREQUALTHAN);
            env.Put(">", Number.GREATERTHAN);
            env.Put(">=", Number.GREATEROREQUALTHAN);

            StdLib.AddProcedures(env);

            return env;
        }

        public Exp Eval(Exp exp) {
            return UScheme.Eval(exp, this);
        }

        public string Eval(string input) {
            return UScheme.Eval(input, this);
        }

        public Env(Env outer)
        {
            this.outer = outer;
        }

        public Env Find(string name)
        {
            if (values.ContainsKey(name))
            {
                return this;
            }
            if (outer != null)
            {
                return outer.Find(name);
            }
            throw new Exception("symbol '" + name + "' not found");
        }

        public Exp Get(string name)
        {
            Exp value;
            if (values.TryGetValue(name, out value))
            {
                return value;
            }
            if (outer != null)
            {
                return outer.Get(name);
            }
            throw new Exception("symbol '" + name + "' not found");
        }

        public Exp Put(string name, Exp value)
        {
            values.Add(name, value);
            return value;
        }

        public Exp Set(string name, Exp value)
        {
            values[name] = value;
            return value;
        }
    }
}