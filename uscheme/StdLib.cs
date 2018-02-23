using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UScheme {
    partial class StdLib {

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

        public static void EnsureAll(Cell list, Func<Exp, bool> predicate, string error) {
            foreach (var exp in list.Iterate())
                if (!predicate(exp))
                    throw new EvalException(error);
        }

        public static void EnsureIs<T>(Exp exp) {
            if (!(exp is T))
                throw new EvalException("Expected " + typeof(T).ToString() + ", got " + typeof(Exp).ToString());
        }

        private static Procedure BuildIsProcedure<T>(Func<T, bool> evalFunction = null) where T : Exp {
            return new Procedure((Cell parameters, Env env) => {
                EnsureArity(parameters, 1);
                evalFunction = evalFunction ?? (e => true);
                return Boolean.Get(parameters.First is T && evalFunction((T)parameters.First));
            });
        }

        private static readonly Procedure IsNumber = BuildIsProcedure<Number>();
        private static readonly Procedure IsInteger = BuildIsProcedure<Number>( n => n.IsInteger());
        private static readonly Procedure IsReal = BuildIsProcedure<Number>(); // all numbers are real with current implementation
        private static readonly Procedure IsProcedure = BuildIsProcedure<Procedure>();
        private static readonly Procedure IsSymbol = BuildIsProcedure<Symbol>();
        private static readonly Procedure IsBoolean = BuildIsProcedure<Boolean>();
        private static readonly Procedure IsList = BuildIsProcedure<Cell>();

        private static readonly Procedure IsPair = new Procedure((Cell parameters, Env env) => {
            EnsureArity(parameters, 1);
            return Boolean.Get(parameters.First is Cell && parameters.First != Cell.Null);
        });

        private static readonly Procedure List = new Procedure((Cell parameters, Env env) => {
            return parameters;
        });

        private static readonly Procedure Equal = new Procedure((Cell parameters, Env env) => {
            EnsureArity(parameters, 2);
            return Boolean.Get(parameters.First.UEquals(parameters.Second));
        });

        private static readonly Procedure Eq = new Procedure((Cell parameters, Env env) => {
            EnsureArity(parameters, 2);
            return Boolean.Get(parameters.First == parameters.Second);
        });

        private static readonly Procedure Not = new Procedure((Cell parameters, Env env) => {
            EnsureArity(parameters, 1);
            return (parameters.First == Boolean.FALSE) ? Boolean.TRUE : Boolean.FALSE;
        });

        private static readonly Procedure Car = new Procedure((Cell parameters, Env env) => {
            EnsureArity(parameters, 1);
            var pair = parameters.First as Cell;
            return pair.car;
        });

        private static readonly Procedure Cdr = new Procedure((Cell parameters, Env env) => {
            EnsureArity(parameters, 1);
            var pair = parameters.First as Cell;
            return pair.cdr;
        });

        private static readonly Procedure Cons = new Procedure((Cell parameters, Env env) => {
            EnsureArity(parameters, 2);
            return new Cell(parameters.First, parameters.Second);
        });

        private static readonly Procedure Length = new Procedure((Cell parameters, Env env) => {
            EnsureArity(parameters, 1);
            return new IntegerNumber((parameters.First as Cell).Length());
        });

        private static readonly Procedure Append = new Procedure((Cell parameters, Env env) => {
            EnsureArityMin(parameters, 1);
            EnsureAll(parameters, exp => (exp is Cell) && (exp as Cell).IsList, "append parameters must be lists");

            var cell = Cell.Duplicate(parameters.First as Cell);
            foreach (Cell sublist in parameters.Rest().Iterate())
                cell = Cell.Append(cell, sublist);

            return cell;
        });

        private static readonly Procedure Apply = new Procedure((Cell parameters, Env env) => {
            EnsureArityMin(parameters, 2);
            EnsureIs<Procedure>(parameters.First);
            EnsureIs<Cell>(parameters.LastCell());

            var procedure = parameters.First as Procedure;
            if (parameters.Length() > 2)
                throw new UException("Not implemented: apply cannot accept more than the procedure and a list");

            return procedure.Apply(parameters.Rest());
        });

        private static readonly Procedure Map = new Procedure((parameters, env) => {
            var proc = parameters.First as Procedure;
            var args = parameters.Rest();

            var list = new List<Exp>();
            foreach (var exp in parameters.Iterate())
                list.Add(proc.Apply(exp));

            return Cell.BuildList(list);
        });

        public static Exp FoldlBase(Procedure op, Cell parameters, Env env) {
            EnsureArityMin(parameters, 2);
            var value = parameters.First;

            for (int i = 1 ; i < parameters.Length() ; i++)
                value = op.Apply(Cell.BuildList(value, parameters[i]));

            return value;
        }

        private static readonly Procedure Foldl = new Procedure((Cell parameters, Env env) => {
            EnsureArity(parameters, 3);
            var op = parameters.First as Procedure;
            var value = parameters.Second;
            var list = parameters.Third as Cell;
            foreach (var e in list.Iterate())
                value = op.Apply(Cell.BuildList(value, e));
            
            return value;
        });

        private static readonly Procedure Print = new Procedure((Cell parameters, Env env) => {
            var ev = parameters.First;
            // TODO: output should be configurable somewhere
            Console.Out.WriteLine(ev.ToString());
            return ev;
        });

        private static readonly Procedure StringAppend = new Procedure((Cell parameters, Env env) => {
            var sb = new StringBuilder();
            foreach (Exp substring in parameters.Iterate())
                sb.Append((substring as UString).str);
            
            return new UString(sb.ToString());
        });

        private static readonly Procedure Nth = new Procedure((Cell parameters, Env env) => {
            EnsureArity(parameters, 2);
            var index = parameters.First as IntegerNumber;
            var list = parameters.Second as Cell;
            return list[index.IntValue];
        });

        // TODO: for-each, cons, pair?, eval, zip, foldr, compose
        public static void AddLibrary(Env env) {
            env.Bind("abs", new UnaryNumberProc(Math.Abs));
            env.Bind("log", new UnaryNumberProc(Math.Log));
            env.Bind("sin", new UnaryNumberProc(Math.Sin));
            env.Bind("cos", new UnaryNumberProc(Math.Cos));
            env.Bind("acos", new UnaryNumberProc(Math.Acos));
            env.Bind("asin", new UnaryNumberProc(Math.Asin));
            env.Bind("tan", new UnaryNumberProc(Math.Tan));
            env.Bind("atan", new UnaryNumberProc(Math.Atan));

            env.Bind("+", Number.ADD);
            env.Bind("-", Number.SUB);
            env.Bind("*", Number.MULT);
            env.Bind("/", Number.DIV);
            env.Bind("=", Number.EQUALS);
            env.Bind("<", Number.LESSTHAN);
            env.Bind("<=", Number.LESSOREQUALTHAN);
            env.Bind(">", Number.GREATERTHAN);
            env.Bind(">=", Number.GREATEROREQUALTHAN);

            env.Bind("number?", IsNumber);
            env.Bind("integer?", IsInteger);
            env.Bind("real?", IsReal);
            env.Bind("boolean?", IsBoolean);
            env.Bind("procedure?", IsProcedure);
            env.Bind("symbol?", IsSymbol);
            env.Bind("list?", IsList);
            env.Bind("equal?", Equal);
            env.Bind("pair?", IsPair);
            env.Bind("eq?", Eq);

            env.Bind("print", Print);

            env.Bind("not", Not);
            env.Bind("string-append", StringAppend);
            env.Bind("length", Length);
            env.Bind("nth", Nth);
            env.Bind("map", Map);
            env.Bind("foldl", Foldl);
            env.Bind("list", List);
            env.Bind("apply", Apply);
            env.Bind("append", Append);
            env.Bind("cons", Cons);
            env.Bind("car", Car);
            env.Bind("cdr", Cdr);

            Parser.Load("lib/stdlib.usc", env);
        }
    }
}