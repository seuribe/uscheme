
using System;

namespace UScheme {
    public abstract class CharConstants {
        public const char ParensOpen = '(';
        public const char BracketOpen = '[';
        public const char ParensClose = ')';
        public const char BracketClose = ']';
        public const char Hash = '#';
        public const char Dot = '.';
        public const char Space = ' ';
        public const char SemiColon = ';';
        public const char DoubleQuote = '"';
        public const char Quote = '\'';
        public const char Slash = '\\';
        public const char Tab = '\t';
        public const char Newline = '\n';
        public const char CarriageReturn = '\r';

        public static bool IsNotNewline(int ch) => ch != Newline;
        public static bool IsWhitespace(int ch) => Array.IndexOf(WhitespaceChars, ch) != -1;
        public static bool IsAtomEnd(int ch) => Array.IndexOf(AtomEndChars, ch) != -1;

        public static bool IsParensClose(int ch) => ch == ParensClose || ch == BracketClose;
        public static bool IsParensOpen(int ch) => ch == ParensOpen || ch == BracketOpen;
        public static bool IsQuoted(char[] chars) => chars[0] == Quote;

        public static readonly int[] AtomEndChars = new int[] {
            Space, Tab, Newline, CarriageReturn, ParensClose, BracketClose };

        public static readonly int[] WhitespaceChars = new int[] {
            Space, Tab, Newline, CarriageReturn
        };

    }
}
