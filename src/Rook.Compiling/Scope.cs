using System.Collections.Generic;
using Rook.Compiling.Syntax;
using Rook.Compiling.Types;
using Rook.Core.Collections;

namespace Rook.Compiling
{
    public interface Scope
    {
        bool TryIncludeUniqueBinding(Binding binding);
        bool TryGet(string identifier, out DataType type);
        bool Contains(string identifier);
    }

    public class GlobalScope : Scope
    {
        private readonly BindingDictionary globals;

        public GlobalScope()
        {
            globals = new BindingDictionary();

            DataType @int = NamedType.Integer;
            DataType @bool = NamedType.Boolean;

            DataType integerOperation = NamedType.Function(new[] { @int, @int }, @int);
            DataType integerComparison = NamedType.Function(new[] { @int, @int }, @bool);
            DataType booleanOperation = NamedType.Function(new[] { @bool, @bool }, @bool);

            globals["<"] = integerComparison;
            globals["<="] = integerComparison;
            globals[">"] = integerComparison;
            globals[">="] = integerComparison;
            globals["=="] = integerComparison;
            globals["!="] = integerComparison;

            globals["+"] = integerOperation;
            globals["*"] = integerOperation;
            globals["/"] = integerOperation;
            globals["-"] = integerOperation;

            globals["&&"] = booleanOperation;
            globals["||"] = booleanOperation;
            globals["!"] = NamedType.Function(new[] { @bool }, @bool);

            var T = TypeVariable.CreateGeneric(); //TypeVariable 0
            var S = TypeVariable.CreateGeneric(); //TypeVariable 1

            globals["??"] = NamedType.Function(new DataType[] { NamedType.Nullable(T), T }, T);
            globals["Print"] = NamedType.Function(new[] { T }, NamedType.Void);
            globals["Nullable"] = NamedType.Function(new[] { T }, NamedType.Nullable(T));
            globals["First"] = NamedType.Function(new[] { NamedType.Enumerable(T) }, T);
            globals["Take"] = NamedType.Function(new[] { NamedType.Enumerable(T), @int }, NamedType.Enumerable(T));
            globals["Skip"] = NamedType.Function(new[] { NamedType.Enumerable(T), @int }, NamedType.Enumerable(T));
            globals["Any"] = NamedType.Function(new[] { NamedType.Enumerable(T) }, @bool);
            globals["Count"] = NamedType.Function(new[] { NamedType.Enumerable(T) }, @int);
            globals["Select"] = NamedType.Function(new[] { NamedType.Enumerable(T), NamedType.Function(new[] { T }, S) }, NamedType.Enumerable(S));
            globals["Where"] = NamedType.Function(new[] { NamedType.Enumerable(T), NamedType.Function(new[] { T }, @bool) }, NamedType.Enumerable(T));
            globals["Each"] = NamedType.Function(new[] { NamedType.Vector(T) }, NamedType.Enumerable(T));
            globals["Index"] = NamedType.Function(new[] { NamedType.Vector(T), @int }, T);
            globals["Slice"] = NamedType.Function(new[] { NamedType.Vector(T), @int, @int }, NamedType.Vector(T));
            globals["Append"] = NamedType.Function(new DataType[] { NamedType.Vector(T), T }, NamedType.Vector(T));
            globals["With"] = NamedType.Function(new[] { NamedType.Vector(T), @int, T }, NamedType.Vector(T));
        }

        public bool TryIncludeUniqueBinding(Binding binding)
        {
            return globals.TryIncludeUniqueBinding(binding);
        }

        public bool TryGet(string identifier, out DataType type)
        {
            return globals.TryGet(identifier, out type);
        }

        public bool Contains(string identifier)
        {
            return globals.Contains(identifier);
        }
    }

    public sealed class TypeMemberScope : Scope
    {
        private readonly BindingDictionary members;

        public TypeMemberScope(Vector<Binding> typeMembers)
        {
            members = new BindingDictionary();
            foreach (var member in typeMembers)
                TryIncludeUniqueBinding(member);
        }

        public bool TryIncludeUniqueBinding(Binding binding)
        {
            return members.TryIncludeUniqueBinding(binding);
        }

        public bool TryGet(string identifier, out DataType type)
        {
            return members.TryGet(identifier, out type);
        }

        public bool Contains(string identifier)
        {
            return members.Contains(identifier);
        }
    }

    public class LocalScope : Scope
    {
        protected readonly Scope parent;
        private readonly BindingDictionary locals;

        public LocalScope(Scope parent)
        {
            this.parent = parent;
            locals  = new BindingDictionary();
        }

        public bool TryIncludeUniqueBinding(Binding binding)
        {
            if (Contains(binding.Identifier))
                return false;

            return locals.TryIncludeUniqueBinding(binding);
        }

        public bool TryGet(string identifier, out DataType type)
        {
            return locals.TryGet(identifier, out type) || parent.TryGet(identifier, out type);
        }

        public bool Contains(string identifier)
        {
            return locals.Contains(identifier) || parent.Contains(identifier);
        }
    }
}