using System;
using System.Collections.Generic;
using System.Linq;
using Rook.Compiling.Types;
using Rook.Core.Collections;

namespace Rook.Compiling.Syntax
{
    public class TypeChecker
    {
        public readonly Func<TypeVariable> CreateTypeVariable;
        private readonly TypeUnifier unifier;
        private readonly TypeRegistry typeRegistry;

        public TypeChecker()
        {
            unifier = new TypeUnifier();
            typeRegistry = new TypeRegistry();

            int next = 0;
            CreateTypeVariable = () => new TypeVariable(next++);
        }

        //TODO: This property is deprecated.  Once TypeRegistry can discover .NET types via reflection,
        //rephrase unit test usages of this so they don't have to manually prepare the TypeRegistry.
        public TypeRegistry TypeRegistry { get { return typeRegistry; } }

        public TypeChecked<CompilationUnit> TypeCheck(CompilationUnit compilationUnit)
        {
            var Position = compilationUnit.Position;
            var Classes = compilationUnit.Classes;
            var Functions = compilationUnit.Functions;

            foreach (var @class in Classes)//TODO: Test coverage.
                typeRegistry.Register(@class);

            var scope = Scope.CreateRoot(this);

            foreach (var @class in Classes)
                if (!scope.TryIncludeUniqueBinding(@class))
                    return TypeChecked<CompilationUnit>.DuplicateIdentifierError(@class);

            foreach (var function in Functions)
                if (!scope.TryIncludeUniqueBinding(function))
                    return TypeChecked<CompilationUnit>.DuplicateIdentifierError(function);

            var typeCheckedClasses = TypeCheck(Classes, scope);
            var typeCheckedFunctions = TypeCheck(Functions, scope);

            var classErrors = typeCheckedClasses.Errors();
            var functionErrors = typeCheckedFunctions.Errors();

            if (classErrors.Any() || functionErrors.Any())
                return TypeChecked<CompilationUnit>.Failure(classErrors.Concat(functionErrors).ToVector());

            return TypeChecked<CompilationUnit>.Success(new CompilationUnit(Position, typeCheckedClasses.Classes(), typeCheckedFunctions.Functions()));
        }

        public TypeChecked<Class> TypeCheck(Class @class, Scope scope)
        {
            var Position = @class.Position;
            var Name = @class.Name;
            var Methods = @class.Methods;

            var localScope = scope.CreateLocalScope();

            foreach (var method in Methods)
                if (!localScope.TryIncludeUniqueBinding(method))
                    return TypeChecked<Class>.DuplicateIdentifierError(method);

            var typeCheckedMethods = TypeCheck(Methods, localScope);

            var errors = typeCheckedMethods.Errors();
            if (errors.Any())
                return TypeChecked<Class>.Failure(errors);

            return TypeChecked<Class>.Success(new Class(Position, Name, typeCheckedMethods.Functions()));
        }

        public TypeChecked<Function> TypeCheck(Function function, Scope scope)
        {
            var Position = function.Position;
            var ReturnType = function.ReturnType;
            var Name = function.Name;
            var Parameters = function.Parameters;
            var Body = function.Body;
            var DeclaredType = function.DeclaredType;

            var localScope = scope.CreateLocalScope();

            foreach (var parameter in Parameters)
                if (!localScope.TryIncludeUniqueBinding(parameter))
                    return TypeChecked<Function>.DuplicateIdentifierError(parameter);

            var typeCheckedBody = TypeCheck(Body, localScope);
            if (typeCheckedBody.HasErrors)
                return TypeChecked<Function>.Failure(typeCheckedBody.Errors);

            var typedBody = typeCheckedBody.Syntax;
            var unifyErrors = Unify(ReturnType, typedBody);
            if (unifyErrors.Count > 0)
                return TypeChecked<Function>.Failure(unifyErrors);

            return TypeChecked<Function>.Success(new Function(Position, ReturnType, Name, Parameters, typedBody, DeclaredType));
        }

        public TypeChecked<Expression> TypeCheck(Expression expression, Scope scope)
        {
            return expression.WithTypes(this, scope);
        }

        public TypeChecked<Expression> TypeCheck(Name name, Scope scope)
        {
            var Position = name.Position;
            var Identifier = name.Identifier;

            DataType type;

            //TODO: We should probably normalize 'type' before freshening its variables.

            if (scope.TryGet(Identifier, out type))
                return TypeChecked<Expression>.Success(new Name(Position, Identifier, FreshenGenericTypeVariables(scope, type)));

            return TypeChecked<Expression>.UndefinedIdentifierError(name);
        }

        private DataType FreshenGenericTypeVariables(Scope scope, DataType type)
        {
            var substitutions = new Dictionary<TypeVariable, DataType>();
            var genericTypeVariables = type.FindTypeVariables().Where(scope.IsGeneric);
            foreach (var genericTypeVariable in genericTypeVariables)
                substitutions[genericTypeVariable] = CreateTypeVariable();

            return type.ReplaceTypeVariables(substitutions);
        }

        public TypeChecked<Expression> TypeCheck(Block block, Scope scope)
        {
            var Position = block.Position;
            var VariableDeclarations = block.VariableDeclarations;
            var InnerExpressions = block.InnerExpressions;

            var localScope = scope.CreateLocalScope();

            var typedVariableDeclarations = new List<VariableDeclaration>();
            foreach (var variable in VariableDeclarations)
            {
                var typeCheckedValue = TypeCheck(variable.Value, localScope);

                if (typeCheckedValue.HasErrors)
                    return typeCheckedValue;

                var typedValue = typeCheckedValue.Syntax;

                Binding binding = variable;
                if (variable.IsImplicitlyTyped())
                    binding = new VariableDeclaration(variable.Position, /*Replaces implicit type.*/ typedValue.Type, variable.Identifier, variable.Value);

                if (!localScope.TryIncludeUniqueBinding(binding))
                    return TypeChecked<Expression>.DuplicateIdentifierError(binding);

                typedVariableDeclarations.Add(new VariableDeclaration(variable.Position,
                                                                      binding.Type,
                                                                      variable.Identifier,
                                                                      typedValue));

                var unifyErrors = Unify(binding.Type, typedValue);

                if (unifyErrors.Count > 0)
                    return TypeChecked<Expression>.Failure(unifyErrors);
            }

            var typeCheckedInnerExpressions = TypeCheck(InnerExpressions, localScope);

            var errors = typeCheckedInnerExpressions.Errors();
            if (errors.Any())
                return TypeChecked<Expression>.Failure(errors);

            var typedInnerExpressions = typeCheckedInnerExpressions.Expressions();

            var blockType = typedInnerExpressions.Last().Type;

            return TypeChecked<Expression>.Success(new Block(Position, typedVariableDeclarations.ToVector(), typedInnerExpressions, blockType));
        }

        public TypeChecked<Expression> TypeCheck(Lambda lambda, Scope scope)
        {
            var Position = lambda.Position;
            var Parameters = lambda.Parameters;
            var Body = lambda.Body;

            var localScope = scope.CreateLocalScope();

            var typedParameters = ReplaceImplicitTypesWithNewNonGenericTypeVariables(Parameters, localScope);

            foreach (var parameter in typedParameters)
                if (!localScope.TryIncludeUniqueBinding(parameter))
                    return TypeChecked<Expression>.DuplicateIdentifierError(parameter);

            var typeCheckedBody = TypeCheck(Body, localScope);
            if (typeCheckedBody.HasErrors)
                return typeCheckedBody;

            var typedBody = typeCheckedBody.Syntax;

            var normalizedParameters = NormalizeTypes(typedParameters);
            //TODO: Determine whether I should also normalize typedBody.Type for the return below.

            var parameterTypes = normalizedParameters.Select(p => p.Type).ToArray();

            return TypeChecked<Expression>.Success(new Lambda(Position, normalizedParameters, typedBody, NamedType.Function(parameterTypes, typedBody.Type)));
        }

        private Parameter[] ReplaceImplicitTypesWithNewNonGenericTypeVariables(IEnumerable<Parameter> parameters, Scope localScope)
        {
            var decoratedParameters = new List<Parameter>();
            var typeVariables = new List<TypeVariable>();

            foreach (var parameter in parameters)
            {
                if (parameter.IsImplicitlyTyped())
                {
                    var typeVariable = CreateTypeVariable();
                    typeVariables.Add(typeVariable);
                    decoratedParameters.Add(new Parameter(parameter.Position, typeVariable, parameter.Identifier));
                }
                else
                {
                    decoratedParameters.Add(parameter);
                }
            }

            localScope.TreatAsNonGeneric(typeVariables);

            return decoratedParameters.ToArray();
        }

        private Vector<Parameter> NormalizeTypes(IEnumerable<Parameter> typedParameters)
        {
            return typedParameters.Select(p => new Parameter(p.Position, unifier.Normalize(p.Type), p.Identifier)).ToVector();
        }

        public TypeChecked<Expression> TypeCheck(If conditional, Scope scope)
        {
            var Position = conditional.Position;
            var Condition = conditional.Condition;
            var BodyWhenTrue = conditional.BodyWhenTrue;
            var BodyWhenFalse = conditional.BodyWhenFalse;

            var typeCheckedCondition = TypeCheck(Condition, scope);
            var typeCheckedWhenTrue = TypeCheck(BodyWhenTrue, scope);
            var typeCheckedWhenFalse = TypeCheck(BodyWhenFalse, scope);

            if (typeCheckedCondition.HasErrors || typeCheckedWhenTrue.HasErrors || typeCheckedWhenFalse.HasErrors)
                return TypeChecked<Expression>.Failure(new[] { typeCheckedCondition, typeCheckedWhenTrue, typeCheckedWhenFalse }.ToVector().Errors());

            var typedCondition = typeCheckedCondition.Syntax;
            var typedWhenTrue = typeCheckedWhenTrue.Syntax;
            var typedWhenFalse = typeCheckedWhenFalse.Syntax;

            var unifyErrorsA = Unify(NamedType.Boolean, typedCondition);
            var unifyErrorsB = Unify(typedWhenTrue.Type, typedWhenFalse);

            if (unifyErrorsA.Any() || unifyErrorsB.Any())
                return TypeChecked<Expression>.Failure(unifyErrorsA.Concat(unifyErrorsB).ToVector());

            return TypeChecked<Expression>.Success(new If(Position, typedCondition, typedWhenTrue, typedWhenFalse, typedWhenTrue.Type));
        }

        public TypeChecked<Expression> TypeCheck(Call call, Scope scope)
        {
            var Position = call.Position;
            var Callable = call.Callable;
            var Arguments = call.Arguments;
            var IsOperator = call.IsOperator;

            var typeCheckedCallable = TypeCheck(Callable, scope);
            var typeCheckedArguments = TypeCheck(Arguments, scope);

            var errors = new[] { typeCheckedCallable }.Concat(typeCheckedArguments).ToVector().Errors();
            if (errors.Any())
                return TypeChecked<Expression>.Failure(errors);

            var typedCallable = typeCheckedCallable.Syntax;
            var typedArguments = typeCheckedArguments.Expressions();

            var calleeType = typedCallable.Type;
            var calleeFunctionType = calleeType as NamedType;

            if (calleeFunctionType == null || calleeFunctionType.Name != "System.Func")
                return TypeChecked<Expression>.ObjectNotCallableError(Position);

            var returnType = calleeType.InnerTypes.Last();
            var argumentTypes = typedArguments.Select(x => x.Type).ToVector();

            var unifyErrors = new List<CompilerError>(
                unifier.Unify(calleeType, NamedType.Function(argumentTypes, returnType))
                    .Select(error => new CompilerError(Position, error)));
            if (unifyErrors.Count > 0)
                return TypeChecked<Expression>.Failure(unifyErrors.ToVector());

            var callType = unifier.Normalize(returnType);

            return TypeChecked<Expression>.Success(new Call(Position, typedCallable, typedArguments, IsOperator, callType));
        }

        public TypeChecked<Expression> TypeCheck(MethodInvocation methodInvocation, Scope scope)
        {
            var Position = methodInvocation.Position;
            var Instance = methodInvocation.Instance;
            var MethodName = methodInvocation.MethodName;
            var Arguments = methodInvocation.Arguments;

            var typeCheckedInstance = TypeCheck(Instance, scope);

            if (typeCheckedInstance.HasErrors)
                return typeCheckedInstance;

            var typedInstance = typeCheckedInstance.Syntax;
            var instanceType = typedInstance.Type;
            NamedType instanceNamedType = instanceType as NamedType;

            if (instanceNamedType == null)
                return TypeChecked<Expression>.AmbiguousMethodInvocationError(Position);

            Scope typeMemberScope;
            if (scope.TryGetMemberScope(typeRegistry, instanceNamedType, out typeMemberScope))
            {
                //This block is SUSPICIOUSLY like all of TypeCheck(Call, Scope), but Callable/MethodName is evaluated in a special scope and the successful return is different.

                var Callable = MethodName;

                var typeCheckedCallable = TypeCheck(Callable, typeMemberScope);//If typeCheckedCallable.HasErrors, can we avoid giving up and instead see if it is an extension method call, resulting in a TypeChecked Call?

                //EXPERIMENTAL - TRY EXTENSION METHOD WHEN WE FAILED TO FIND THE METHOD IN THE TYPE MEMBER SCOPE
                if (typeCheckedCallable.HasErrors)
                {
                    var extensionMethodCall = TypeCheck(new Call(Position, MethodName, new[] {Instance}.Concat(Arguments)), scope);
                
                    if (!extensionMethodCall.HasErrors)
                        return extensionMethodCall;
                }
                //END EXPERIMENT

                var typeCheckedArguments = TypeCheck(Arguments, scope);

                var errors = new[] { typeCheckedCallable }.Concat(typeCheckedArguments).ToVector().Errors();
                if (errors.Any())
                    return TypeChecked<Expression>.Failure(errors);

                var typedCallable = typeCheckedCallable.Syntax;
                var typedArguments = typeCheckedArguments.Expressions();

                var calleeType = typedCallable.Type;
                var calleeFunctionType = calleeType as NamedType;

                if (calleeFunctionType == null || calleeFunctionType.Name != "System.Func")
                    return TypeChecked<Expression>.ObjectNotCallableError(Position);

                var returnType = calleeType.InnerTypes.Last();
                var argumentTypes = typedArguments.Select(x => x.Type).ToVector();

                var unifyErrors = new List<CompilerError>(
                    unifier.Unify(calleeType, NamedType.Function(argumentTypes, returnType))
                        .Select(error => new CompilerError(Position, error)));
                if (unifyErrors.Count > 0)
                    return TypeChecked<Expression>.Failure(unifyErrors.ToVector());

                var callType = unifier.Normalize(returnType);
                //End of suspiciously duplicated code.

                var typedMethodName = (Name)typedCallable;
                return TypeChecked<Expression>.Success(new MethodInvocation(Position, typedInstance, typedMethodName, typedArguments, callType));
            }
            else
            {
                //HACK: Because TypeRegistry cannot yet look up members for concretions of generic types like int*,
                //  we have to double-check whether this is an extension method call for a regular built-in generic function.
                //  Once TypeRegistry lets you look up the members for a type like int*, this block should be removed.
                var typeCheckedCallable = TypeCheck(MethodName, scope);
                if (!typeCheckedCallable.HasErrors)
                {
                    var extensionMethodCall = TypeCheck(new Call(Position, MethodName, new[] {Instance}.Concat(Arguments)), scope);
                
                    if (!extensionMethodCall.HasErrors)
                        return extensionMethodCall;
                }
                //END HACK

                return TypeChecked<Expression>.UndefinedTypeError(Instance.Position, instanceNamedType);
            }
        }

        public TypeChecked<Expression> TypeCheck(New @new, Scope scope)
        {
            var Position = @new.Position;
            var TypeName = @new.TypeName;

            var typeCheckedTypeName = TypeCheck(TypeName, scope);

            if (typeCheckedTypeName.HasErrors)
                return typeCheckedTypeName;

            var typedTypeName = (Name)typeCheckedTypeName.Syntax;

            var constructorType = typedTypeName.Type as NamedType;

            if (constructorType == null || constructorType.Name != "Rook.Core.Constructor")
                return TypeChecked<Expression>.TypeNameExpectedForConstructionError(TypeName.Position, TypeName);

            var constructedType = constructorType.InnerTypes.Last();

            return TypeChecked<Expression>.Success(new New(Position, typedTypeName, constructedType));
        }

        public TypeChecked<Expression> TypeCheck(BooleanLiteral booleanLiteral, Scope scope)
        {
            return TypeChecked<Expression>.Success(booleanLiteral);
        }

        public TypeChecked<Expression> TypeCheck(IntegerLiteral integerLiteral, Scope scope)
        {
            var Position = integerLiteral.Position;
            var Digits = integerLiteral.Digits;

            int value;

            if (int.TryParse(Digits, out value))
                return TypeChecked<Expression>.Success(new IntegerLiteral(Position, Digits, NamedType.Integer));

            return TypeChecked<Expression>.InvalidConstantError(Position, Digits);
        }


        public TypeChecked<Expression> TypeCheck(StringLiteral stringLiteral, Scope scope)
        {
            return TypeChecked<Expression>.Success(stringLiteral);
        }

        public TypeChecked<Expression> TypeCheck(Null nullLiteral, Scope scope)
        {
            var Position = nullLiteral.Position;
            var result = new Null(Position, NamedType.Nullable(CreateTypeVariable()));
            return TypeChecked<Expression>.Success(result);
        }

        public TypeChecked<Expression> TypeCheck(VectorLiteral vectorLiteral, Scope scope)
        {
            var Position = vectorLiteral.Position;
            var Items = vectorLiteral.Items;

            var typeCheckedItems = TypeCheck(Items, scope);

            var errors = typeCheckedItems.Errors();
            if (errors.Any())
                return TypeChecked<Expression>.Failure(errors);

            var typedItems = typeCheckedItems.Expressions();

            var firstItemType = typedItems.First().Type;

            var unifyErrors = new List<CompilerError>();
            foreach (var typedItem in typedItems)
                unifyErrors.AddRange(Unify(firstItemType, typedItem));

            if (unifyErrors.Count > 0)
                return TypeChecked<Expression>.Failure(unifyErrors.ToVector());

            return TypeChecked<Expression>.Success(new VectorLiteral(Position, typedItems, NamedType.Vector(firstItemType)));
        }

        private Vector<TypeChecked<Expression>> TypeCheck(Vector<Expression> expressions, Scope scope)
        {
            return expressions.Select(x => TypeCheck(x, scope)).ToVector();
        }

        private Vector<TypeChecked<Function>> TypeCheck(Vector<Function> functions, Scope scope)
        {
            return functions.Select(x => TypeCheck(x, scope)).ToVector();
        }

        private Vector<TypeChecked<Class>> TypeCheck(Vector<Class> classes, Scope scope)
        {
            return classes.Select(x => TypeCheck(x, scope)).ToVector();
        }

        private Vector<CompilerError> Unify(DataType type, TypedSyntaxTree typedSyntaxTree)
        {
            return unifier.Unify(type, typedSyntaxTree);
        }
    }
}