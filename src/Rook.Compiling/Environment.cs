using System;
using System.Collections.Generic;
using Rook.Compiling.Syntax;
using Rook.Compiling.Types;

namespace Rook.Compiling
{
    public class Environment
    {
        public readonly Func<TypeVariable> CreateTypeVariable;

        private readonly IDictionary<string, DataType> locals;
        private readonly List<TypeVariable> localNonGenericTypeVariables;
        private readonly Environment parent;
        private readonly TypeNormalizer typeNormalizer;

        public Environment(Environment parent)
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
        }

        public Environment()
            : this(null)
        {
            DataType @int = NamedType.Integer;
            DataType @bool = NamedType.Boolean;

            DataType integerOperation = NamedType.Function(new[] {@int, @int}, @int);
            DataType integerComparison = NamedType.Function(new[] {@int, @int}, @bool);
            DataType booleanOperation = NamedType.Function(new[] {@bool, @bool}, @bool);

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
            this["!"] = NamedType.Function(new[] {@bool}, @bool);

            TypeVariable x;
            TypeVariable y;

            x = CreateTypeVariable();
            this["??"] = NamedType.Function(new DataType[] {NamedType.Nullable(x), x}, x);

            x = CreateTypeVariable();
            this["Print"] = NamedType.Function(new[] {x}, NamedType.Void);

            x = CreateTypeVariable();
            this["Nullable"] = NamedType.Function(new[] {x}, NamedType.Nullable(x));

            x = CreateTypeVariable();
            this["First"] = NamedType.Function(new[] {NamedType.Enumerable(x)}, x);

            x = CreateTypeVariable();
            this["Take"] = NamedType.Function(new[] {NamedType.Enumerable(x), @int}, NamedType.Enumerable(x));

            x = CreateTypeVariable();
            this["Skip"] = NamedType.Function(new[] {NamedType.Enumerable(x), @int}, NamedType.Enumerable(x));

            x = CreateTypeVariable();
            this["Any"] = NamedType.Function(new[] {NamedType.Enumerable(x)}, @bool);

            x = CreateTypeVariable();
            this["Count"] = NamedType.Function(new[] {NamedType.Enumerable(x)}, @int);

            x = CreateTypeVariable();
            y = CreateTypeVariable();
            this["Select"] = NamedType.Function(new[] {NamedType.Enumerable(x), NamedType.Function(new[] {x}, y)}, NamedType.Enumerable(y));

            x = CreateTypeVariable();
            this["Where"] = NamedType.Function(new[] {NamedType.Enumerable(x), NamedType.Function(new[] {x}, @bool)}, NamedType.Enumerable(x));

            x = CreateTypeVariable();
            this["Yield"] = NamedType.Function(new DataType[] {x, NamedType.Enumerable(x)}, NamedType.Enumerable(x));

            x = CreateTypeVariable();
            this["Each"] = NamedType.Function(new[] {NamedType.Vector(x)}, NamedType.Enumerable(x));

            x = CreateTypeVariable();
            this["Index"] = NamedType.Function(new[] { NamedType.Vector(x), @int }, x);

            x = CreateTypeVariable();
            this["Slice"] = NamedType.Function(new[] {NamedType.Vector(x), @int, @int}, NamedType.Vector(x));

            x = CreateTypeVariable();
            this["Append"] = NamedType.Function(new DataType[] {NamedType.Vector(x), x}, NamedType.Vector(x));

            x = CreateTypeVariable();
            this["With"] = NamedType.Function(new[] {NamedType.Vector(x), @int, x}, NamedType.Vector(x));
        }

        public DataType this[string key]
        {
            set { locals[key] = value; }
        }

        public TypeNormalizer TypeNormalizer { get { return typeNormalizer; } }

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