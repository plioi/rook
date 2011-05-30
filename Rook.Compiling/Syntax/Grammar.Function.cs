using Parsley;

namespace Rook.Compiling.Syntax
{
    public sealed partial class Grammar
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

                return Choice(explicitlyTyped, implicitlyTyped);
            }
        }
    }
}