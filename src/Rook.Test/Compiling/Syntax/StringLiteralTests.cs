using Parsley;
using Rook.Compiling.Types;
using Should;

namespace Rook.Compiling.Syntax
{
    [Facts]
    public class StringLiteralTests : ExpressionTests
    {
        public void IsIdentifiedByQuotedContentCharacters()
        {
            Parses("\"\"").IntoTree("\"\"");
            Parses("\"abcdef\"").IntoTree("\"abcdef\"");
        }

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

        public void HasPositionOfOpeningQuotationMark()
        {
            Parses("\"abcdef\"").WithValue(
                syntaxTree => ((StringLiteral) syntaxTree).Position.ShouldEqual(new Position(1, 1)));
        }

        public void HasStringType()
        {
            Type("\"abcdef\"").ShouldEqual(NamedType.String);
        }

        public void AreAlwaysFullyTyped()
        {
            var str = (StringLiteral)Parse("\"abcdef\"");
            str.Type.ShouldEqual(NamedType.String);

            var typedStr = WithTypes(str);
            typedStr.Type.ShouldEqual(NamedType.String);
            typedStr.ShouldBeSameAs(str);
        }
    }
}