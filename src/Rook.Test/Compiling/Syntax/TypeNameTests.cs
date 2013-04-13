using Should;

namespace Rook.Compiling.Syntax
{
    public class TypeNameTests
    {
        public void HasAName()
        {
            new TypeName("A").Name.ShouldEqual("A");
            new TypeName("B", new TypeName("A")).Name.ShouldEqual("B");
        }

        public void HasZeroOrMoreGenericArguments()
        {
            new TypeName("A").GenericArguments.ShouldBeEmpty();
            new TypeName("B", new TypeName("A")).GenericArguments.ShouldList(new TypeName("A"));
            new TypeName("C", new TypeName("B", new TypeName("A"))).GenericArguments.ShouldList(new TypeName("B", new TypeName("A")));
        }

        public void HasAStringRepresentation()
        {
            new TypeName("A").ToString().ShouldEqual("A");
            new TypeName("A", new TypeName("B")).ToString().ShouldEqual("A<B>");
            new TypeName("A", new TypeName("B", new TypeName("C"), new TypeName("D"))).ToString().ShouldEqual("A<B<C, D>>");
        }

        public void HasValueEqualitySemantics()
        {
            var type = new TypeName("B", new TypeName("A"));
            type.ShouldEqual(type);
            type.ShouldEqual(new TypeName("B", new TypeName("A")));
            type.ShouldNotEqual(new TypeName("B"));

            type.GetHashCode().ShouldEqual(new TypeName("B", new TypeName("A")).GetHashCode());
            type.GetHashCode().ShouldNotEqual(new TypeName("B").GetHashCode());
        }

        public void HasStaticEmptyValueRepresentingTheAbsenseOfAName()
        {
            TypeName.Empty.Name.ShouldEqual("");
            TypeName.Empty.GenericArguments.ShouldBeEmpty();
            TypeName.Empty.ToString().ShouldEqual("");
            TypeName.Empty.ShouldEqual(TypeName.Empty);
        }

        public void HasStaticHelpersForKeywordTypes()
        {
            TypeName.Integer.ToString().ShouldEqual("System.Int32");
            TypeName.Boolean.ToString().ShouldEqual("System.Boolean");
            TypeName.String.ToString().ShouldEqual("System.String");
            TypeName.Void.ToString().ShouldEqual("Rook.Core.Void");
        }

        public void HasStaticHelperForEnumerableTypes()
        {
            TypeName.Enumerable(TypeName.Integer).ToString().ShouldEqual("System.Collections.Generic.IEnumerable<System.Int32>");
        }

        public void HasStaticHelperForVectorTypes()
        {
            TypeName.Vector(TypeName.Integer).ToString().ShouldEqual("Rook.Core.Collections.Vector<System.Int32>");
        }

        public void HasStaticHelperForNullableTypes()
        {
            TypeName.Nullable(TypeName.Integer).ToString().ShouldEqual("Rook.Core.Nullable<System.Int32>");
        }
    }
}
