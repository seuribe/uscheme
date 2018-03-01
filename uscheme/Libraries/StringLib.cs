using System.Text;

namespace UScheme {
    public class StringLib {

        private static readonly Procedure StringAppend = new CSharpProcedure(parameters => {
            var sb = new StringBuilder();
            foreach (Exp substring in parameters.Iterate())
                sb.Append((substring as UString).str);

            return new UString(sb.ToString());
        });

        private static readonly Procedure StringLength = new CSharpProcedure(parameters => {
            StdLib.EnsureIs<UString>(parameters.First);
            return new IntegerNumber((parameters.First as UString).str.Length);
        });

        private static readonly Procedure MakeString = new CSharpProcedure(parameters => {
            StdLib.EnsureArityWithin(parameters, 1, 2);
            StdLib.EnsureIs<IntegerNumber>(parameters.First);
            int len = (parameters.First as IntegerNumber).IntValue;
            char ch = '\u0000';
            if (parameters.Length() == 2)
                ch = (parameters.Second as Character).character;

            var builder = new StringBuilder();
            for (int i = 0 ; i < len ; i++)
                builder.Append(ch);

            return new UString(builder.ToString());
        });

        private static readonly Procedure NumberToString = new CSharpProcedure(parameters => {
            StdLib.EnsureArity(parameters, 1);
            return new UString((parameters.First as Number).ToString());
        });

        public static void AddLibrary(Env env) {
            env.Bind("string-append", StringAppend);
            env.Bind("string-length", StringLength);
            env.Bind("make-string", MakeString);
            env.Bind("number->string", NumberToString);
        }
    }
}