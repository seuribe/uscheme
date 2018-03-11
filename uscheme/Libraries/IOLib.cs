using System;
using System.IO;
using System.Text;

namespace UScheme {
    public class IOLib : CoreLib {

        static Exp Read(Port port) {
            var sb = new StringBuilder();
            var ch = -1;
            while ((ch = port.ReadChar()) != -1)
                sb.Append(Convert.ToChar(ch));

            return Parser.Parse(sb.ToString());
        }

        static Exp Write(Port port, Exp exp) {
            return WriteString(port, exp.ToString());
        }

        static Exp Display(Port port, Exp exp) {
            return Write(port, exp);
        }

        static Exp WriteString(Port port, string str) {
            foreach (var ch in str)
                port.WriteChar(ch);

            return SpecialObject.OK;
        }

        static Exp ReadChar(Port port) {
            var read = port.ReadChar();
            return (read == -1) ? (Exp)SpecialObject.EOF : new Character(Convert.ToChar(read));
        }

        static Exp PeekChar(Port port) {
            var read = port.PeekChar();
            return (read == -1) ? (Exp)SpecialObject.EOF : new Character(Convert.ToChar(read));
        }

        static Exp CharReady(Port port) {
            return Boolean.Get(port.CharReady());
        }

        static Exp WriteNewline(Port port, Exp exp) {
            port.WriteChar(CharConstants.Newline);
            return SpecialObject.OK;
        }

        static Exp WriteChar(Port port, Exp exp) {
            port.WriteChar((exp as Character).character);
            return SpecialObject.OK;
        }

        static Procedure IsEof = new CSharpProcedure(parameters => {
            return Boolean.Get(SpecialObject.IsEOF(parameters.First));
        });

        static Procedure OpenInputFile = new CSharpProcedure(parameters => {
            var file = File.OpenText((parameters.First as UString).str);
            return new TextReaderPort(file);
        });

        static Procedure OpenOutputFile = new CSharpProcedure(parameters => {
            var file = File.OpenWrite((parameters.First as UString).str);
            return new TextWriterPort(new StreamWriter(file));
        });

        public static void AddLibrary(Env env) {
            env.Bind("port?", IsA<Port>());
            env.Bind("input-port?", CallWithFirstAsPort(port => Boolean.Get(port != null && port.IsInput) ));
            env.Bind("output-port?", CallWithFirstAsPort(port => Boolean.Get(port != null && port.IsOutput)));

            env.Bind("read", CallWithPortOrDefaultInput(Read));
            env.Bind("read-char", CallWithPortOrDefaultInput(ReadChar));
            env.Bind("peek-char", CallWithPortOrDefaultInput(PeekChar));
            env.Bind("char-ready?", CallWithPortOrDefaultInput(CharReady));

            env.Bind("eof-object?", IsEof);

            env.Bind("write", CallWithPortOrDefaultOutput(Write));
            env.Bind("display", CallWithPortOrDefaultOutput(Display));
            env.Bind("newline", CallWithPortOrDefaultOutput(WriteNewline));
            env.Bind("write-char", CallWithPortOrDefaultOutput(WriteChar));

            env.Bind("current-input-port", new CSharpProcedure(parameters => env.CurrentInput));
            env.Bind("current-output-port", new CSharpProcedure(parameters => env.CurrentInput));

            env.Bind("open-input-file", OpenInputFile);
            env.Bind("open-output-file", OpenOutputFile);

            env.Bind("close-input-port", CallWithFirstAsPort(ClosePort));
            env.Bind("close-output-port", CallWithFirstAsPort(ClosePort));
        }

        static Exp ClosePort(Port port) {
            port.Close();
            return SpecialObject.OK;
        }

        static Port GetPortOrDefault(Cell parameters, Port defaultPort) {
            return (parameters.IsNull) ? defaultPort : parameters.First as Port;
        }

        static Procedure CallWithFirstAsPort(Func<Port, Exp> call) {
            return new CSharpProcedure(parameters => call(parameters.First as Port));
        }

        static Procedure CallWithPortOrDefaultInput(Func<Port, Exp> call) {
            return new CSharpProcedure(parameters => {
                var port = GetPortOrDefault(parameters, Env.Global.CurrentInput);
                return call(port);
            });
        }

        static Procedure CallWithPortOrDefaultOutput(Func<Port, Exp, Exp> call) {
            return new CSharpProcedure(parameters => {
                var obj = parameters.First;
                var port = GetPortOrDefault(parameters.Rest(), Env.Global.CurrentOutput);
                return call(port, obj);
            });
        }
    }
}