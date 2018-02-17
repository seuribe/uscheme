using System;

namespace UScheme {
    partial class StdLib {
        class UnaryNumberProc : Procedure {
            public UnaryNumberProc(Func<double, double> func) {
                EvalProc = (UList fparams, Env env) => {
                    EnsureArity(fparams, 1);
                    var expr = fparams[0];
                    var number = UScheme.Eval(expr, env) as Number;
                    return new RealNumber((float)func(number.DoubleValue));
                };
            }
        }
    }
}