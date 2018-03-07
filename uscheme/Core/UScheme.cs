using System;
using System.Collections;

namespace UScheme {
    public delegate Exp ProcedureBody(Cell argumentList);
    public delegate Exp InternalForm(Cell argumentList, Env env);

    public class UScheme {

        // TODO: externalize this to somewhere local / not in repository
        public static string LibraryDir = @"C:/src/uscheme/uscheme/lib/";

        public static Exp Eval(Exp exp, Env env) {
            return new StackEvaluator().Eval(exp, env);
        }
    }
}