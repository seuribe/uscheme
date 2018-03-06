using System;
using System.Collections.Generic;
using System.Text;

namespace UScheme {

    public class ByteVector : BaseVector<byte> {

        public ByteVector(byte[] data) : base(data) { }

        public static ByteVector FromList(List<Exp> elements) {
            var data = new byte[elements.Count];
            for (int i = 0 ; i < data.Length ; i++)
                data[i] = (byte)((elements[i] as Number).IntValue);

            return new ByteVector(data);
        }

        public override Exp Clone() {
            return new ByteVector(data);
        }

        public override bool UEquals(Exp other) {
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
