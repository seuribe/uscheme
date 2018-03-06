using System;
using System.Collections.Generic;
using System.Text;

namespace UScheme {

    public class ByteVector : Exp {
        readonly byte[] data;

        public byte this[int index] {
            get { return data[index]; }
            set { data[index] = value; }
        }

        public ByteVector(byte[] data) {
            this.data = data;
        }

        public ByteVector(List<Exp> elements) {
            this.data = new byte[elements.Count];
            for (int i = 0 ; i < data.Length ; i++)
                data[i] = (byte)((elements[i] as Number).IntValue);
        }

        public Exp Clone() {
            return new ByteVector(data);
        }

        public bool UEquals(Exp other) {
            return other == this ||
                ((other is ByteVector) && ByteArrayEquals(data, (other as ByteVector).data));
        }

        static bool ByteArrayEquals(byte[] a, byte[] b) {
            if (a.Length != b.Length)
                return false;

            for (int i = 0 ; i < a.Length ; i++)
                if (a[i] != b[i])
                    return false;

            return true;
        }

        public override string ToString() {
            var sb = new StringBuilder();
            sb.Append("#vu8(");
            for (int i = 0 ; i < data.Length ; i++)
                sb.Append((int)(data[i]));
            sb.Append(")");
            return sb.ToString();
        }
    }
}
