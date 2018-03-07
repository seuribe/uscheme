using System;
using System.IO;

namespace UScheme {
    public class TextWriterPort : Port {
        readonly TextWriter writer;
        bool closed = false;

        public TextWriterPort(TextWriter writer) : base(output: true) {
            this.writer = writer;
        }

        public override bool CharReady() {
            throw new NotImplementedException();
        }

        public override int PeekChar() {
            throw new NotImplementedException();
        }

        public override int ReadChar() {
            throw new NotImplementedException();
        }

        public override void Close() {
            writer.Flush();
            writer.Close();
            closed = true;
        }

        public override void WriteChar(char ch) {
            if (!closed)
                writer.Write(ch);
        }
    }
}
