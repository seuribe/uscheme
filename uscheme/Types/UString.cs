
namespace UScheme {
    public class UString : Exp {
        public readonly string str;
        public UString(string str) {
            this.str = str;
        }

        public static bool TryParse(string token, out UString str) {
            str = IsString(token) ? new UString(token.Substring(1, token.Length - 2)) : null;
            return str != null;
        }

        static bool IsString(string token) {
            return token[0] == CharConstants.DoubleQuote && token[token.Length - 1] == CharConstants.DoubleQuote;
        }

        public override string ToString() {
            return CharConstants.DoubleQuote + str + CharConstants.DoubleQuote;
        }
        public bool UEquals(Exp other) {
            return (other is UString) && str.Equals((other as UString).str);
        }
    }
}