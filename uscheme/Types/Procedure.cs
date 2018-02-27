using System;
using System.Collections.Generic;

namespace UScheme {

    public interface Procedure : Exp {
        Exp Apply(Cell values);
        Env Env { get;  }
    }

    public class CSharpProcedure : Procedure {
        readonly ProcedureBody body;

        public CSharpProcedure(ProcedureBody body) {
            this.body = body;
        }

        public Env Env { get => Env.Global; }

        public Exp Apply(Cell values) {
            return body(values);
        }

        public Exp Clone() {
            return this;
        }

        public bool UEquals(Exp other) {
            if (other == this)
                return true;

            var proc = other as CSharpProcedure;
            if (proc == null)
                return false;

            return body == proc.body;
        }
    }

    public class SchemeProcedure : Procedure {
        public List<string> ArgumentNames { get { return argumentNames; } }
        private List<string> argumentNames;
        private Cell body;
        private Env env;

        public Cell Body { get => body; }
        public Env Env { get => env; }

        public SchemeProcedure(List<string> argumentNames, Cell body, Env env) {
            this.argumentNames = argumentNames;
            this.body = body;
            this.env = env;
        }

        public Exp Apply(Cell values) {
            return UScheme.Eval(body, env);
        }

        public Exp Clone() {
            return new SchemeProcedure(argumentNames, Cell.Duplicate(body), env);
        }

        public bool UEquals(Exp other) {
            if (other == this)
                return true;

            var proc = other as SchemeProcedure;
            if (proc == null)
                return false;

            return body.UEquals(proc.body) && argumentNames.Equals(proc.argumentNames);
        }
    }
}