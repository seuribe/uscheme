using System;
using System.IO;

namespace UScheme {
    public class TextReaderPort : Port {
        readonly TextReader reader;

        public TextReaderPort(TextReader reader) : base(input: true) {
            this.reader = reader;
        }

        public override Exp CharReady() {
            return Boolean.Get(reader.Peek() != -1);
        }

        public override Exp PeekChar() {
            return new Character(Convert.ToChar(reader.Peek()));
        }

        public override Exp ReadChar() {
            return new Character(Convert.ToChar(reader.Read()));
        }

        public override Exp WriteChar(Character ch) {
            throw new NotImplementedException();
        }
    }
}
