namespace UScheme {
    public struct SpecialObject : Exp {
        public static SpecialObject EOF = new SpecialObject();
        public static SpecialObject OK = new SpecialObject();
        public Exp Clone() => this;
        public bool UEquals(Exp other) => (other is SpecialObject) && Equals(other);

        public static bool IsEOF(Exp exp) => EOF.Equals(exp);
    }
}
