namespace UScheme {
    public interface Evaluator {
        Exp Eval(Exp exp, Env env);
    }
}