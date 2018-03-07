using System;
using System.IO;
using System.Text;

namespace UScheme {
    public class IOLib {

        static Exp Read(Port port) {
            return null;
        }

        static Exp Write(Port port, Exp exp) {
            return WriteString(port, exp.ToString());
        }

        static Exp Display(Port port, Exp exp) {
            return Write(port, exp);
        }

        static Exp WriteString(Port port, string str) {
            foreach (var ch in str)
                port.WriteChar(new Character(ch));

            return SpecialObject.OK;
        }

        static Port GetPortOrDefault(Cell parameters, Port defaultPort) {
            return (parameters.IsNull) ? defaultPort : parameters.First as Port;
        }

        static Procedure IsEof = new CSharpProcedure(parameters => {
            return Boolean.Get(SpecialObject.IsEOF(parameters.First));
        });

        static Procedure CallWithFirstAsPort(Func<Port, Exp> call) {
            return new CSharpProcedure(parameters => call(parameters.First as Port));
        }

        static Procedure CallWithPortOrDefault(Func<Port, Exp> call, Port defaultPort) {
            return new CSharpProcedure(parameters => {
                var port = GetPortOrDefault(parameters, defaultPort);
                return call(port);
            });
        }

        static Procedure CallWithPortOrDefault(Func<Port, Exp, Exp> call, Port defaultPort) {
            return new CSharpProcedure(parameters => {
                var obj = parameters.First;
                var port = GetPortOrDefault(parameters.Rest(), defaultPort);
                return call(port, obj);
            });
        }
        static Procedure OpenInputFile = new CSharpProcedure(parameters => {
            var file = File.OpenText((parameters.First as UString).str);
            return new TextReaderPort(file);
        });

        static Procedure OpenOutputFile = new CSharpProcedure(parameters => {
            var file = File.OpenWrite((parameters.First as UString).str);
            return new TextWriterPort(new StreamWriter(file));
        });

        public static void AddLibrary(Env env) {
            env.Bind("port?", StdLib.IsA<Port>());
            env.Bind("input-port?", CallWithFirstAsPort(port => Boolean.Get(port != null && port.IsInput) ));
            env.Bind("output-port?", CallWithFirstAsPort(port => Boolean.Get(port != null && port.IsOutput)));

            env.Bind("read", CallWithPortOrDefault(Read, Env.Global.CurrentInput));
            env.Bind("read-char", CallWithPortOrDefault(port => port.ReadChar(), Env.Global.CurrentInput));
            env.Bind("peek-char", CallWithPortOrDefault(port => port.PeekChar(), Env.Global.CurrentInput));
            env.Bind("char-ready?", CallWithPortOrDefault(port => port.CharReady(), Env.Global.CurrentInput));

            env.Bind("eof-object?", IsEof);

            env.Bind("write", CallWithPortOrDefault(Write, Env.Global.CurrentOutput));
            env.Bind("display", CallWithPortOrDefault(Display, Env.Global.CurrentOutput));
            env.Bind("newline", CallWithPortOrDefault(port => port.WriteChar(Character.Newline), Env.Global.CurrentOutput));
            env.Bind("write-char", CallWithPortOrDefault(port => port.WriteChar(Character.Newline), Env.Global.CurrentOutput));

            env.Bind("current-input-port", new CSharpProcedure(parameters => env.CurrentInput));
            env.Bind("current-output-port", new CSharpProcedure(parameters => env.CurrentInput));

            env.Bind("open-input-file", OpenInputFile);
            env.Bind("open-output-file", OpenOutputFile);
        }
    }
}