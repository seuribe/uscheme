using System.Collections.Generic;

namespace UScheme.Tests {
    public class TestConstants {
        protected static readonly string SimpleSum = "(+ 1 2)";
        protected static readonly List<string> SimpleSumTokens = new List<string> { "(", "+", "1", "2", ")" };
        protected static readonly string NestedSum = "(+ 3 (+ 1 2) 4)";
        protected static readonly List<string> NestedSumTokens = new List<string> { "(", "+", "3", "(", "+", "1", "2", ")", "4", ")" };

        protected static readonly Symbol PlusSymbol = Symbol.From("+");
        protected static readonly IntegerNumber Number1 = new IntegerNumber(1);
        protected static readonly IntegerNumber Number2 = new IntegerNumber(2);
        protected static readonly IntegerNumber Number3 = new IntegerNumber(3);
        protected static readonly IntegerNumber Number4 = new IntegerNumber(4);

        protected static readonly UList SimpleSumForm = new UList { PlusSymbol, Number1, Number2 };
        protected static readonly UList NestedSumForm = new UList {
            PlusSymbol, Number3, new UList {
                        PlusSymbol, Number1, Number2 },
            Number4 };
    }
}
