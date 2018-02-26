namespace UScheme {
    public interface Exp {
        bool UEquals(Exp other);
        Exp Clone();
    }
}