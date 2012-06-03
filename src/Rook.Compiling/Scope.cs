using System;
using System.Collections.Generic;
using Rook.Compiling.Syntax;
using Rook.Compiling.Types;

namespace Rook.Compiling
{
    public class Scope
    {
        public readonly Func<TypeVariable> CreateTypeVariable;

        private readonly IDictionary<string, DataType> locals;
        private readonly IDictionary<DataType, Scope> typeMemberScopes;
        private readonly List<TypeVariable> localNonGenericTypeVariables;
        private readonly Scope parent;
        private readonly TypeNormalizer typeNormalizer;

        public Scope(Scope parent)
        {
            locals = new Dictionary<string, DataType>();
            localNonGenericTypeVariables = new List<TypeVariable>();
            this.parent = parent;

            if (parent == null)
            {
                //Conceal the type variable counter inside a closure stored
                //in the root, so that nonroot instances don't need to keep
                //around a meaningless int field.

                int next = 0;
                CreateTypeVariable = () => new TypeVariable(next++);
            }
            else
            {
                CreateTypeVariable = parent.CreateTypeVariable;
            }

            typeNormalizer = parent == null ? new TypeNormalizer() : parent.typeNormalizer;
            typeMemberScopes = parent == null ? new Dictionary<DataType, Scope>() : parent.typeMemberScopes;
        }

        public Scope()
            : this((Scope)null)
        {
        }

        public Scope(IEnumerable<TypeMemberBinding> typeMemberBindings)
            : this((Scope)null)
        {
            foreach (var typeMemberBinding in typeMemberBindings)
            {
                var typeKey = typeMemberBinding.Type;

                if (!typeMemberScopes.ContainsKey(typeKey))
                    typeMemberScopes[typeKey] = new Scope();

                var typeMemberScope = typeMemberScopes[typeKey];

                foreach (var member in typeMemberBinding.Members)
                    typeMemberScope.TryIncludeUniqueBinding(member);
            }
        }

        public static Scope CreateScopeWithBuiltins(Scope parent)
        {
            //TODO: If given Scope is not a root, throw!

            var scope = new Scope(parent);

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

            TypeVariable x;
            TypeVariable y;

            x = scope.CreateTypeVariable();
            scope["??"] = NamedType.Function(new DataType[] { NamedType.Nullable(x), x }, x);

            x = scope.CreateTypeVariable();
            scope["Print"] = NamedType.Function(new[] { x }, NamedType.Void);

            x = scope.CreateTypeVariable();
            scope["Nullable"] = NamedType.Function(new[] { x }, NamedType.Nullable(x));

            x = scope.CreateTypeVariable();
            scope["First"] = NamedType.Function(new[] { NamedType.Enumerable(x) }, x);

            x = scope.CreateTypeVariable();
            scope["Take"] = NamedType.Function(new[] { NamedType.Enumerable(x), @int }, NamedType.Enumerable(x));

            x = scope.CreateTypeVariable();
            scope["Skip"] = NamedType.Function(new[] { NamedType.Enumerable(x), @int }, NamedType.Enumerable(x));

            x = scope.CreateTypeVariable();
            scope["Any"] = NamedType.Function(new[] { NamedType.Enumerable(x) }, @bool);

            x = scope.CreateTypeVariable();
            scope["Count"] = NamedType.Function(new[] { NamedType.Enumerable(x) }, @int);

            x = scope.CreateTypeVariable();
            y = scope.CreateTypeVariable();
            scope["Select"] = NamedType.Function(new[] { NamedType.Enumerable(x), NamedType.Function(new[] { x }, y) }, NamedType.Enumerable(y));

            x = scope.CreateTypeVariable();
            scope["Where"] = NamedType.Function(new[] { NamedType.Enumerable(x), NamedType.Function(new[] { x }, @bool) }, NamedType.Enumerable(x));

            x = scope.CreateTypeVariable();
            scope["Each"] = NamedType.Function(new[] { NamedType.Vector(x) }, NamedType.Enumerable(x));

            x = scope.CreateTypeVariable();
            scope["Index"] = NamedType.Function(new[] { NamedType.Vector(x), @int }, x);

            x = scope.CreateTypeVariable();
            scope["Slice"] = NamedType.Function(new[] { NamedType.Vector(x), @int, @int }, NamedType.Vector(x));

            x = scope.CreateTypeVariable();
            scope["Append"] = NamedType.Function(new DataType[] { NamedType.Vector(x), x }, NamedType.Vector(x));

            x = scope.CreateTypeVariable();
            scope["With"] = NamedType.Function(new[] { NamedType.Vector(x), @int, x }, NamedType.Vector(x));

            return scope;
        }

        public DataType this[string key]
        {
            set { locals[key] = value; }
        }

        public TypeNormalizer TypeNormalizer { get { return typeNormalizer; } }

        public bool TryGetMemberScope(DataType typeKey, out Scope typeMemberScope)
        {
            if (typeMemberScopes.ContainsKey(typeKey))
            {
                typeMemberScope = typeMemberScopes[typeKey];
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