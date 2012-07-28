using System;
using System.Collections.Generic;
using System.Linq;
using Rook.Compiling.Syntax;
using Rook.Compiling.Types;
using Rook.Core.Collections;

namespace Rook.Compiling
{
    public class Scope
    {
        private readonly Func<TypeVariable> CreateTypeVariable;

        protected readonly IDictionary<string, DataType> locals;
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

            locals[binding.Identifier] = binding.Type;
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

    public class TypeMemberScope : Scope
    {
        public TypeMemberScope(TypeChecker typeChecker, Vector<Binding> typeMembers)
            : base(null, typeChecker.CreateTypeVariable)
        {
            foreach (var member in typeMembers)
                TryIncludeUniqueBinding(member);
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