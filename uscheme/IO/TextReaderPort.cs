using System;
using System.IO;

namespace UScheme {
    public class TextReaderPort : Port {
        readonly TextReader reader;
        bool closed = false;

        public TextReaderPort(TextReader reader) : base(input: true) {
            this.reader = reader;
        }

        public override bool CharReady() {
            return !closed && reader.Peek() != -1;
        }

        public override int PeekChar() {
            return closed ? -1 : reader.Peek();
        }

        public override int ReadChar() {
            return closed ? -1 : reader.Read();
        }

        public override void Close() {
            reader.Close();
            closed = true;
        }

        public override void WriteChar(char ch) {
            throw new NotImplementedException();
        }
    }
}
