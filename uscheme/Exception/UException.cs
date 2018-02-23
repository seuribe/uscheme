using System;

namespace UScheme {
    public class UException : Exception {
        public UException(string message) : base(message) { }
    }
}