using System.Collections.Generic;
using System.Linq;
using Rook.Compiling.Syntax;
using Rook.Compiling.Types;
using Rook.Core;
using Rook.Core.Collections;

namespace Rook.Compiling
{
    public class TypeRegistry
    {
        private readonly IDictionary<TypeName, NamedType> types;

        public TypeRegistry()
        {
            types = new Dictionary<TypeName, NamedType>();

            RegisterCommonTypes();
        }

        private void RegisterCommonTypes()
        {
            types.Add(TypeName.Integer, NamedType.Integer);
            types.Add(TypeName.Boolean, NamedType.Boolean);
            types.Add(TypeName.String, NamedType.String);
            types.Add(TypeName.Void, NamedType.Void);
        }

        public void Add(Class @class)
        {
            var typeName = new TypeName(@class.Name.Identifier);

            types[typeName] = new NamedType(@class, this);
        }

        public NamedType TypeOf(TypeName name)
        {
            if (!types.ContainsKey(name))
            {
                if (name.Name == typeof(IEnumerable<>).QualifiedName())
                {
                    var itemType = TypeOf(name.GenericArguments.Single());
                    if (itemType == null)
                        return null;
                    types.Add(name, NamedType.Enumerable(itemType));
                }
                else if (name.Name == typeof(Vector<>).QualifiedName())
                {
                    var itemType = TypeOf(name.GenericArguments.Single());
                    if (itemType == null)
                        return null;
                    types.Add(name, NamedType.Vector(itemType));
                }
                else if (name.Name == typeof(Nullable<>).QualifiedName())
                {
                    var itemType = TypeOf(name.GenericArguments.Single());
                    if (itemType == null)
                        return null;
                    types.Add(name, NamedType.Nullable(itemType));
                }
                else
                {
                    return null;
                }
            }

            return types[name];
        }

        public NamedType DeclaredType(Function function)
        {
            var parameterTypes = function.Parameters.Select(p => TypeOf(p.DeclaredTypeName)).ToArray();

            return NamedType.Function(parameterTypes, TypeOf(function.ReturnTypeName));
        }
    }
}