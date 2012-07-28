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
        protected readonly Scope parent;

        protected Scope(Scope parent, Func<TypeVariable> createTypeVariable)
        {
            CreateTypeVariable = createTypeVariable;
            locals = new Dictionary<string, DataType>();
            this.parent = parent;
        }

        public Scope CreateLocalScope()
        {
            return new Scope(this, CreateTypeVariable);
        }

        public LambdaScope CreateLambdaScope()
        {
            return new LambdaScope(this, CreateTypeVariable);
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
                value = FreshenGenericTypeVariables(locals[key]);
                return true;
            }

            if (parent == null)
            {
                value = null;
                return false;
            }

            return parent.TryGet(key, out value);
        }

        private DataType FreshenGenericTypeVariables(DataType type)
        {
            var substitutions = new Dictionary<TypeVariable, DataType>();
            var genericTypeVariables = type.FindTypeVariables().Where(IsGeneric);
            foreach (var genericTypeVariable in genericTypeVariables)
                substitutions[genericTypeVariable] = CreateTypeVariable();

            return type.ReplaceTypeVariables(substitutions);
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

        public virtual bool IsGeneric(TypeVariable typeVariable)
        {
            return parent == null || parent.IsGeneric(typeVariable);
        }
    }

    public class GlobalScope : Scope
    {
        public GlobalScope(TypeChecker typeChecker)
            : base(null, typeChecker.CreateTypeVariable)
        {
            DataType @int = NamedType.Integer;
            DataType @bool = NamedType.Boolean;

            DataType integerOperation = NamedType.Function(new[] { @int, @int }, @int);
            DataType integerComparison = NamedType.Function(new[] { @int, @int }, @bool);
            DataType booleanOperation = NamedType.Function(new[] { @bool, @bool }, @bool);

            this["<"] = integerComparison;
            this["<="] = integerComparison;
            this[">"] = integerComparison;
            this[">="] = integerComparison;
            this["=="] = integerComparison;
            this["!="] = integerComparison;

            this["+"] = integerOperation;
            this["*"] = integerOperation;
            this["/"] = integerOperation;
            this["-"] = integerOperation;

            this["&&"] = booleanOperation;
            this["||"] = booleanOperation;
            this["!"] = NamedType.Function(new[] { @bool }, @bool);

            var T = typeChecker.CreateTypeVariable(); //TypeVariable 0
            var S = typeChecker.CreateTypeVariable(); //TypeVariable 1

            this["??"] = NamedType.Function(new DataType[] { NamedType.Nullable(T), T }, T);
            this["Print"] = NamedType.Function(new[] { T }, NamedType.Void);
            this["Nullable"] = NamedType.Function(new[] { T }, NamedType.Nullable(T));
            this["First"] = NamedType.Function(new[] { NamedType.Enumerable(T) }, T);
            this["Take"] = NamedType.Function(new[] { NamedType.Enumerable(T), @int }, NamedType.Enumerable(T));
            this["Skip"] = NamedType.Function(new[] { NamedType.Enumerable(T), @int }, NamedType.Enumerable(T));
            this["Any"] = NamedType.Function(new[] { NamedType.Enumerable(T) }, @bool);
            this["Count"] = NamedType.Function(new[] { NamedType.Enumerable(T) }, @int);
            this["Select"] = NamedType.Function(new[] { NamedType.Enumerable(T), NamedType.Function(new[] { T }, S) }, NamedType.Enumerable(S));
            this["Where"] = NamedType.Function(new[] { NamedType.Enumerable(T), NamedType.Function(new[] { T }, @bool) }, NamedType.Enumerable(T));
            this["Each"] = NamedType.Function(new[] { NamedType.Vector(T) }, NamedType.Enumerable(T));
            this["Index"] = NamedType.Function(new[] { NamedType.Vector(T), @int }, T);
            this["Slice"] = NamedType.Function(new[] { NamedType.Vector(T), @int, @int }, NamedType.Vector(T));
            this["Append"] = NamedType.Function(new DataType[] { NamedType.Vector(T), T }, NamedType.Vector(T));
            this["With"] = NamedType.Function(new[] { NamedType.Vector(T), @int, T }, NamedType.Vector(T));
        }
    }

    public class LambdaScope : Scope
    {
        private readonly List<TypeVariable> localNonGenericTypeVariables;

        public LambdaScope(Scope parent, Func<TypeVariable> createTypeVariable)
            : base(parent, createTypeVariable)
        {
            localNonGenericTypeVariables = new List<TypeVariable>();
        }

        public void TreatAsNonGeneric(IEnumerable<TypeVariable> typeVariables)
        {
            localNonGenericTypeVariables.AddRange(typeVariables);
        }

        public override bool IsGeneric(TypeVariable typeVariable)
        {
            if (localNonGenericTypeVariables.Contains(typeVariable))
                return false;

            return parent.IsGeneric(typeVariable);
        }
    }
}