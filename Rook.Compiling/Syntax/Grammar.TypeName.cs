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
                return OnError(
                    from rootType in GreedyChoice(NameType, KeywordType)
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
                       select NamedType.Create(name.Identifier);
            }
        }

        private static Parser<Token> TypeModifier
        {
            get { return Operator("*", "?", "[]"); }
        }

        private static Parser<NamedType> KeywordType
        {
            get
            {
                return GreedyChoice(
                    from _ in @int select NamedType.Integer,
                    from _ in @bool select NamedType.Boolean,
                    from _ in @void select NamedType.Void);
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