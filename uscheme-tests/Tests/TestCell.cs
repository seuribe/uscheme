using NUnit.Framework;
using System.Collections.Generic;

namespace UScheme.Tests {

    [TestFixture]
    public class TestCell : TestConstants {

        static Cell ABCell;
        static Cell CABCell;
        static Cell ABCCellFromList;
        static Cell ABCCellFromArray;

        [SetUp]
        public void SetUp() {
            ABCell = new Cell(SymbolA, SymbolB);
            CABCell = new Cell(SymbolC, ABCell);
            ABCCellFromList = Cell.BuildList(new List<Exp> { SymbolA, SymbolB, SymbolC });
            ABCCellFromArray = Cell.BuildList(SymbolA, SymbolB, SymbolC);
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
    }
}
