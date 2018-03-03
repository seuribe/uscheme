using System;
using System.Collections.Generic;

namespace UScheme {

    public interface Procedure : Exp {
        Exp Apply(Cell values = null);
        Env Env { get;  }
    }

    public class CSharpProcedure : Procedure {
        readonly ProcedureBody body;

        public CSharpProcedure(ProcedureBody body) {
            this.body = body;
        }

        public Env Env { get => Env.Global; }

        public Exp Apply(Cell values = null) {
            return body(values ?? Cell.Null);
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
        public string VariadicName { get { return variadicName; } }
        public bool HasVariadicArguments => variadicName != null;
        public bool HasFixedArguments => ArgumentNames != null;
        public int NumberFixedArguments => HasFixedArguments ? argumentNames.Count : 0;

        private readonly List<string> argumentNames;
        private readonly string variadicName;
        private readonly Cell body;
        private readonly Env env;

        public Cell Body { get => body; }
        public Env Env { get => env; }

        public SchemeProcedure(Cell body, Env env, List<string> argumentNames = null, string variadicVariable = null) {
            this.argumentNames = argumentNames;
            this.variadicName = variadicVariable;
            this.body = body;
            this.env = env;
        }

        public Exp Apply(Cell values = null) {
            throw new EvalException("SchemeProcedure.Apply should not be called");
//            return UScheme.Eval(body, env);
        }

        public Exp Clone() {
            return new SchemeProcedure(Cell.Duplicate(body), env, argumentNames, variadicName);
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