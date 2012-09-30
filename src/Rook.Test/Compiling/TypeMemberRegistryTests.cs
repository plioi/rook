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
    public class TypeMemberRegistryTests
    {
        private static readonly NamedType Integer = NamedType.Integer;
        private static readonly NamedType Boolean = NamedType.Boolean;

        private readonly TypeMemberRegistry typeMemberRegistry;

        public TypeMemberRegistryTests()
        {
            typeMemberRegistry = new TypeMemberRegistry();
        }

        public void LooksUpMemberBindingsForKnownClassDefinitions()
        {
            var fooClass = "class Foo { int I() 0; bool B() true; }".ParseClass();
            var mathClass = "class Math { int Square(int x) x*x; bool Zero(int x) x==0; }".ParseClass();

            var foo = new NamedType(fooClass);
            var math = new NamedType(mathClass);

            typeMemberRegistry.Register(fooClass);
            typeMemberRegistry.Register(mathClass);

            AssertMemberType(NamedType.Function(Integer), foo, "I");
            AssertMemberType(NamedType.Function(Boolean), foo, "B");
            AssertMemberType(NamedType.Function(new[] { Integer }, Integer), math, "Square");
            AssertMemberType(NamedType.Function(new[] { Integer }, Boolean), math, "Zero");
        }

        public void FailsToLookUpMemberBindingsForUnknownTypes()
        {
            var unregisteredAndUndiscoverable = "class UnknownType { }".ParseClass();

            Vector<Binding> expectedFailure;
            typeMemberRegistry.TryGetMembers(new NamedType(unregisteredAndUndiscoverable), out expectedFailure).ShouldBeFalse();
            expectedFailure.ShouldBeNull();
        }

        private void AssertMemberType(DataType expectedType, NamedType typeKey, string memberKey)
        {
            Vector<Binding> memberBindings;

            if (typeMemberRegistry.TryGetMembers(typeKey, out memberBindings))
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