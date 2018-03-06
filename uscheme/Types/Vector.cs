using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace UScheme {
    public class Vector : BaseVector<Exp>, IEnumerable<Exp>, IEnumerable {

        public Vector(params Exp[] elements) : base(elements) { }

        public static Vector FromList(List<Exp> elements) {
            return new Vector(elements.ToArray());
        }

        public static Vector FromCell(Cell parameters) => FromList(parameters.AsList());

        public override Exp Clone() {
            return new Vector((Exp[])data.Clone());
        }

        public override bool UEquals(Exp other) {
            if (this == other)
                return true;

            var vector = other as Vector;
            if (vector == null || data.Length != vector.data.Length)
                return false;

            for (int i = 0 ; i < Length ; i++)
                if (!data[i].UEquals(vector[i]))
                    return false;

            return true;
        }

        public override string ToString() {
            return "#(" + string.Join(" ", data.Select( e => e.ToString() )) + ")";
        }

        public IEnumerator<Exp> GetEnumerator() {
            return data.GetEnumerator() as IEnumerator<Exp>;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
