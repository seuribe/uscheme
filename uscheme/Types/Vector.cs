using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace UScheme {
    public class Vector : Exp, IEnumerable<Exp>, IEnumerable {

        public int Length { get { return elements.Length; } }

        readonly Exp[] elements;

        public Exp this[int index] {
            get { return elements[index]; }
            set { elements[index] = value; }
        }

        public Vector(params Exp[] elements) {
            this.elements = elements;
        }

        public Vector(List<Exp> elements) {
            this.elements = elements.ToArray();
        }

        public Vector(Cell parameters) : this(parameters.AsList()) { }

        public Exp Clone() {
            return new Vector(new List<Exp>((Exp[])elements.Clone()));
        }

        public bool UEquals(Exp other) {
            if (this == other)
                return true;

            var vector = other as Vector;
            if (vector == null || elements.Length != vector.elements.Length)
                return false;

            for (int i = 0 ; i < Length ; i++)
                if (!elements[i].UEquals(vector[i]))
                    return false;

            return true;
        }

        public override string ToString() {
            return "#(" + string.Join(" ", elements.Select( e => e.ToString() )) + ")";
        }

        public IEnumerator<Exp> GetEnumerator() {
            return elements.GetEnumerator() as IEnumerator<Exp>;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
