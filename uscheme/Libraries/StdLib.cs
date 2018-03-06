using System;
using System.Collections.Generic;

namespace UScheme {

    public class StdLib {

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

        private static readonly Procedure Vector = new CSharpProcedure(parameters => {
            return new Vector(parameters);
        });

        private static readonly Procedure Equal = new CSharpProcedure(parameters => {
            EnsureArity(parameters, 2);
            return Boolean.Get(parameters.First.UEquals(parameters.Second));
        });

        private static readonly Procedure Eqv = new CSharpProcedure(parameters => {
            EnsureArity(parameters, 2);
            var first = parameters.First;
            var second = parameters.Second;
            return Boolean.Get(
                first == second || // This also covers boolean, Identifiers & empty lists
                (first.GetType() == second.GetType() &&
                    (AreSameCharacter(first, second) || AreSameNumber(first, second))
                ));
        });

        private static bool AreSameNumber(Exp a, Exp b) {
            return a.GetType() == typeof(Number) && AreSameAs<Number, float>(a, b, ch => ch.FloatValue);
        }

        private static bool AreSameCharacter(Exp a, Exp b) {
            return a.GetType() == typeof(Character) && AreSameAs<Character, char>(a, b, ch => ch.character);
        }

        private static bool AreSameAs<T, Tin>(Exp a, Exp b, Func<T, Tin> accessor) where T : Exp {
            return accessor((T)a).Equals(accessor((T)b));
        }

        private static readonly Procedure Eq = new CSharpProcedure(parameters => {
            EnsureArity(parameters, 2);
            return Boolean.Get(parameters.First == parameters.Second);
        });

        private static readonly Procedure Not = new CSharpProcedure(parameters => {
            EnsureArity(parameters, 1);
            return (parameters.First == Boolean.FALSE) ? Boolean.TRUE : Boolean.FALSE;
        });

        private static readonly Procedure Car = new CSharpProcedure(parameters => {
            EnsureArity(parameters, 1);
            var pair = parameters.First as Cell;
            return pair.car;
        });

        private static readonly Procedure Cdr = new CSharpProcedure(parameters => {
            EnsureArity(parameters, 1);
            var pair = parameters.First as Cell;
            return pair.cdr;
        });

        private static readonly Procedure Cons = new CSharpProcedure(parameters => {
            EnsureArity(parameters, 2);
            return new Cell(parameters.First, parameters.Second);
        });

        private static readonly Procedure Length = new CSharpProcedure(parameters => {
            EnsureArity(parameters, 1);
            return new IntegerNumber((parameters.First as Cell).Length());
        });

        private static readonly Procedure Append = new CSharpProcedure(parameters => {
            EnsureArityMin(parameters, 1);
            EnsureAll(parameters, exp => (exp is Cell) && (exp as Cell).IsList, "append parameters must be lists");

            var cell = Cell.Duplicate(parameters.First as Cell);
            foreach (Cell sublist in parameters.Rest().Iterate())
                cell = Cell.Append(cell, sublist);

            return cell;
        });

        private static readonly Procedure Apply = new CSharpProcedure(parameters => {
            EnsureArityMin(parameters, 2);
            EnsureIs<Procedure>(parameters.First);
            EnsureIs<Cell>(parameters.LastCell());

            var procedure = parameters.First as Procedure;
            if (parameters.Length() > 2)
                throw new UException("Not implemented: apply cannot accept more than the procedure and a list");

            return UScheme.Eval(Cell.BuildList(procedure, parameters.Rest()), procedure.Env);
        });

        // Only to be used in internal (CSharp) procedures
        public static Exp FoldlBase(CSharpProcedure op, Cell parameters) {
            EnsureArityMin(parameters, 2);
            var value = parameters.First;

            for (int i = 1 ; i < parameters.Length() ; i++)
                value = op.Apply(Cell.BuildList(value, parameters[i]));

            return value;
        }

        private static readonly Procedure Print = new CSharpProcedure(parameters => {
            var ev = parameters.First;
            // TODO: output should be configurable somewhere
            Console.Out.WriteLine(ev.ToString());
            return ev;
        });

        public static void AddLibrary(Env env) {
            env.Bind("number?", IsA<Number>());
            env.Bind("integer?", IsA<Number>(n => n.IsInteger()));
            env.Bind("real?", IsA<Number>()); // all numbers are real with current implementation
            env.Bind("boolean?", IsA<Boolean>());
            env.Bind("procedure?", IsA<Procedure>());
            env.Bind("list?", IsA<Cell>( c => c.IsList ));
            env.Bind("pair?", IsA<Cell>( c => c != Cell.Null ));
            env.Bind("vector?", IsA<Vector>());
            env.Bind("string?", IsA<UString>());

            env.Bind("equal?", Equal);
            env.Bind("eqv?", Eqv);
            env.Bind("eq?", Eq);

            env.Bind("print", Print);

            env.Bind("not", Not);
            env.Bind("length", Length);
            env.Bind("vector-length", new CSharpProcedure( list => new IntegerNumber(((list as Cell).First as Vector).Length) ));
            env.Bind("apply", Apply);
            env.Bind("append", Append);
            env.Bind("car", Car);
            env.Bind("cdr", Cdr);

            env.Bind("list", List);
            env.Bind("cons", Cons);
            env.Bind("vector", Vector);

            Parser.Load(UScheme.LibraryDir + "stdlib.usc", env);
        }
    }
}