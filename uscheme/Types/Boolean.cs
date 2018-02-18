
namespace UScheme {
    class Boolean : Exp {

        public bool Value { get { return this == TRUE; } }

        public static readonly string TrueString = "#t";
        public static readonly string FalseString = "#f";

        public static Boolean Get(bool value) {
            return value ? TRUE : FALSE;
        }

        public static bool TryParse(string str, out Boolean value) {
            if (TrueString.Equals(str))
                value = TRUE;
            else if (FalseString.Equals(str))
                value = FALSE;
            else
                value = null;

            return (value != null);
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

        public static Boolean TRUE = new Boolean(TrueString);
        public static Boolean FALSE = new Boolean(FalseString);
    }
}
