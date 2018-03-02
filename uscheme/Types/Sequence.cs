
using System.Collections.Generic;

namespace UScheme {

    // Top level expression returned when parsing something.
    public class Sequence : Exp {

        public readonly List<Exp> forms;

        public Sequence(List<Exp> forms) {
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