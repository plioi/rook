using NUnit.Framework;

namespace Rook.Core
{
    [TestFixture]
    public class ValueSpec
    {
        [Test]
        public void AnInstanceShouldAlwaysReturnTheSameHashCode()
        {
            var o = new Sample(1, "A");
            o.GetHashCode().ShouldEqual(o.GetHashCode());
        }

        [Test]
        public void HashCodesOfEquivalentObjectsShouldBeEqual()
        {
            var a = new Sample(1, "A");
            var b = new Sample(1, "A");
            a.GetHashCode().ShouldEqual(b.GetHashCode());
        }

        [Test]
        public void HashCodesOfNonEquivalentObjectsShouldUsuallyBeDifferent()
        {
            var a = new Sample(0, "A");
            var b = new Sample(1, "A");
            var c = new Sample(0, "B");
            a.GetHashCode().ShouldNotEqual(b.GetHashCode());
            a.GetHashCode().ShouldNotEqual(c.GetHashCode());
            b.GetHashCode().ShouldNotEqual(c.GetHashCode());
        }

        [Test]
        public void HashCodesShouldBeSafeFromNullFields()
        {
            new Sample(0, null).GetHashCode();
        }

        [Test]
        public void HashCodesShouldBeSafeFromNumericOverflow()
        {
            new Sample(int.MaxValue, "A").GetHashCode();
        }

        [Test]
        public void IsEquatableByComparingImmutableFields()
        {
            var nil = (Sample)null;
            new Sample(0, "A").Equals(new Sample(0, "A")).ShouldBeTrue();
            new Sample(0, "A").Equals(nil).ShouldBeFalse();
            new Sample(0, "A").Equals(new Sample(1, "A")).ShouldBeFalse();
            new Sample(0, "A").Equals(new Sample(0, null)).ShouldBeFalse();
            new Sample(0, null).Equals(new Sample(0, "A")).ShouldBeFalse();
        }

        [Test]
        public void OverridesObjectEquals()
        {
            object nil = null;
            new Sample(0, "A").Equals((object)new Sample(0, "A")).ShouldBeTrue();
            new Sample(0, "A").Equals(nil).ShouldBeFalse();
            new Sample(0, "A").Equals((object)new Sample(1, "A")).ShouldBeFalse();
            new Sample(0, "A").Equals((object)new Sample(0, null)).ShouldBeFalse();
            new Sample(0, null).Equals((object)new Sample(0, "A")).ShouldBeFalse();
        }

        [Test]
        public void OverloadsEqualityOperators()
        {
            var a = new Sample(0, "A");
            var a2 = new Sample(0, "A");
            var b = new Sample(0, "B");

            (a == a).ShouldBeTrue();
            (a == b).ShouldBeFalse();
            (a == a2).ShouldBeTrue();
            (null == a).ShouldBeFalse();
            (a == null).ShouldBeFalse();

            (a != a).ShouldBeFalse();
            (a != b).ShouldBeTrue();
            (a != a2).ShouldBeFalse();
            (null != a).ShouldBeTrue();
            (a != null).ShouldBeTrue();
        }

        private class Sample : Value<Sample>
        {
            private readonly int i;
            private readonly string s;

            public Sample(int i, string s)
            {
                this.i = i;
                this.s = s;
            }

            protected override object[] ImmutableFields()
            {
                return new object[] {i, s};
            }
        }
    }
}