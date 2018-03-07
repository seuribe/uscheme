using System;
using System.IO;

namespace UScheme {
    public class TextWriterPort : Port {
        readonly TextWriter writer;

        public TextWriterPort(TextWriter writer) : base(output: true) {
            this.writer = writer;
        }

        public override Exp CharReady() {
            throw new NotImplementedException();
        }

        public override Exp PeekChar() {
            throw new NotImplementedException();
        }

        public override Exp ReadChar() {
            throw new NotImplementedException();
        }

        public override Exp WriteChar(Character ch) {
            writer.Write(ch.character);
            return SpecialObject.OK;
        }
    }
}
