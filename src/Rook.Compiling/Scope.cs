using System.Collections.Generic;
using Rook.Compiling.Syntax;
using Rook.Compiling.Types;
using Rook.Core.Collections;

namespace Rook.Compiling
{
    public abstract class Scope
    {
        protected readonly IDictionary<string, DataType> locals;

        protected Scope()
        {
            locals = new Dictionary<string, DataType>();
        }

        public bool TryIncludeUniqueBinding(Binding binding)
        {
            if (Contains(binding.Identifier))
                return false;

            locals[binding.Identifier] = binding.Type;
            return true;
        }

        public virtual bool TryGet(string key, out DataType value)
        {
            if (locals.ContainsKey(key))
            {
                value = locals[key];
                return true;
            }

            value = null;
            return false;
        }

        public virtual bool Contains(string key)
        {
            return locals.ContainsKey(key);
        }

        public virtual bool IsGeneric(TypeVariable typeVariable)
        {
            return true;
        }
    }

    public class GlobalScope : Scope
    {
        public GlobalScope(TypeChecker typeChecker)
        {
            DataType @int = NamedType.Integer;
            DataType @bool = NamedType.Boolean;

            DataType integerOperation = NamedType.Function(new[] { @int, @int }, @int);
            DataType integerComparison = NamedType.Function(new[] { @int, @int }, @bool);
            DataType booleanOperation = NamedType.Function(new[] { @bool, @bool }, @bool);

            locals["<"] = integerComparison;
            locals["<="] = integerComparison;
            locals[">"] = integerComparison;
            locals[">="] = integerComparison;
            locals["=="] = integerComparison;
            locals["!="] = integerComparison;

            locals["+"] = integerOperation;
            locals["*"] = integerOperation;
            locals["/"] = integerOperation;
            locals["-"] = integerOperation;

            locals["&&"] = booleanOperation;
            locals["||"] = booleanOperation;
            locals["!"] = NamedType.Function(new[] { @bool }, @bool);

            var T = typeChecker.CreateTypeVariable(); //TypeVariable 0
            var S = typeChecker.CreateTypeVariable(); //TypeVariable 1

            locals["??"] = NamedType.Function(new DataType[] { NamedType.Nullable(T), T }, T);
            locals["Print"] = NamedType.Function(new[] { T }, NamedType.Void);
            locals["Nullable"] = NamedType.Function(new[] { T }, NamedType.Nullable(T));
            locals["First"] = NamedType.Function(new[] { NamedType.Enumerable(T) }, T);
            locals["Take"] = NamedType.Function(new[] { NamedType.Enumerable(T), @int }, NamedType.Enumerable(T));
            locals["Skip"] = NamedType.Function(new[] { NamedType.Enumerable(T), @int }, NamedType.Enumerable(T));
            locals["Any"] = NamedType.Function(new[] { NamedType.Enumerable(T) }, @bool);
            locals["Count"] = NamedType.Function(new[] { NamedType.Enumerable(T) }, @int);
            locals["Select"] = NamedType.Function(new[] { NamedType.Enumerable(T), NamedType.Function(new[] { T }, S) }, NamedType.Enumerable(S));
            locals["Where"] = NamedType.Function(new[] { NamedType.Enumerable(T), NamedType.Function(new[] { T }, @bool) }, NamedType.Enumerable(T));
            locals["Each"] = NamedType.Function(new[] { NamedType.Vector(T) }, NamedType.Enumerable(T));
            locals["Index"] = NamedType.Function(new[] { NamedType.Vector(T), @int }, T);
            locals["Slice"] = NamedType.Function(new[] { NamedType.Vector(T), @int, @int }, NamedType.Vector(T));
            locals["Append"] = NamedType.Function(new DataType[] { NamedType.Vector(T), T }, NamedType.Vector(T));
            locals["With"] = NamedType.Function(new[] { NamedType.Vector(T), @int, T }, NamedType.Vector(T));
        }
    }

    public sealed class TypeMemberScope : Scope
    {
        public TypeMemberScope(Vector<Binding> typeMembers)
        {
            foreach (var member in typeMembers)
                TryIncludeUniqueBinding(member);
        }
    }

    public class LocalScope : Scope
    {
        protected readonly Scope parent;

        public LocalScope(Scope parent)
        {
            this.parent = parent;
        }

        public override bool TryGet(string key, out DataType value)
        {
            return base.TryGet(key, out value) || parent.TryGet(key, out value);
        }

        public override bool Contains(string key)
        {
            return locals.ContainsKey(key) || parent.Contains(key);
        }

        public override bool IsGeneric(TypeVariable typeVariable)
        {
            return parent.IsGeneric(typeVariable);
        }
    }

    public class LambdaScope : LocalScope
    {
        private readonly List<TypeVariable> localNonGenericTypeVariables;

        public LambdaScope(Scope parent)
            : base(parent)
        {
            localNonGenericTypeVariables = new List<TypeVariable>();
        }

        public void TreatAsNonGeneric(IEnumerable<TypeVariable> typeVariables)
        {
            localNonGenericTypeVariables.AddRange(typeVariables);
        }

        public override bool IsGeneric(TypeVariable typeVariable)
        {
            return !localNonGenericTypeVariables.Contains(typeVariable) && parent.IsGeneric(typeVariable);
        }
    }
}