namespace UScheme {
    public class Character : Exp {

        public static readonly Character Newline = new Character(CharConstants.Newline);

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

        public override string ToString() {
            return string.Format("U+{0:X4} ", System.Convert.ToUInt16(character));
        }

        public bool UEquals(Exp other) {
            var asChar = other as Character;
            return asChar != null && asChar.character == character;
        }
    }
}