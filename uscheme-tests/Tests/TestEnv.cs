using NUnit.Framework;

namespace UScheme.Tests {
    [TestFixture]
    public class TestEnv : TestConstants {

        protected Env initialEnv;
        protected Exp evalResult;

        [SetUp]
        public void SetUp() {
            initialEnv = Env.InitialEnv();
        }

        [Test]
        public void BindThenHas() {
            ThenHasNot("a");
            WhenBinding("a", Number1);
            ThenHas("a");
        }

        [Test]
        public void BindThenGet() {
            WhenBinding("a", Number1);
            ThenValueIs("a", Number1);
        }

        [Test]
        public void BindThenFind() {
            WhenBinding("a", Number1);
            ThenFinds("a");
        }

        void WhenBinding(string name, Exp value) {
            initialEnv.Bind(name, value);
        }

        void ThenHas(string name) {
            Assert.IsTrue(initialEnv.Has(name));
        }

        void ThenHasNot(string name) {
            Assert.IsFalse(initialEnv.Has(name));
        }

        void ThenValueIs(string name, Exp value) {
            Assert.IsTrue(value.UEquals(initialEnv.Get(name)));
        }

        void ThenFinds(string name) {
            Assert.DoesNotThrow(() => initialEnv.Find(name));
        }
    }
}
