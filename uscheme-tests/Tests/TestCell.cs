using NUnit.Framework;
using System.Collections.Generic;

namespace UScheme.Tests {

    [TestFixture]
    public class TestCell : TestConstants {

        static Cell CList;
        static Cell ABACList;
        static Cell ABCell;
        static Cell CABCell;
        static Cell ABCCellFromList;
        static Cell ABCCellFromArray;
        static Cell BCCellFromList;

        [SetUp]
        public void SetUp() {
            ABCell = new Cell(SymbolA, SymbolB);
            CABCell = new Cell(SymbolC, ABCell);
            ABCCellFromList = Cell.BuildList(new List<Exp> { SymbolA, SymbolB, SymbolC });
            ABCCellFromArray = Cell.BuildList(SymbolA, SymbolB, SymbolC);
            BCCellFromList = Cell.BuildList(SymbolB, SymbolC);
            CList = Cell.BuildList(SymbolC);
            ABACList = Cell.BuildList(SymbolA, SymbolB, SymbolA, SymbolC);
        }

        [Test]
        public void CarReturnsCar() {
            Assert.AreEqual(SymbolA, ABCell.car);
        }

        [Test]
        public void CdrReturnsCdr() {
            Assert.AreEqual(SymbolB, ABCell.cdr);
        }

        [Test]
        public void NullIsNull() {
            Assert.IsTrue(Cell.Null.IsNull);
        }

        [Test]
        public void NullIsList() {
            Assert.IsTrue(Cell.Null.IsList);
        }

        [Test]
        public void SimplePairIsNotList() {
            Assert.IsFalse(ABCell.IsList);
        }

        [Test]
        public void SimplePairIsNotNull() {
            Assert.IsFalse(ABCell.IsNull);
        }

        [Test]
        public void NestedSimplePairIsNotList() {
            Assert.IsFalse(CABCell.IsList);
        }

        [Test]
        public void BuildFromListIsList() {
            Assert.IsTrue(ABCCellFromArray.IsList);
            Assert.IsTrue(ABCCellFromList.IsList);
        }

        [Test]
        public void ListConstructionMethodsAreEquivalent() {
            Assert.IsTrue(ABCCellFromArray.UEquals(ABCCellFromList));
            Assert.IsTrue(ABCCellFromList.UEquals(ABCCellFromArray));
        }

        [Test]
        public void BuildsUnitaryList() {
            var list = Cell.BuildList(SymbolC);
            Assert.AreEqual(1, list.Length());
            Assert.IsTrue(list.IsList);
            Assert.IsFalse(list.IsNull);
            Assert.IsTrue(SymbolC.UEquals(list.First));
        }

        [Test]
        public void PositionFunctions() {
            var list = Cell.BuildList(SymbolA, SymbolB, SymbolA, SymbolC);

            Assert.IsTrue(SymbolA.Equals(list.First));
            Assert.IsTrue(SymbolB.Equals(list.Second));
            Assert.IsTrue(SymbolA.Equals(list.Third));
            Assert.IsTrue(SymbolC.Equals(list.Fourth));
        }

        [Test]
        public void BuildsEmptyListIfNoElements() {
            var list = Cell.BuildList();
            Assert.IsTrue(list.IsNull);
            Assert.IsTrue(list.IsList);
            Assert.AreEqual(0, list.Length());
        }

        [Test]
        public void BuildFromEmptyListIsNull() {
            Assert.IsTrue(Cell.BuildList(new List<Exp>()).IsList);
            Assert.IsTrue(Cell.BuildList().IsList);
        }

        [Test]
        public void ConsStringForm() {
            Assert.AreEqual("(a . b)", ABCell.ToString());
            Assert.AreEqual("(c . (a . b))", CABCell.ToString());
        }

        [Test]
        public void NullStringForm() {
            Assert.AreEqual("()", Cell.Null.ToString());
        }

        [Test]
        public void ListForm() {
            Assert.AreEqual("(a b c)", ABCCellFromList.ToString());
        }

        [Test]
        public void NthGetsCell() {
            Assert.AreEqual(SymbolA, ABCCellFromList[0]);
        }

        [Test]
        public void IteratesOvelCellList() {
            var values = new List<Exp>();
            foreach (var exp in ABCCellFromList.Iterate()) {
                values.Add(exp);
            }
            Assert.AreEqual(new List<Exp> { SymbolA, SymbolB, SymbolC }, values);
        }

        [Test]
        public void LastReturnsLast() {
            Assert.IsTrue(SymbolC.UEquals(ABACList.Last()));
        }

        [Test]
        public void LastCellReturnsLastCell() {
            var lastCell = ABCCellFromList.LastCell();
            Assert.AreEqual(SymbolC, lastCell.car);
            Assert.AreEqual(Cell.Null, lastCell.cdr);
        }

        [Test]
        public void RestReturnsRest() {
            Assert.IsTrue(BCCellFromList.UEquals(ABCCellFromList.Rest()));
        }

        [Test]
        public void CannotAddToNullList() {
            var list = Cell.Null;
            Assert.Throws<UException>(() => list.Add(SymbolC));
        }

        [Test]
        public void DuplicateDuplicates() {
            var dup = Cell.Duplicate(ABACList);
            Assert.IsTrue(dup.UEquals(ABACList));
        }

        [Test]
        public void DuplicateDoesNotModifyOriginal() {
            var dup = Cell.Duplicate(ABACList);
            dup.car = SymbolC;
            Assert.IsFalse(dup.UEquals(ABACList));
            Assert.AreEqual(SymbolC, dup.car);
            Assert.AreEqual(SymbolA, ABACList.car);
        }

        [Test]
        public void AppendAppends() {
            var list = Cell.Append(ABACList, CList);
            var expected = Cell.BuildList(SymbolA, SymbolB, SymbolA, SymbolC, SymbolC);
            Assert.IsTrue(expected.UEquals(list));
        }

        [Test]
        public void AppendDoesNotModifyOriginal() {
            var list = Cell.Append(ABACList, CList);
            var expected = Cell.BuildList(SymbolA, SymbolB, SymbolA, SymbolC, SymbolC);
            list.car = SymbolB;
            Assert.AreEqual(SymbolA, ABACList.car);
        }
    }
}
