using System;
using System.Collections.Generic;
using System.Linq;
using Rook.Compiling.Syntax;
using Rook.Compiling.Types;

namespace Rook.Compiling
{
    public class Scope
    {
        private readonly Func<TypeVariable> CreateTypeVariable;

        private readonly IDictionary<string, DataType> locals;
        private readonly List<TypeVariable> localNonGenericTypeVariables;
        private readonly Scope parent;

        private Scope(Scope parent, Func<TypeVariable> createTypeVariable)
        {
            CreateTypeVariable = createTypeVariable;
            locals = new Dictionary<string, DataType>();
            localNonGenericTypeVariables = new List<TypeVariable>();
            this.parent = parent;
        }

        public static Scope CreateRoot(TypeChecker typeChecker)
        {
            var scope = new Scope(null, typeChecker.CreateTypeVariable);

            DataType @int = NamedType.Integer;
            DataType @bool = NamedType.Boolean;

            DataType integerOperation = NamedType.Function(new[] { @int, @int }, @int);
            DataType integerComparison = NamedType.Function(new[] { @int, @int }, @bool);
            DataType booleanOperation = NamedType.Function(new[] { @bool, @bool }, @bool);

            scope["<"] = integerComparison;
            scope["<="] = integerComparison;
            scope[">"] = integerComparison;
            scope[">="] = integerComparison;
            scope["=="] = integerComparison;
            scope["!="] = integerComparison;

            scope["+"] = integerOperation;
            scope["*"] = integerOperation;
            scope["/"] = integerOperation;
            scope["-"] = integerOperation;

            scope["&&"] = booleanOperation;
            scope["||"] = booleanOperation;
            scope["!"] = NamedType.Function(new[] { @bool }, @bool);

            var T = typeChecker.CreateTypeVariable(); //TypeVariable 0
            var S = typeChecker.CreateTypeVariable(); //TypeVariable 1

            scope["??"] = NamedType.Function(new DataType[] { NamedType.Nullable(T), T }, T);
            scope["Print"] = NamedType.Function(new[] { T }, NamedType.Void);
            scope["Nullable"] = NamedType.Function(new[] { T }, NamedType.Nullable(T));
            scope["First"] = NamedType.Function(new[] { NamedType.Enumerable(T) }, T);
            scope["Take"] = NamedType.Function(new[] { NamedType.Enumerable(T), @int }, NamedType.Enumerable(T));
            scope["Skip"] = NamedType.Function(new[] { NamedType.Enumerable(T), @int }, NamedType.Enumerable(T));
            scope["Any"] = NamedType.Function(new[] { NamedType.Enumerable(T) }, @bool);
            scope["Count"] = NamedType.Function(new[] { NamedType.Enumerable(T) }, @int);
            scope["Select"] = NamedType.Function(new[] { NamedType.Enumerable(T), NamedType.Function(new[] { T }, S) }, NamedType.Enumerable(S));
            scope["Where"] = NamedType.Function(new[] { NamedType.Enumerable(T), NamedType.Function(new[] { T }, @bool) }, NamedType.Enumerable(T));
            scope["Each"] = NamedType.Function(new[] { NamedType.Vector(T) }, NamedType.Enumerable(T));
            scope["Index"] = NamedType.Function(new[] { NamedType.Vector(T), @int }, T);
            scope["Slice"] = NamedType.Function(new[] { NamedType.Vector(T), @int, @int }, NamedType.Vector(T));
            scope["Append"] = NamedType.Function(new DataType[] { NamedType.Vector(T), T }, NamedType.Vector(T));
            scope["With"] = NamedType.Function(new[] { NamedType.Vector(T), @int, T }, NamedType.Vector(T));

            return scope;
        }

        public Scope CreateLocalScope()
        {
            return new Scope(this, CreateTypeVariable);
        }

        public DataType this[string key]
        {
            set { locals[key] = value; }
        }

        public bool TryGetMemberScope(TypeRegistry typeRegistry, NamedType typeKey, out Scope typeMemberScope)
        {
            IEnumerable<Binding> typeMembers;
            if (typeRegistry.TryGetMembers(typeKey, out typeMembers))
            {
                var scope = new Scope(null, CreateTypeVariable);

                foreach (var member in typeMembers)
                    scope.TryIncludeUniqueBinding(member);

                typeMemberScope = scope;
                return true;
            }

            typeMemberScope = null;
            return false;
        }

        public bool TryGet(string key, out DataType value)
        {
            if (locals.ContainsKey(key))
            {
                value = locals[key];
                return true;
            }

            if (parent == null)
            {
                value = null;
                return false;
            }

            return parent.TryGet(key, out value);
        }

        public bool Contains(string key)
        {
            return locals.ContainsKey(key) || (parent != null && parent.Contains(key));
        }

        public bool TryIncludeUniqueBinding(Binding binding)
        {
            if (Contains(binding.Identifier))
                return false;

            this[binding.Identifier] = binding.Type;
            return true;
        }

        public void TreatAsNonGeneric(IEnumerable<TypeVariable> typeVariables)
        {
            localNonGenericTypeVariables.AddRange(typeVariables);
        }

        public bool IsGeneric(TypeVariable typeVariable)
        {
            if (localNonGenericTypeVariables.Contains(typeVariable))
                return false;

            if (parent == null)
                return true;

            return parent.IsGeneric(typeVariable);
        }
    }
}