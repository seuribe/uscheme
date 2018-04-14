namespace UScheme {
    public class CharLib : CoreLib {
        public static void AddLibrary(Env env) {
            env.Bind("char->integer", Conversion<Character, IntegerNumber>(ch => ch.ToInteger()));
        }
    }
}