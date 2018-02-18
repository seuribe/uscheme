using System;

namespace UScheme {
    class UException : Exception {
        public UException(string message) : base(message) { }
    }
}