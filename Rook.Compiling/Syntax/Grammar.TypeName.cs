using System.Linq;
using Parsley;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public sealed partial class Grammar
    {
        public static Parser<NamedType> TypeName
        {
            get
            {
                return OnError(
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
                return Choice(
                    from @int in Keyword("int") select NamedType.Integer,
                    from @bool in Keyword("bool") select NamedType.Boolean,
                    from @void in Keyword("void") select NamedType.Void);
            }
        }

        private static NamedType ApplyTypeModifier(NamedType targetType, Token modifier)
        {
            if (modifier.ToString() == "*")
                return NamedType.Enumerable(targetType);

            if (modifier.ToString() == "[]")
                return NamedType.Vector(targetType);

            return NamedType.Nullable(targetType);
        }
    }
}