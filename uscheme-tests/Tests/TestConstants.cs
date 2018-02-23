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

        protected static readonly Cell SimpleSumForm = Cell.BuildList(PlusSymbol, Number1, Number2);
        protected static readonly Cell NestedSumForm = Cell.BuildList(
            PlusSymbol, Number3, Cell.BuildList(PlusSymbol, Number1, Number2), Number4);


        protected static readonly Symbol SymbolA = Symbol.From("a");
        protected static readonly Symbol SymbolB = Symbol.From("b");
        protected static readonly Symbol SymbolC = Symbol.From("c");
        protected static readonly Symbol SymbolD = Symbol.From("d");
    }
}
