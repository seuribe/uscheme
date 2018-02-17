using System;
using System.Linq;
using System.Text;

namespace UScheme {
    partial class StdLib {

        public static void EnsureArity(UList list, int size) {
            if (list.Count != size) {
                throw new Exception("procedure accepts only " + size + " arguments, " + list.Count + " provided");
            }
        }
        public static void EnsureArityMin(UList list, int size) {
            if (list.Count < size) {
                throw new Exception("procedure accepts only " + size + " arguments, " + list.Count + " provided");
            }
        }

        private static readonly Procedure IsNumber = new Procedure((UList list, Env env) => {
            EnsureArity(list, 1);
            return Boolean.Get(list[0] is Number);
        });
        private static readonly Procedure IsInteger = new Procedure((UList list, Env env) => {
            EnsureArity(list, 1);
            return Boolean.Get(list[0] is IntegerNumber);
        });
        private static readonly Procedure IsReal = new Procedure((UList list, Env env) => {
            EnsureArity(list, 1);
            return Boolean.Get(list[0] is RealNumber || list[0] is IntegerNumber);
        });
        private static readonly Procedure IsProcedure = new Procedure((UList list, Env env) => {
            EnsureArity(list, 1);
            return Boolean.Get(list[0] is Procedure);
        });
        private static readonly Procedure IsSymbol = new Procedure((UList list, Env env) => {
            EnsureArity(list, 1);
            return Boolean.Get(list[0] is Symbol);
        });
        private static readonly Procedure IsBoolean = new Procedure((UList list, Env env) => {
            EnsureArity(list, 1);
            return Boolean.Get(list[0] is Boolean);
        });
        private static readonly Procedure IsList = new Procedure((UList list, Env env) => {
            EnsureArity(list, 1);
            return Boolean.Get((UScheme.Eval(list[0], env) as UList).Count >= 2);
        });

        private static readonly Procedure List = new Procedure((UList list, Env env) => {
            return new UList(list.Select(e => UScheme.Eval(e, env)));
        });

        private static readonly Procedure Equal = new Procedure((UList list, Env env) => {
            EnsureArity(list, 2);
            Exp a = UScheme.Eval(list[0], env);
            Exp b = UScheme.Eval(list[1], env);
            return Boolean.Get(a.UEquals(b));
        });
        private static readonly Procedure Not = new Procedure((UList list, Env env) => {
            EnsureArity(list, 1);
            Exp v = UScheme.Eval(list[0], env);
            return (v == Boolean.FALSE) ? Boolean.TRUE : Boolean.FALSE;
        });
        private static readonly Procedure Car = new Procedure((UList list, Env env) => {
            EnsureArity(list, 1);
            var pair = UScheme.Eval(list.First, env) as UList;
            return pair.First;
        });
        private static readonly Procedure Cdr = new Procedure((UList list, Env env) => {
            EnsureArity(list, 1);
            var pair = UScheme.Eval(list.First, env) as UList;
            return pair.Second;
        });
        private static readonly Procedure Cons = new Procedure((UList list, Env env) => {
            EnsureArity(list, 2);
            UList ret = new UList();
            foreach (Exp e in list) {
                ret.Add(UScheme.Eval(e, env));
            }
            return ret;
        });
        private static readonly Procedure Length = new Procedure((UList list, Env env) => {
            EnsureArity(list, 1);
            return new IntegerNumber((UScheme.Eval(list[0], env) as UList).Count);
        });

        private static readonly Procedure Append = new Procedure((UList list, Env env) => {
            EnsureArityMin(list, 1);
            UList ret = new UList();
            foreach (Exp e in list) {
                Exp r = UScheme.Eval(e, env);
                if (r is UList) {
                    ret.AddRange(r as UList);
                } else {
                    throw new Exception("Expected list, got " + r.ToString());
                }
            }
            return ret;
        });

        private static readonly Procedure Apply = new Procedure((UList list, Env env) => {
            EnsureArityMin(list, 2);
            Procedure proc = UScheme.Eval(list[0], env) as Procedure;
            UList listArgs = UScheme.Eval(list[list.Count-1], env) as UList;

            if (list.Count > 2) {
                UList firstArgs = new UList();
                foreach (Exp e in list.GetRange(1, list.Count - 2)) {
                    firstArgs.Add(UScheme.Eval(e, env));
                }
                firstArgs.AddRange(listArgs);
                listArgs = firstArgs;
            }
            return proc.Eval(listArgs, env);
        });

        private static readonly Procedure Map = new Procedure((fparams, env) => {
            var proc = UScheme.Eval(fparams[0], env) as Procedure;
            var args = fparams.Tail();
            return new UList(args.Select(arg => proc.Eval(UScheme.Eval(arg, env), env)));
        });

        public static Exp FoldlBase(Procedure op, UList fparams, Env env) {
            EnsureArityMin(fparams, 2);
            var value = UScheme.Eval(fparams[0], env);

            for (int i = 1 ; i < fparams.Count ; i++)
                value = op.Eval(new UList() { value, UScheme.Eval(fparams[i], env) }, env);

            return value;
        }

        private static readonly Procedure Foldl = new Procedure((fparams, env) => {
            EnsureArity(fparams, 3);
            var op = UScheme.Eval(fparams[0], env) as Procedure;
            var value = UScheme.Eval(fparams[1], env);
            var list = UScheme.Eval(fparams[2], env) as UList;
            foreach (var e in list) {
                value = op.Eval(new UList() { value, UScheme.Eval(e, env) }, env);
            }
            return value;
        });

        private static readonly Procedure Print = new Procedure((UList list, Env env) => {
            Exp ev = UScheme.Eval(list[0], env);
            Console.Out.WriteLine(ev.ToString());
            return ev;
        });

        private static readonly Procedure StringAppend = new Procedure((UList list, Env env) => {
            StringBuilder sb = new StringBuilder();
            foreach (Exp e in list) {
                Exp val = UScheme.Eval(e, env);
                sb.Append((val as UString).str);
            }
            return new UString(sb.ToString());
        });

        // TODO: for-each, cons, pair?, eval, zip, foldr, compose
        public static Env AddProcedures(Env env) {
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

            env.Bind("print", Print);

            env.Bind("not", Not);
            env.Bind("string-append", StringAppend);
            env.Bind("length", Length);
            env.Bind("map", Map);
            env.Bind("foldl", Foldl);
            env.Bind("list", List);
            env.Bind("apply", Apply);
            env.Bind("append", Append);
            env.Bind("cons", Cons);
            env.Bind("car", Car);
            env.Bind("cdr", Cdr);
            return env;
        }
    }
}