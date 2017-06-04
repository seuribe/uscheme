using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UScheme {
/*
    class Pair : Exp {
        public Exp car;
        public Exp cdr;
    }
*/
    class StdLib {

        private static void EnsureArity(UList list, int size) {
            if (list.Count != size) {
                throw new Exception("procedure accepts only " + size + " arguments, " + list.Count + " provided");
            }
        }
        private static void EnsureArityMin(UList list, int size) {
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
            UList ret = new UList();
            list.ForEach(e => ret.Add(UScheme.Eval(e, env)));
            return ret;
        });

        private static readonly Procedure Car = new Procedure((UList list, Env env) => {
            EnsureArityMin(list, 1);
            return list[0];
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
        private static readonly Procedure Cdr = new Procedure((UList list, Env env) => {
            EnsureArityMin(list, 2);
            return list.Tail();
        });
        private static readonly Procedure Cons = new Procedure((UList list, Env env) => {
            EnsureArity(list, 2);
            UList ret = new UList();
            foreach (Exp e in list) {
                ret.Add(env.Eval(e));
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
                    firstArgs.Add(env.Eval(e));
                }
                firstArgs.AddRange(listArgs);
                listArgs = firstArgs;
            }
            return proc.Eval(listArgs, env);
        });

        private static readonly Procedure Map = new Procedure((UList list, Env env) => {
            Procedure proc = UScheme.Eval(list[0], env) as Procedure;
            list = list.Tail();
            // first, eval all parameters
            for (int i = 0 ; i < list.Count ; i++) {
                list[i] = UScheme.Eval(list[i], env);
            }
            //
            UList ret = new UList();
            int len = (list[0] as UList).Count;
            for (int i = 0 ; i < len ; i++) {
                UList args = new UList();
                foreach (UList l in list) {
                    args.Add(l[i]);
                }
                ret.Add(proc.Eval(args, env));
            }
            return ret;
        });

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

        // TODO: for-each, cons, pair?, apply, eval, zip, append, foldl, foldr, compose, (math functions: abs, log, sin, cos, acos, asin, tan, atan)

        public static Env AddProcedures(Env env) {
            env.Put("number?", StdLib.IsNumber);
            env.Put("integer?", StdLib.IsInteger);
            env.Put("real?", StdLib.IsReal);
            env.Put("boolean?", StdLib.IsBoolean);
            env.Put("procedure?", StdLib.IsProcedure);
            env.Put("symbol?", StdLib.IsSymbol);
            env.Put("list?", StdLib.IsList);
            env.Put("equal?", StdLib.Equal);
            env.Put("print", StdLib.Print);
            env.Put("not", StdLib.Not);
            env.Put("string-append", StdLib.StringAppend);
            env.Put("length", StdLib.Length);
            env.Put("map", StdLib.Map);
            env.Put("foldl", StdLib.Foldl);
            env.Put("list", StdLib.List);
            env.Put("apply", StdLib.Apply);
            env.Put("append", StdLib.Append);
            env.Put("cons", StdLib.Cons);
            env.Put("car", StdLib.Car);
            env.Put("cdr", StdLib.Cdr);
            return env;
        }
    }
}