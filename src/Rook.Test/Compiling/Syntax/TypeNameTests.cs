using Should;

namespace Rook.Compiling.Syntax
{
    [Facts]
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
    }
}
