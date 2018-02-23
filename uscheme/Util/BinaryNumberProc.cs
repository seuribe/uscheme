using System;

namespace UScheme {
    public class BinaryNumberProc : Procedure {
        public BinaryNumberProc(Func<double, double, double> func) {
            ApplyBody = (Cell parameters, Env env) => {
                StdLib.EnsureArity(parameters, 2);
                var first = parameters.First as Number;
                var second = parameters.Second as Number;
                return new RealNumber((float)func(first.DoubleValue, second.DoubleValue));
            };
        }
    }
}