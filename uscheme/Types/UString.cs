
namespace UScheme {
    class UString : Exp {
        public readonly string str;
        public UString(string str) {
            this.str = str;
        }
        public override string ToString() {
            return "\"" + str + "\"";
        }
        public bool UEquals(Exp other) {
            return (other is UString) && str.Equals((other as UString).str);
        }
    }
}