using System;

namespace UScheme {
    public class UMath {

        class UnaryNumberProc : Procedure {
            public UnaryNumberProc(Func<double, double> func) {
                ApplyBody = (Cell parameters, Env env) => {
                    StdLib.EnsureArity(parameters, 1);
                    var number = parameters.First as Number;
                    return new RealNumber((float)func(number.DoubleValue));
                };
            }
        }

        private static Procedure CompareAndCarryIf(Func<Number, Number, bool> takeNew) {
            return new Procedure((Cell parameters, Env env) => {
                StdLib.EnsureArityMin(parameters, 1);
                StdLib.EnsureAll(parameters, p => (p is Number), "function parameters should be numbers");
                var result = parameters.First as Number;
                foreach (Number number in parameters.Rest().Iterate())
                    if (takeNew(result, number))
                        result = number;

                return result;
            });
        }

        public static void AddLibrary(Env env) {
            env.Bind("abs", new UnaryNumberProc(Math.Abs));
            env.Bind("log", new UnaryNumberProc(Math.Log));
            env.Bind("sin", new UnaryNumberProc(Math.Sin));
            env.Bind("cos", new UnaryNumberProc(Math.Cos));
            env.Bind("acos", new UnaryNumberProc(Math.Acos));
            env.Bind("asin", new UnaryNumberProc(Math.Asin));
            env.Bind("tan", new UnaryNumberProc(Math.Tan));
            env.Bind("atan", new UnaryNumberProc(Math.Atan));
            env.Bind("max", Max);
            env.Bind("max", CompareAndCarryIf((a, b) => a.LessThan(b)));
            env.Bind("min", CompareAndCarryIf((a, b) => b.LessThan(a)));

            env.Bind("+", Number.ADD);
            env.Bind("-", Number.SUB);
            env.Bind("*", Number.MULT);
            env.Bind("/", Number.DIV);
            env.Bind("=", Number.EQUALS);
            env.Bind("<", Number.LESSTHAN);
            env.Bind("<=", Number.LESSOREQUALTHAN);
            env.Bind(">", Number.GREATERTHAN);
            env.Bind(">=", Number.GREATEROREQUALTHAN);
        }
    }
}