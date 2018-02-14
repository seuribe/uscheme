﻿using System.Collections.Generic;
using System.Linq;

namespace UScheme {
    class UList : List<Exp>, Exp {

        public UList() { }

        public UList(IEnumerable<Exp> l) : base(l) { }

        public UList Tail() {
            return new UList(this.Skip(1));
        }

        public override string ToString() {
            return "(" + string.Join(" ", ToStrings()) + ")";
        }

        public List<string> ToStrings() {
            return this.Select(e => e.ToString()).ToList();
        }

        public bool UEquals(Exp other) {
            if (!(other is UList)) {
                return false;
            }
            UList b = other as UList;
            if (Count != b.Count) {
                return false;
            }
            for (int i = 0 ; i < Count ; i++) {
                if (!this[i].UEquals(b[i])) {
                    return false;
                }
            }
            return true;
        }
    }
}