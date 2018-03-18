using System.Collections;
using System.Collections.Generic;

namespace UScheme {
    public abstract class BaseVector<T> : Exp, IEnumerable<T>, IEnumerable {
        protected readonly T[] data;

        public T this[int index] {
            get { return data[index]; }
            set { data[index] = value; }
        }

        public BaseVector(T[] data) {
            this.data = data;
        }

        public int Length { get { return data.Length; } }

        public abstract Exp Clone();
        public abstract bool UEquals(Exp other);


        public IEnumerator<T> GetEnumerator() {
            return ((IEnumerable<T>)data).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
