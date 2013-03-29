using System.Collections.Generic;
using Rook.Compiling.Syntax;
using Rook.Compiling.Types;
using Rook.Core.Collections;

namespace Rook.Compiling
{
    public abstract class Scope
    {
        public bool TryIncludeUniqueBinding(Binding binding)
        {
            return TryIncludeUniqueBinding(binding.Identifier, binding.Type);
        }

        public abstract bool TryIncludeUniqueBinding(string identifier, DataType type);
        public abstract bool TryGet(string identifier, out DataType type);
        public abstract bool Contains(string identifier);
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

            var enumerableT = NamedType.Enumerable(T);
            var nullableT = NamedType.Nullable(T);
            var vectorT = NamedType.Vector(T);
            var enumerableS = NamedType.Enumerable(S);

            globals["??"] = NamedType.Function(new DataType[] { nullableT, T }, T);
            globals["Print"] = NamedType.Function(new[] { T }, NamedType.Void);
            globals["Nullable"] = NamedType.Function(new[] { T }, nullableT);
            globals["First"] = NamedType.Function(new[] { enumerableT }, T);
            globals["Take"] = NamedType.Function(new[] { enumerableT, @int }, enumerableT);
            globals["Skip"] = NamedType.Function(new[] { enumerableT, @int }, enumerableT);
            globals["Any"] = NamedType.Function(new[] { enumerableT }, @bool);
            globals["Count"] = NamedType.Function(new[] { enumerableT }, @int);
            globals["Select"] = NamedType.Function(new[] { enumerableT, NamedType.Function(new[] { T }, S) }, enumerableS);
            globals["Where"] = NamedType.Function(new[] { enumerableT, NamedType.Function(new[] { T }, @bool) }, enumerableT);
            globals["Each"] = NamedType.Function(new[] { vectorT }, enumerableT);
            globals[ReservedName.__index__] = NamedType.Function(new[] { vectorT, @int }, T);
            globals[ReservedName.__slice__] = NamedType.Function(new[] { vectorT, @int, @int }, vectorT);
            globals["Append"] = NamedType.Function(new DataType[] { vectorT, T }, vectorT);
            globals["With"] = NamedType.Function(new[] { vectorT, @int, T }, vectorT);
        }

        public override bool TryIncludeUniqueBinding(string identifier, DataType type)
        {
            return globals.TryIncludeUniqueBinding(identifier, type);
        }

        public override bool TryGet(string identifier, out DataType type)
        {
            return globals.TryGet(identifier, out type);
        }

        public override bool Contains(string identifier)
        {
            return globals.Contains(identifier);
        }
    }

    public sealed class TypeMemberScope : Scope
    {
        private readonly BindingDictionary members;

        public TypeMemberScope(IEnumerable<Binding> typeMembers)
        {
            members = new BindingDictionary();
            foreach (var member in typeMembers)
                TryIncludeUniqueBinding(member);
        }

        public override bool TryIncludeUniqueBinding(string identifier, DataType type)
        {
            return members.TryIncludeUniqueBinding(identifier, type);
        }

        public override bool TryGet(string identifier, out DataType type)
        {
            return members.TryGet(identifier, out type);
        }

        public override bool Contains(string identifier)
        {
            return members.Contains(identifier);
        }
    }

    public class LocalScope : Scope
    {
        private readonly Scope parent;
        private readonly BindingDictionary locals;

        public LocalScope(Scope parent)
        {
            this.parent = parent;
            locals  = new BindingDictionary();
        }

        public override bool TryIncludeUniqueBinding(string identifier, DataType type)
        {
            if (Contains(identifier))
                return false;

            return locals.TryIncludeUniqueBinding(identifier, type);
        }

        public override bool TryGet(string identifier, out DataType type)
        {
            return locals.TryGet(identifier, out type) || parent.TryGet(identifier, out type);
        }

        public override bool Contains(string identifier)
        {
            return locals.Contains(identifier) || parent.Contains(identifier);
        }
    }
}