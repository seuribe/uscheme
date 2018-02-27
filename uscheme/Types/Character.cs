namespace UScheme {
    public class Character : Exp {
        public readonly char character;

        public Character(char character) {
            this.character = character;
        }

        public static bool TryParse(string token, out Character ch) {
            ch = (token.StartsWith("#\\")) ? Parse(token) : null;
            return ch != null;
        }

        static Character Parse(string str) {
            if (str.Length == 2)
                return new Character(' ');

            return new Character(str.Substring(2)[0]);
        }

        public Exp Clone() {
            return this;
        }

        public bool UEquals(Exp other) {
            var asChar = other as Character;
            return asChar != null && asChar.character == character;
        }
    }
}