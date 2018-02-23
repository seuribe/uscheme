using System;

namespace UScheme {
    partial class StdLib {
        class UnaryNumberProc : Procedure {
            public UnaryNumberProc(Func<double, double> func) {
                EvalProc = (Cell parameters, Env env) => {
                    EnsureArity(parameters, 1);
                    var number = parameters.First as Number;
                    return new RealNumber((float)func(number.DoubleValue));
                };
            }
        }
    }
}