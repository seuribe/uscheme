﻿using System.Text;

namespace UScheme {

    public class StringLib : CoreLib {

        private static readonly Procedure StringAppend = new CSharpProcedure(parameters => {
            var sb = new StringBuilder();
            foreach (Exp substring in parameters.Iterate())
                sb.Append((substring as UString).str);

            return new UString(sb.ToString());
        });

        private static readonly Procedure StringLength = new CSharpProcedure(parameters => {
            EnsureIs<UString>(parameters.First);
            return new IntegerNumber((parameters.First as UString).str.Length);
        });

        private static readonly Procedure MakeString = new CSharpProcedure(parameters => {
            EnsureArityWithin(parameters, 1, 2);
            EnsureIs<IntegerNumber>(parameters.First);
            int len = (parameters.First as IntegerNumber).IntValue;
            char ch = '\u0000';
            if (parameters.Length() == 2)
                ch = (parameters.Second as Character).character;

            var builder = new StringBuilder();
            for (int i = 0 ; i < len ; i++)
                builder.Append(ch);

            return new UString(builder.ToString());
        });

        static readonly Procedure StringFromChars = new CSharpProcedure(parameters => {
            return StringFromCharList(parameters);
        });

        static UString StringFromCharList(Cell chars) {
            var sb = new StringBuilder();
            foreach (Character ch in chars.Iterate())
                sb.Append(ch.character);
            return new UString(sb.ToString());
        }

        private static readonly Procedure StringRef = new CSharpProcedure(parameters => {
            var str = parameters.First as UString;
            var index = parameters.Second as IntegerNumber;
            return new Character(str.str[index.IntValue]);
        });

        private static readonly Procedure NumberToString = new CSharpProcedure(parameters => {
            EnsureArity(parameters, 1);
            return new UString((parameters.First as Number).ToString());
        });

        private static readonly Procedure StringList = new CSharpProcedure(parameters => {
            var str = parameters.First as UString;
            if (str.str.Length == 0)
                return Cell.Null;

            var list = new Cell(new Character(str.str[0]));
            var current = list;
            for (int i = 1 ; i < str.str.Length ; i++)
                current = (current.cdr = new Cell(new Character(str.str[i]))) as Cell;

            return list;
        });

        private static readonly Procedure ListString = new CSharpProcedure(parameters => {
            return StringFromCharList(parameters.First as Cell);
        });

        public static void AddLibrary(Env env) {
            env.Bind("string-append", StringAppend);
            env.Bind("string-length", StringLength);
            env.Bind("make-string", MakeString);
            env.Bind("number->string", NumberToString);
            env.Bind("string", StringFromChars);
            env.Bind("string-ref", StringRef);
            env.Bind("string=?", ListUEqual<UString>());
            env.Bind("string->list", StringList);
            env.Bind("list->string", ListString);
            env.Bind("string-copy", Conversion<UString, Exp>(str => str.Clone()));
        }
    }
}