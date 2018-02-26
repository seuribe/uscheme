using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace UScheme {

    public class Cell : Exp {
        public Exp car;
        public Exp cdr;

        public bool IsNull => this == Null;
        public bool IsList => IsNull || ((cdr is Cell) && (cdr as Cell).IsList);
        public bool IsPair => this != Null;

        public static readonly Cell Null = new Cell();

        public Exp First { get { return this[0]; } }
        public Exp Second { get { return this[1]; } }
        public Exp Third { get { return this[2]; } }

        public Exp Fourth { get { return this[3]; } }

        public Exp this[int index] {
            get {
                EnsureIsList();
                return NthCell(index).car;
            }
            set {
                EnsureIsList();
                NthCell(index).car = value;
            }
        }

        protected Cell() { }
        public Cell(Exp car, Exp cdr) {
            this.car = car;
            this.cdr = cdr;
        }

        internal List<Exp> AsList() {
            return new List<Exp>(Iterate());
        }

        void EnsureIsList() {
            if (!IsList)
                throw new UException("Cell not a list: " + ToString());
        }
        
        void EnsureNotNull() {
            if (IsNull)
                throw new UException("Cell must be non-null");
        }

        public void Add(Exp exp) {
            EnsureNotNull();
            LastCell().cdr = new Cell(exp, Null);
        }

        public static Cell Duplicate(Cell other) {
            if (other.IsNull)
                return Null;

            if (!other.IsList)
                return new Cell(other.car, other.cdr);

            return DoDuplicate(other);
        }

        static Cell DoDuplicate(Cell other) {
            if (other == Null)
                return Null;
            return new Cell(other.car, DoDuplicate(other.cdr as Cell));
        }

        public void RemoveLast() {
            throw new Exception("not implemented");
        }

        public static Cell Append(Cell first, Cell second) {
            first.EnsureIsList();
            second.EnsureIsList();
            var list = Duplicate(first);
            list.LastCell().cdr = Duplicate(second);
            return list;
        }

        public Exp Last() {
            return LastCell().car;
        }

        public Cell LastCell() {
            EnsureIsList();
            var cell = this;
            while (cell.cdr != Null)
                cell = cell.cdr as Cell;

            return cell;
        }

        public Cell Rest() {
            EnsureIsList();
            return cdr as Cell;
        }

        public Cell Skip(int n) {
            EnsureIsList();
            return DoSkip(n);
        }

        Cell DoSkip(int n) {
            if (n <= 0)
                return this;

            return (cdr as Cell).DoSkip(n - 1);
        }

        public int Length() {
            EnsureIsList();
            return IsNull ? 0 : (1 + Rest().Length());
        }

        public IEnumerable<Exp> Iterate() {
            EnsureIsList();

            var cell = this;
            while (cell != null && !cell.IsNull) {
                yield return cell.car;
                cell = cell.cdr as Cell;
            }
        }

        public Cell Reverse() {
            EnsureIsList();
            var asList = new List<Exp>(Iterate());
            asList.Reverse();
            return BuildList(asList);
        }

        Cell NthCell(int index) {
            if (index == 0)
                return this;

            return (cdr as Cell).NthCell(index - 1);
        }

        // for now, recursive and inefficient. Will replace with something better if/when need arises
        public static Cell BuildList(List<Exp> elements) {
            if (elements.Count == 0)
                return Null;

            return new Cell(elements[0], BuildList(elements.Skip(1).ToList()));
        }

        public static Cell BuildList(params Exp[] elements) {
            return BuildList(elements.ToList());
        }

        public override string ToString() {
            if (IsList) // list takes care of the Null (empty) format
                return ToListString();

            return CharConstants.ParensOpen + car.ToString() + " . " + cdr.ToString() + CharConstants.ParensClose;
        }

        public List<string> ToStringList() {
            var strings = new List<string>();
            foreach (var exp in Iterate())
                strings.Add(exp.ToString());
            return strings;
        }

        string ToListString() {
            var sb = new StringBuilder();
            sb.Append(CharConstants.ParensOpen);
            var cell = this;
            while (cell != Null) {
                sb.Append(cell.car.ToString());
                cell = cell.cdr as Cell;
                if (cell != Null)
                    sb.Append(" ");
            }
            sb.Append(CharConstants.ParensClose);
            return sb.ToString();
        }

        public Exp Clone() {
            return Duplicate(this);
        }

        public bool UEquals(Exp other) {
            if (other == this)
                return true;

            var cell = other as Cell;
            if (cell == null || IsNull != cell.IsNull)
                return false;

            return car.UEquals(cell.car) && cdr.UEquals(cell.cdr);
        }
    }
}