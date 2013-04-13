using System;
using Should;

namespace Rook.Core
{
    public class NullableTests
    {
        public void CanWrapValueTypes()
        {
            var zero = new Nullable<int>(0);
            zero.Value.ShouldEqual(0);
        }

        public void CanWrapReferenceTypes()
        {
            var o = new object();
            var nullable = new Nullable<object>(o);
            nullable.Value.ShouldBeSameAs(o);
        }

        public void CannotWrapNullReferences()
        {
            Action wrapNull = () => new Nullable<object>(null);
            wrapNull.ShouldThrow<ArgumentNullException>("Value cannot be null.\r\nParameter name: value");
        }
    }
}
