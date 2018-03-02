
using System.Collections.Generic;

namespace UScheme {

    // Top level expression returned when parsing something.
    public class Program : Exp {

        public readonly List<Exp> forms;

        public Program(List<Exp> forms) {
            this.forms = forms;
        }

        public Exp Clone() {
            return this;
        }

        public bool UEquals(Exp other) {
            return other == this;
        }
    }
}