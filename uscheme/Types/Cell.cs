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

        public static Cell Null = new Cell();

        Cell() { }
        public Cell(Exp car, Exp cdr) {
            this.car = car;
            this.cdr = cdr;
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

        public bool UEquals(Exp other) {
            var cell = other as Cell;
            return cell != null && car.UEquals(cell.car) && cdr.UEquals(cell.cdr);
        }
    }
}