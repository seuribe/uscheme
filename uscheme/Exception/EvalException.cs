namespace UScheme {
    public class EvalException : UException {
        public EvalException(string message) : base(message) { }
        public EvalException(Exp exp) : base(exp.ToString()) { }
    }
}