using System;
using Rook.Compiling;
using Rook.Compiling.Syntax;
using Rook.Compiling.Types;
using Should;

namespace Rook
{
    public static class ScopeExtensions
    {
        public static void Bind(this Scope scope, string identifier, DataType type)
        {
            var result = scope.TryIncludeUniqueBinding(new StubBinding(identifier, type));

            result.ShouldBeTrue(String.Format("Failed to bind identifier '{0}' to type '{1}'", identifier, type));
        }
    }
}