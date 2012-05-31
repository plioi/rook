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
        private readonly IDictionary<DataType, Environment> typeMemberEnvironments;
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
            typeMemberEnvironments = parent == null ? new Dictionary<DataType, Environment>() : parent.typeMemberEnvironments;
        }

        public Environment()
            : this((Environment)null)
        {
        }

        public Environment(IEnumerable<TypeMemberBinding> typeMemberBindings)
            : this((Environment)null)
        {
            foreach (var typeMemberBinding in typeMemberBindings)
            {
                var typeKey = typeMemberBinding.Type;

                if (!typeMemberEnvironments.ContainsKey(typeKey))
                    typeMemberEnvironments[typeKey] = new Environment();

                var typeMemberEnvironment = typeMemberEnvironments[typeKey];

                foreach (var member in typeMemberBinding.Members)
                    typeMemberEnvironment.TryIncludeUniqueBinding(member);
            }
        }

        public static Environment CreateEnvironmentWithBuiltins(Environment parent)
        {
            //TODO: If given environment is not a root, throw!

            var environment = new Environment(parent);

            DataType @int = NamedType.Integer;
            DataType @bool = NamedType.Boolean;

            DataType integerOperation = NamedType.Function(new[] { @int, @int }, @int);
            DataType integerComparison = NamedType.Function(new[] { @int, @int }, @bool);
            DataType booleanOperation = NamedType.Function(new[] { @bool, @bool }, @bool);

            environment["<"] = integerComparison;
            environment["<="] = integerComparison;
            environment[">"] = integerComparison;
            environment[">="] = integerComparison;
            environment["=="] = integerComparison;
            environment["!="] = integerComparison;

            environment["+"] = integerOperation;
            environment["*"] = integerOperation;
            environment["/"] = integerOperation;
            environment["-"] = integerOperation;

            environment["&&"] = booleanOperation;
            environment["||"] = booleanOperation;
            environment["!"] = NamedType.Function(new[] { @bool }, @bool);

            TypeVariable x;
            TypeVariable y;

            x = environment.CreateTypeVariable();
            environment["??"] = NamedType.Function(new DataType[] { NamedType.Nullable(x), x }, x);

            x = environment.CreateTypeVariable();
            environment["Print"] = NamedType.Function(new[] { x }, NamedType.Void);

            x = environment.CreateTypeVariable();
            environment["Nullable"] = NamedType.Function(new[] { x }, NamedType.Nullable(x));

            x = environment.CreateTypeVariable();
            environment["First"] = NamedType.Function(new[] { NamedType.Enumerable(x) }, x);

            x = environment.CreateTypeVariable();
            environment["Take"] = NamedType.Function(new[] { NamedType.Enumerable(x), @int }, NamedType.Enumerable(x));

            x = environment.CreateTypeVariable();
            environment["Skip"] = NamedType.Function(new[] { NamedType.Enumerable(x), @int }, NamedType.Enumerable(x));

            x = environment.CreateTypeVariable();
            environment["Any"] = NamedType.Function(new[] { NamedType.Enumerable(x) }, @bool);

            x = environment.CreateTypeVariable();
            environment["Count"] = NamedType.Function(new[] { NamedType.Enumerable(x) }, @int);

            x = environment.CreateTypeVariable();
            y = environment.CreateTypeVariable();
            environment["Select"] = NamedType.Function(new[] { NamedType.Enumerable(x), NamedType.Function(new[] { x }, y) }, NamedType.Enumerable(y));

            x = environment.CreateTypeVariable();
            environment["Where"] = NamedType.Function(new[] { NamedType.Enumerable(x), NamedType.Function(new[] { x }, @bool) }, NamedType.Enumerable(x));

            x = environment.CreateTypeVariable();
            environment["Yield"] = NamedType.Function(new DataType[] { x, NamedType.Enumerable(x) }, NamedType.Enumerable(x));

            x = environment.CreateTypeVariable();
            environment["Each"] = NamedType.Function(new[] { NamedType.Vector(x) }, NamedType.Enumerable(x));

            x = environment.CreateTypeVariable();
            environment["Index"] = NamedType.Function(new[] { NamedType.Vector(x), @int }, x);

            x = environment.CreateTypeVariable();
            environment["Slice"] = NamedType.Function(new[] { NamedType.Vector(x), @int, @int }, NamedType.Vector(x));

            x = environment.CreateTypeVariable();
            environment["Append"] = NamedType.Function(new DataType[] { NamedType.Vector(x), x }, NamedType.Vector(x));

            x = environment.CreateTypeVariable();
            environment["With"] = NamedType.Function(new[] { NamedType.Vector(x), @int, x }, NamedType.Vector(x));

            return environment;
        }

        public DataType this[string key]
        {
            set { locals[key] = value; }
        }

        public TypeNormalizer TypeNormalizer { get { return typeNormalizer; } }

        public bool TryGetMember(DataType typeKey, string memberKey, out DataType value)
        {
            if (typeMemberEnvironments.ContainsKey(typeKey))
            {
                var typeMemberEnvironment = typeMemberEnvironments[typeKey];
                return typeMemberEnvironment.TryGet(memberKey, out value);
            }

            value = null;
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