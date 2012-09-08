using System;
using System.Collections.Generic;
using System.Linq;
using Parsley;
using Rook.Compiling.Syntax;
using Rook.Compiling.Types;
using Rook.Core.Collections;
using Should;

namespace Rook.Compiling
{
    [Facts]
    public class TypeRegistryTests
    {
        private static readonly NamedType Integer = NamedType.Integer;
        private static readonly NamedType Boolean = NamedType.Boolean;

        private readonly TypeRegistry typeRegistry;

        public TypeRegistryTests()
        {
            typeRegistry = new TypeRegistry();
        }

        public void LooksUpMemberBindingsForKnownClassDefinitions()
        {
            var foo = new NamedType("Foo");
            var math = new NamedType("Math");

            var fooBinding = ParseClass("class Foo { int I() 0; bool B() true; }");
            var mathBinding = ParseClass("class Math { int Square(int x) x*x; bool Zero(int x) x==0; }");

            typeRegistry.Register(fooBinding);
            typeRegistry.Register(mathBinding);

            AssertMemberType(NamedType.Function(Integer), foo, "I");
            AssertMemberType(NamedType.Function(Boolean), foo, "B");
            AssertMemberType(NamedType.Function(new[] { Integer }, Integer), math, "Square");
            AssertMemberType(NamedType.Function(new[] { Integer }, Boolean), math, "Zero");
        }

        public void FailsToLookUpMemberBindingsForUnknownTypes()
        {
            Vector<Binding> expectedFailure;
            typeRegistry.TryGetMembers(new NamedType("UnknownType"), out expectedFailure).ShouldBeFalse();
            expectedFailure.ShouldBeNull();
        }

        private static Class ParseClass(string classDeclaration)
        {
            var tokens = new RookLexer().Tokenize(classDeclaration);
            var parser = new RookGrammar().Class;
            return parser.Parse(new TokenStream(tokens)).Value;
        }

        private void AssertMemberType(DataType expectedType, NamedType typeKey, string memberKey)
        {
            Vector<Binding> memberBindings;

            if (typeRegistry.TryGetMembers(typeKey, out memberBindings))
                AssertMemberType(expectedType, memberBindings, memberKey);
            else
                throw new Exception("Failed to look up the type of '" + typeKey + "+" + memberKey + "' in the Scope");
        }

        private static void AssertMemberType(DataType expectedType, IEnumerable<Binding> memberBindings, string key)
        {
            var binding = memberBindings.SingleOrDefault(x => x.Identifier == key);

            if (binding != null)
                binding.Type.ShouldEqual(expectedType);
            else
                throw new Exception("Failed to look up the type of '" + key + "' in the Scope");
        }
    }
}