using System;
using System.Linq;
using System.Text;

namespace UScheme {
    partial class StdLib {

        public static void EnsureArity(UList list, int size) {
            if (list.Count != size) {
                throw new EvalException("procedure accepts only " + size + " arguments, " + list.Count + " provided");
            }
        }
        public static void EnsureArityMin(UList list, int size) {
            if (list.Count < size) {
                throw new EvalException("procedure accepts only " + size + " arguments, " + list.Count + " provided");
            }
        }

        private static Procedure BuildIsProcedure<T>(Func<T, bool> evalFunction = null) where T : Exp {
            return new Procedure((UList parameters, Env env) => {
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
        private static readonly Procedure IsList = BuildIsProcedure<UList>();

        private static readonly Procedure List = new Procedure((UList parameters, Env env) => {
            return parameters;
        });

        private static readonly Procedure Equal = new Procedure((UList parameters, Env env) => {
            EnsureArity(parameters, 2);
            return Boolean.Get(parameters.First.UEquals(parameters.Second));
        });

        private static readonly Procedure Not = new Procedure((UList parameters, Env env) => {
            EnsureArity(parameters, 1);
            return (parameters.First == Boolean.FALSE) ? Boolean.TRUE : Boolean.FALSE;
        });

        private static readonly Procedure Car = new Procedure((UList parameters, Env env) => {
            EnsureArity(parameters, 1);
            var pair = parameters.First as UList;
            return pair.First;
        });

        private static readonly Procedure Cdr = new Procedure((UList parameters, Env env) => {
            EnsureArity(parameters, 1);
            var pair = parameters.First as UList;
            return pair.Second;
        });

        private static readonly Procedure Cons = new Procedure((UList parameters, Env env) => {
            EnsureArity(parameters, 2);
            return new UList { parameters.First, parameters.Second };
        });

        private static readonly Procedure Length = new Procedure((UList parameters, Env env) => {
            EnsureArity(parameters, 1);
            return new IntegerNumber((parameters.First as UList).Count);
        });

        private static readonly Procedure Append = new Procedure((UList parameters, Env env) => {
            EnsureArityMin(parameters, 1);

            var newList = new UList();
            for (int i = 0 ; i < parameters.Count; i++) {
                var sublist = parameters[i] as UList;
                if (sublist == null)
                    throw new EvalException("append parameter " + (i + 1) + " is not a list");

                newList.AddRange(sublist);
            }
            return newList;
        });

        private static readonly Procedure Apply = new Procedure((UList parameters, Env env) => {
            EnsureArityMin(parameters, 2);
            var procedure = parameters.First as Procedure;
            var listParameter = parameters.Second as UList;

            if (parameters.Count > 2) {
                UList firstArgs = new UList();
                foreach (Exp e in parameters.GetRange(1, parameters.Count - 2)) {
                    firstArgs.Add(e);
                }
                firstArgs.AddRange(listParameter);
                listParameter = firstArgs;
            }
            return procedure.Eval(listParameter);
        });

        private static readonly Procedure Map = new Procedure((parameters, env) => {
            var proc = parameters.First as Procedure;
            var args = parameters.Tail();
            return new UList(args.Select(arg => proc.Eval(arg)));
        });

        public static Exp FoldlBase(Procedure op, UList parameters, Env env) {
            EnsureArityMin(parameters, 2);
            var value = parameters.First;

            for (int i = 1 ; i < parameters.Count ; i++)
                value = op.Eval(new UList() { value, parameters[i] });

            return value;
        }

        private static readonly Procedure Foldl = new Procedure((parameters, env) => {
            EnsureArity(parameters, 3);
            var op = parameters.First as Procedure;
            var value = parameters.Second;
            var list = parameters.Third as UList;
            foreach (var e in list)
                value = op.Eval(new UList() { value, e });
            
            return value;
        });

        private static readonly Procedure Print = new Procedure((UList parameters, Env env) => {
            Exp ev = parameters.First;
            // TODO: output should be configurable somewhere
            Console.Out.WriteLine(ev.ToString());
            return ev;
        });

        private static readonly Procedure StringAppend = new Procedure((UList parameters, Env env) => {
            var sb = new StringBuilder();
            foreach (Exp substring in parameters)
                sb.Append((substring as UString).str);
            
            return new UString(sb.ToString());
        });

        private static readonly Procedure Nth = new Procedure((UList parameters, Env env) => {
            EnsureArity(parameters, 2);
            var index = parameters.First as IntegerNumber;
            var list = parameters.Second as UList;
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
            env.Bind("eq?", Equal);

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

            UReader.Load("lib/stdlib.usc", env);
        }
    }
}