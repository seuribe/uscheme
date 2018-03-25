using System;
using System.Collections.Generic;

namespace UScheme {

    public class StdLib : CoreLib {

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

        private static readonly Procedure Append = new CSharpProcedure(parameters => {
            EnsureArityMin(parameters, 1);
            EnsureAll(parameters, exp => (exp is Cell) && (exp as Cell).IsList, "append parameters must be lists");

            var cell = Cell.Duplicate(parameters.First as Cell);
            foreach (Cell sublist in parameters.Rest().Iterate())
                cell = Cell.Append(cell, sublist);

            return cell;
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
            env.Bind("byte-vector?", IsA<ByteVector>());
            env.Bind("char?", IsA<Character>());
            env.Bind("null?", new CSharpProcedure(p => Boolean.Get(p.First == Cell.Null)));
            env.Bind("symbol?", IsA<Identifier>());

            env.Bind("symbol->string", new CSharpProcedure(p => new UString((p.First as Identifier).str)));
            env.Bind("string->symbol", new CSharpProcedure(p => Identifier.From((p.First as UString).str)));
            env.Bind("symbol=?", ListUEqual<Identifier>());

            env.Bind("equal?", Equal);
            env.Bind("eqv?", Eqv);
            env.Bind("eq?", Eq);

            env.Bind("print", Print);

            env.Bind("vector-length", new CSharpProcedure( list => new IntegerNumber(((list as Cell).First as Vector).Length) ));
            env.Bind("byte-vector-length", new CSharpProcedure(list => new IntegerNumber(((list as Cell).First as ByteVector).Length) ));
            env.Bind("append", Append);
            env.Bind("car", Car);
            env.Bind("cdr", Cdr);

            env.Bind("cons", Cons);

            Parser.Load(UScheme.LibraryDir + "stdlib.usc", env);
        }
    }
}