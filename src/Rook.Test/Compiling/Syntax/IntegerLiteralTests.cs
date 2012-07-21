using Should;
using Xunit;

namespace Rook.Compiling.Syntax
{
    public class IntegerLiteralTests : ExpressionTests
    {
        [Fact]
        public void IsZeroOrPositiveSequenceOfDigits()
        {
            Parses("0").IntoTree("0");
            Parses("2147483647").IntoTree("2147483647");

            //Overflow values parse, but will be caught by the type checker.
            Parses("2147483648").IntoTree("2147483648");
        }

        [Fact]
        public void HasIntegerType()
        {
            Type("0").ShouldEqual(Integer);
            Type("2147483647").ShouldEqual(Integer);
        }

        [Fact]
        public void CanCreateFullyTypedInstance()
        {
            var integer = (IntegerLiteral)Parse("12345");
            integer.Type.ShouldBeNull();

            var typedInteger = WithTypes(integer);
            typedInteger.Type.ShouldEqual(Integer);
        }

        [Fact]
        public void FailsTypeCheckingWhenOutOfRange()
        {
            ShouldFailTypeChecking("2147483648").WithError("Invalid constant: 2147483648", 1, 1);
        }
    }
}