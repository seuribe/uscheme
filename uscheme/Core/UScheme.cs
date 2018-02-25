using System;
using System.Collections;

namespace UScheme {
    public delegate Exp ProcedureBody(Cell argumentList);
    public delegate Exp InternalForm(Cell argumentList, Env env);

    public class UScheme {
        static Evaluator evaluator = new StackEvaluator();

        public static void SetEvaluator(Evaluator evaluator) {
            UScheme.evaluator = evaluator;
        }

        public static Exp Eval(Exp exp, Env env) {
            return evaluator.Eval(exp, env);
        }

        public static Exp Apply(Procedure proc, Cell parameters) {
            return Eval(Cell.BuildList(proc, parameters), proc.Env);
        }

    }
}