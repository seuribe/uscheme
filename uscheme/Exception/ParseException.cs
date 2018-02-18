using System;

namespace UScheme {
    class ParseException : UException {
        public ParseException(string message) : base(message) { }
    }
}