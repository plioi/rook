using Parsley;

namespace Rook.Compiling.Syntax
{
    public partial class RookGrammar
    {
        public static Parser<Function> Function
        {
            get
            {
                return from returnType in TypeName
                       from name in Name
                       from parameters in Tuple(Parameter).TerminatedBy(Optional(EndOfLine))
                       from body in Expression
                       select new Function(name.Position, returnType, name, parameters, body);
            }
        }

        private static Parser<Parameter> Parameter
        {
            get
            {
                var explicitlyTyped = from type in TypeName
                                      from identifier in Identifier
                                      select new Parameter(identifier.Position, type, identifier.Literal);

                var implicitlyTyped = from identifier in Identifier
                                      select new Parameter(identifier.Position, identifier.Literal);

                return Choice(Attempt(explicitlyTyped), implicitlyTyped);
            }
        }
    }
}