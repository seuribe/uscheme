namespace UScheme {
    public class SimpleStringReader {

        public bool Available { get { return index < buffer.Length; } }
        public int Current { get { return Available ? buffer[index] : -1; } }

        readonly char[] buffer;
        int index = 0;

        public SimpleStringReader(string str) {
            buffer = str.ToCharArray();
        }

        public void Advance() {
            if (Available)
                index++;
        }
    }
}