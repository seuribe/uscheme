using System;

namespace UScheme {
    public class CoreLib {
        public static Procedure Conversion(System.Func<Exp, Exp> conv) {
            return new CSharpProcedure(parameters => {
                return conv(parameters.First);
            });
        }

        public static Procedure Conversion<From, To>(Func<From, To> conv) where From : Exp where To : Exp {
            return new CSharpProcedure(parameters => {
                return conv((From)parameters.First);
            });
        }

        public static void EnsureArity(Cell list, int size) {
            var length = list.Length();
            if (length != size)
                throw new EvalException("procedure accepts only " + size + " arguments, " + length + " provided");
        }

        public static void EnsureArityMin(Cell list, int size) {
            var length = list.Length();
            if (length < size)
                throw new EvalException("procedure needs at least " + size + " arguments, " + length + " provided");
        }

        public static void EnsureArityWithin(Cell list, int min, int max = int.MaxValue) {
            var length = list.Length();
            if (length < min || length > max)
                throw new EvalException("invalid number of arguments for procedure: " + length);
        }

        public static void EnsureAll(Cell list, Func<Exp, bool> predicate, string error) {
            foreach (var exp in list.Iterate())
                if (!predicate(exp))
                    throw new EvalException(error);
        }

        public static void EnsureIs<T>(Exp exp) {
            if (!(exp is T))
                throw new EvalException("Expected " + typeof(T).ToString() + ", got " + typeof(Exp).ToString());
        }

        public static Procedure IsA<T>(Func<T, bool> evalFunction = null) where T : Exp {
            return new CSharpProcedure(parameters => {
                EnsureArity(parameters, 1);
                evalFunction = evalFunction ?? (e => true);
                return Boolean.Get(parameters.First is T && evalFunction((T)parameters.First));
            });
        }
    }
}