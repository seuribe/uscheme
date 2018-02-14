using System;

namespace UScheme {
    public class BinaryNumberProc : Procedure {
        public BinaryNumberProc(Func<double, double, double> func) {
            evalProc = (UList fparams, Env env) => {
                StdLib.EnsureArity(fparams, 2);
                var first = UScheme.Eval(fparams[0], env) as Number;
                var second = UScheme.Eval(fparams[1], env) as Number;
                return new RealNumber((float)func(first.DoubleValue, second.DoubleValue));
            };
        }
    }
}