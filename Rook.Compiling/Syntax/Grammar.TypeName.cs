using System.Linq;
using Parsley;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public partial class Grammar
    {
        public static Parser<NamedType> TypeName
        {
            get
            {
                return Label(
                    from rootType in Choice(NameType, KeywordType)
                    from modifiers in ZeroOrMore(TypeModifier)
                    select modifiers.Aggregate(rootType, ApplyTypeModifier),
                    "type name");
            }
        }

        private static Parser<NamedType> NameType
        {
            get
            {
                return from name in Name
                       select new NamedType(name.Identifier);
            }
        }

        private static Parser<Token> TypeModifier
        {
            get { return Choice(Operator("*"), Operator("?"), Operator("[]")); }
        }

        private static Parser<NamedType> KeywordType
        {
            get
            {
                return Choice(
                    from _ in Token(RookLexer.@int) select NamedType.Integer,
                    from _ in Token(RookLexer.@bool) select NamedType.Boolean,
                    from _ in Token(RookLexer.@void) select NamedType.Void);
            }
        }

        private static NamedType ApplyTypeModifier(NamedType targetType, Token modifier)
        {
            if (modifier.Literal == "*")
                return NamedType.Enumerable(targetType);

            if (modifier.Literal == "[]")
                return NamedType.Vector(targetType);

            return NamedType.Nullable(targetType);
        }
    }
}