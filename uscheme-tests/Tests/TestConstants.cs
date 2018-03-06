using System.Collections.Generic;

namespace UScheme.Tests {
    public class TestConstants {
        protected static readonly string SimpleSum = "(+ 1 2)";
        protected static readonly List<string> SimpleSumTokens = new List<string> { "(", "+", "1", "2", ")" };
        protected static readonly string NestedSum = "(+ 3 (+ 1 2) 4)";
        protected static readonly List<string> NestedSumTokens = new List<string> { "(", "+", "3", "(", "+", "1", "2", ")", "4", ")" };

        protected static readonly IntegerNumber SimpleSumResult = new IntegerNumber(3);
        protected static readonly IntegerNumber NestedSumResult = new IntegerNumber(10);

        protected static readonly Identifier PlusSymbol = Identifier.From("+");
        protected static readonly IntegerNumber Number1 = new IntegerNumber(1);
        protected static readonly IntegerNumber Number2 = new IntegerNumber(2);
        protected static readonly IntegerNumber Number3 = new IntegerNumber(3);
        protected static readonly IntegerNumber Number4 = new IntegerNumber(4);
        protected static readonly IntegerNumber Number100 = new IntegerNumber(100);
        protected static readonly IntegerNumber Number200 = new IntegerNumber(200);

        protected static readonly Cell SimpleSumForm = Cell.BuildList(PlusSymbol, Number1, Number2);
        protected static readonly Cell NestedSumForm = Cell.BuildList(
            PlusSymbol, Number3, Cell.BuildList(PlusSymbol, Number1, Number2), Number4);


        protected static readonly Identifier A = Identifier.From("a");
        protected static readonly Identifier B = Identifier.From("b");
        protected static readonly Identifier C = Identifier.From("c");
        protected static readonly Identifier D = Identifier.From("d");
    }
}
