using Parsley;
using Rook.Compiling.Types;
using Should;
using Xunit;

namespace Rook.Compiling.Syntax
{
    public class StringLiteralTests : ExpressionTests
    {
        [Fact]
        public void IsIdentifiedByQuotedContentCharacters()
        {
            Parses("\"\"").IntoTree("\"\"");
            Parses("\"abcdef\"").IntoTree("\"abcdef\"");
        }

        [Fact]
        public void ExposesBothTheQuotedLiteralAndTheAssociatedRawValue()
        {
            Parses("\"\"").WithValue(syntaxTree =>
            {
                var str = (StringLiteral)syntaxTree;
                str.QuotedLiteral.ShouldEqual("\"\"");
                str.Value.ShouldEqual("");
            });

            const string literal = "\"abc \\\" \\\\ \\n \\r \\t \\u263a def\"";
            Parses(literal).WithValue(syntaxTree =>
            {
                var str = (StringLiteral) syntaxTree;
                str.QuotedLiteral.ShouldEqual(literal);
                str.Value.ShouldEqual("abc \" \\ \n \r \t ☺ def");
            });
        }

        [Fact]
        public void HasPositionOfOpeningQuotationMark()
        {
            Parses("\"abcdef\"").WithValue(
                syntaxTree => ((StringLiteral) syntaxTree).Position.ShouldEqual(new Position(1, 1)));
        }

        [Fact]
        public void HasStringType()
        {
            Type("\"abcdef\"").ShouldEqual(NamedType.String);
        }

        [Fact]
        public void AreAlwaysFullyTyped()
        {
            var str = (StringLiteral)Parse("\"abcdef\"");
            str.Type.ShouldEqual(NamedType.String);

            var typedStr = (StringLiteral)str.WithTypes(Scope(), new TypeUnifier()).Syntax;
            typedStr.Type.ShouldEqual(NamedType.String);
            typedStr.ShouldBeSameAs(str);
        }
    }
}