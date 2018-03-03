using NUnit.Framework;
using System.Collections.Generic;

namespace UScheme.Tests {

    [TestFixture]
    public class TestCell : TestConstants {

        static Cell CList;
        static Cell ABACList;
        static Cell CABAList;
        static Cell ABCell;
        static Cell CABCell;
        static Cell ABCCellFromList;
        static Cell ABCCellFromArray;
        static Cell BCCellFromList;

        [SetUp]
        public void SetUp() {
            ABCell = new Cell(A, B);
            CABCell = new Cell(C, ABCell);
            ABCCellFromList = Cell.BuildList(new List<Exp> { A, B, C });
            ABCCellFromArray = Cell.BuildList(A, B, C);
            BCCellFromList = Cell.BuildList(B, C);
            CList = Cell.BuildList(C);
            ABACList = Cell.BuildList(A, B, A, C);
            CABAList = Cell.BuildList(C, A, B, A);
        }

        [Test]
        public void CarReturnsCar() {
            Assert.AreEqual(A, ABCell.car);
        }

        [Test]
        public void CdrReturnsCdr() {
            Assert.AreEqual(B, ABCell.cdr);
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
            var list = Cell.BuildList(C);
            Assert.AreEqual(1, list.Length());
            Assert.IsTrue(list.IsList);
            Assert.IsFalse(list.IsNull);
            Assert.IsTrue(C.UEquals(list.First));
        }

        [Test]
        public void PositionFunctions() {
            var list = Cell.BuildList(A, B, A, C);

            Assert.IsTrue(A.Equals(list.First));
            Assert.IsTrue(B.Equals(list.Second));
            Assert.IsTrue(A.Equals(list.Third));
            Assert.IsTrue(C.Equals(list.Fourth));
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
            Assert.AreEqual(A, ABCCellFromList[0]);
        }

        [Test]
        public void IteratesOvelCellList() {
            var values = new List<Exp>();
            foreach (var exp in ABCCellFromList.Iterate()) {
                values.Add(exp);
            }
            Assert.AreEqual(new List<Exp> { A, B, C }, values);
        }

        [Test]
        public void LastReturnsLast() {
            Assert.IsTrue(C.UEquals(ABACList.Last()));
        }

        [Test]
        public void LastCellReturnsLastCell() {
            var lastCell = ABCCellFromList.LastCell();
            Assert.AreEqual(C, lastCell.car);
            Assert.AreEqual(Cell.Null, lastCell.cdr);
        }

        [Test]
        public void RestReturnsRest() {
            Assert.IsTrue(BCCellFromList.UEquals(ABCCellFromList.Rest()));
        }

        [Test]
        public void CannotAddToNullList() {
            var list = Cell.Null;
            Assert.Throws<UException>(() => list.Add(C));
        }

        [Test]
        public void DuplicateDuplicates() {
            var dup = Cell.Duplicate(ABACList);
            Assert.IsTrue(dup.UEquals(ABACList));
        }

        [Test]
        public void DuplicateDoesNotModifyOriginal() {
            var dup = Cell.Duplicate(ABACList);
            dup.car = C;
            Assert.IsFalse(dup.UEquals(ABACList));
            Assert.AreEqual(C, dup.car);
            Assert.AreEqual(A, ABACList.car);
        }

        [Test]
        public void AppendAppends() {
            var list = Cell.Append(ABACList, CList);
            var expected = Cell.BuildList(A, B, A, C, C);
            Assert.IsTrue(expected.UEquals(list));
        }

        [Test]
        public void AppendDoesNotModifyOriginal() {
            var list = Cell.Append(ABACList, CList);
            var expected = Cell.BuildList(A, B, A, C, C);
            list.car = B;
            Assert.AreEqual(A, ABACList.car);
        }

        [Test]
        public void Reverse() {
            Assert.IsTrue(ABACList.UEquals(CABAList.Reverse()));
        }
    }
}
