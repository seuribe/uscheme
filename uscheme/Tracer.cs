using System;

namespace UScheme {
    public static class Tracer {
        public static bool TraceAtom { get; set; }
        public static bool TraceEval { get; set; }
        public static bool TraceSet { get; set; }

        public static void Atom(string str) {
            if (TraceAtom)
                Trace("Atom from {0}", str);
        }

        public static void Bind(string name, Exp value) {
            if (TraceAtom)
                Trace("Bind '{0}' <- {1}", name, value.ToString());
        }

        public static void Eval(Exp str) {
            if (TraceEval)
                Trace("Eval {0}", str.ToString());
        }

        public static void Eval(string str, params object[] ps) {
            if (TraceEval)
                Trace(str, ps);
        }

        public static void Trace(string str, params object[] ps) {
            Console.Out.WriteLine(string.Format(str, ps));
        }
    }
}