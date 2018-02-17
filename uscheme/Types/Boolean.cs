using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UScheme {
    class Boolean : Exp {

        public bool Value { get { return this == TRUE; } }

        public static Boolean Get(bool value) {
            return value ? TRUE : FALSE;
        }

        public static bool IsFalse(Exp exp) {
            return exp == FALSE;
        }

        public static bool IsTrue(Exp exp) {
            return exp != FALSE;
        }

        public override string ToString() {
            return str;
        }

        private readonly string str;
        private Boolean(string str) {
            this.str = str;
        }

        public bool UEquals(Exp other) {
            return this == other;
        }

        public static Boolean TRUE = new Boolean("#t");
        public static Boolean FALSE = new Boolean("#f");
    }
}
