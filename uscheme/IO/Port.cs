namespace UScheme {
    public abstract class Port : Exp {
        public bool IsInput { get; private set; }
        public bool IsOutput { get; private set; }

        public Port(bool input = false, bool output = false) {
            IsInput = input;
            IsOutput = output;
        }

        public abstract Exp ReadChar();
        public abstract Exp PeekChar();
        public abstract Exp CharReady();

        public abstract Exp WriteChar(Character ch);

        public Exp Clone() => this;
        public bool UEquals(Exp other) => other == this;
    }
}
