using System;

namespace UScheme {
    public class ParseException : UException {
        public ParseException(string message) : base(message) { }
    }
}