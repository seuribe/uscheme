namespace UScheme {
    public abstract class Port : Exp {
        public bool IsInput { get; private set; }
        public bool IsOutput { get; private set; }

        public Port(bool input = false, bool output = false) {
            IsInput = input;
            IsOutput = output;
        }

        public abstract int ReadChar();
        public abstract int PeekChar();
        public abstract bool CharReady();

        public abstract void WriteChar(char ch);

        public abstract void Close();

        public Exp Clone() => this;
        public bool UEquals(Exp other) => other == this;
    }
}
