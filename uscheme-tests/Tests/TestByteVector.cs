using NUnit.Framework;
using System.Collections.Generic;

namespace UScheme.Tests {
    [TestFixture]
    public class TestByteVector : TestConstants {

        [Test]
        public void CreateFromExpList() {
            var exps = new List<Exp> { Number1, Number2, Number100, Number200 };
            var bv = ByteVector.FromList(exps);
            Assert.AreEqual((byte)1, bv[0]);
            Assert.AreEqual((byte)2, bv[1]);
            Assert.AreEqual((byte)100, bv[2]);
            Assert.AreEqual((byte)200, bv[3]);
        }
    }
}
