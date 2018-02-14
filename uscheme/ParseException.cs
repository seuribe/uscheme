using System;

namespace UScheme {
    class ParseException : Exception
    {
        private string p;

        public ParseException(string p)
        {
            this.p = p;
        }
    }
}