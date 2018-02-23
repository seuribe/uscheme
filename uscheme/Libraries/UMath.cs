using System;

namespace UScheme {
    public class UMath {
        private static Procedure UnaryProcedure(Func<double, double> func) {
            return new Procedure((Cell parameters, Env env) => {
                StdLib.EnsureArity(parameters, 1);
                var number = parameters.First as Number;
                return new RealNumber((float)func(number.DoubleValue));
            });
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
            env.Bind("abs", UnaryProcedure(Math.Abs));
            env.Bind("log", UnaryProcedure(Math.Log));
            env.Bind("sin", UnaryProcedure(Math.Sin));
            env.Bind("cos", UnaryProcedure(Math.Cos));
            env.Bind("acos", UnaryProcedure(Math.Acos));
            env.Bind("asin", UnaryProcedure(Math.Asin));
            env.Bind("tan", UnaryProcedure(Math.Tan));
            env.Bind("atan", UnaryProcedure(Math.Atan));
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