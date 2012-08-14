using System.Collections.Generic;
using System.Linq;
using Parsley;
using Rook.Compiling.Types;
using Rook.Core.Collections;

namespace Rook.Compiling.Syntax
{
    public class TypeChecker
    {
        private readonly TypeUnifier unifier;
        private readonly TypeRegistry typeRegistry;
        private readonly List<CompilerError> errorLog;

        public TypeChecker()
        {
            unifier = new TypeUnifier();
            typeRegistry = new TypeRegistry();
            errorLog = new List<CompilerError>();
        }

        public Vector<CompilerError> Errors { get { return errorLog.ToVector(); } }
        public bool HasErrors { get { return errorLog.Any(); } }

        //TODO: This property is deprecated.  Once TypeRegistry can discover .NET types via reflection,
        //rephrase unit test usages of this so they don't have to manually prepare the TypeRegistry.
        public TypeRegistry TypeRegistry { get { return typeRegistry; } }

        public CompilationUnit TypeCheck(CompilationUnit compilationUnit)
        {
            var Position = compilationUnit.Position;
            var Classes = compilationUnit.Classes;
            var Functions = compilationUnit.Functions;

            foreach (var @class in Classes)//TODO: Test coverage.
                typeRegistry.Register(@class);

            var scope = new GlobalScope();

            foreach (var @class in Classes)
                if (!scope.TryIncludeUniqueBinding(@class))
                {
                    LogError(CompilerError.DuplicateIdentifier(@class.Position, @class));
                    return null;
                }

            foreach (var function in Functions)
                if (!scope.TryIncludeUniqueBinding(function))
                {
                    LogError(CompilerError.DuplicateIdentifier(function.Position, function));
                    return null;
                }

            var typeCheckedClasses = TypeCheckAll(Classes, scope);
            var typeCheckedFunctions = TypeCheckAll(Functions, scope);

            if (typeCheckedClasses.Any(x => x == null) || typeCheckedFunctions.Any(x => x == null))
                return null;

            return new CompilationUnit(Position, typeCheckedClasses, typeCheckedFunctions);
        }

        public Class TypeCheck(Class @class, Scope scope)
        {
            var Position = @class.Position;
            var Name = @class.Name;
            var Methods = @class.Methods;

            var localScope = new LocalScope(scope);

            foreach (var method in Methods)
                if (!localScope.TryIncludeUniqueBinding(method))
                {
                    LogError(CompilerError.DuplicateIdentifier(method.Position, method));
                    return null;
                }

            var typeCheckedMethods = TypeCheckAll(Methods, localScope);

            if (typeCheckedMethods.Any(x => x == null))
                return null;

            return new Class(Position, Name, typeCheckedMethods);
        }

        public Function TypeCheck(Function function, Scope scope)
        {
            var Position = function.Position;
            var ReturnType = function.ReturnType;
            var Name = function.Name;
            var Parameters = function.Parameters;
            var Body = function.Body;
            var DeclaredType = function.DeclaredType;

            var localScope = new LocalScope(scope);

            foreach (var parameter in Parameters)
                if (!localScope.TryIncludeUniqueBinding(parameter))
                {
                    LogError(CompilerError.DuplicateIdentifier(parameter.Position, parameter));
                    return null;
                }

            var typeCheckedBody = TypeCheck(Body, localScope);
            if (typeCheckedBody == null)
                return null;

            if (!Unify(typeCheckedBody.Position, ReturnType, typeCheckedBody.Type))
                return null;

            return new Function(Position, ReturnType, Name, Parameters, typeCheckedBody, DeclaredType);
        }

        public Expression TypeCheck(Expression expression, Scope scope)
        {
            return expression.WithTypes(this, scope);
        }

        public Expression TypeCheck(Name name, Scope scope)
        {
            var Position = name.Position;
            var Identifier = name.Identifier;

            DataType type;

            if (scope.TryGet(Identifier, out type))
                return new Name(Position, Identifier, FreshenGenericTypeVariables(type));

            LogError(CompilerError.UndefinedIdentifier(name));
            return null;
        }

        private static DataType FreshenGenericTypeVariables(DataType type)
        {
            var substitutions = new Dictionary<TypeVariable, DataType>();
            var genericTypeVariables = type.FindTypeVariables().Where(x => x.IsGeneric);
            foreach (var genericTypeVariable in genericTypeVariables)
                substitutions[genericTypeVariable] = TypeVariable.CreateGeneric();

            return type.ReplaceTypeVariables(substitutions);
        }

        public Expression TypeCheck(Block block, Scope scope)
        {
            var Position = block.Position;
            var VariableDeclarations = block.VariableDeclarations;
            var InnerExpressions = block.InnerExpressions;

            var localScope = new LocalScope(scope);

            var typedVariableDeclarations = new List<VariableDeclaration>();
            foreach (var variable in VariableDeclarations)
            {
                var typeCheckedValue = TypeCheck(variable.Value, localScope);

                if (typeCheckedValue == null)
                    return null;

                var binding = variable;
                if (variable.IsImplicitlyTyped())
                    binding = new VariableDeclaration(variable.Position, /*Replaces implicit type.*/ typeCheckedValue.Type, variable.Identifier, variable.Value);

                if (!localScope.TryIncludeUniqueBinding(binding))
                {
                    LogError(CompilerError.DuplicateIdentifier(binding.Position, binding));
                    return null;
                }

                typedVariableDeclarations.Add(new VariableDeclaration(variable.Position,
                                                                      binding.Type,
                                                                      variable.Identifier,
                                                                      typeCheckedValue));

                if (!Unify(typeCheckedValue.Position, binding.Type, typeCheckedValue.Type))
                    return null;
            }

            var typeCheckedInnerExpressions = TypeCheckAll(InnerExpressions, localScope);

            if (typeCheckedInnerExpressions.Any(x => x == null))
                return null;

            var blockType = typeCheckedInnerExpressions.Last().Type;

            return new Block(Position, typedVariableDeclarations.ToVector(), typeCheckedInnerExpressions, blockType);
        }

        public Expression TypeCheck(Lambda lambda, Scope scope)
        {
            var Position = lambda.Position;
            var Parameters = lambda.Parameters;
            var Body = lambda.Body;

            var lambdaScope = new LocalScope(scope);

            var typedParameters = ReplaceImplicitTypesWithNewNonGenericTypeVariables(Parameters);

            foreach (var parameter in typedParameters)
            {
                if (!lambdaScope.TryIncludeUniqueBinding(parameter))
                {
                    LogError(CompilerError.DuplicateIdentifier(parameter.Position, parameter));
                    return null;
                }
            }

            var typeCheckedBody = TypeCheck(Body, lambdaScope);
            if (typeCheckedBody == null)
                return null;

            var normalizedParameters = NormalizeTypes(typedParameters);

            var parameterTypes = normalizedParameters.Select(p => p.Type).ToArray();

            return new Lambda(Position, normalizedParameters, typeCheckedBody, NamedType.Function(parameterTypes, typeCheckedBody.Type));
        }

        private Parameter[] ReplaceImplicitTypesWithNewNonGenericTypeVariables(IEnumerable<Parameter> parameters)
        {
            var decoratedParameters = new List<Parameter>();
            var typeVariables = new List<TypeVariable>();

            foreach (var parameter in parameters)
            {
                if (parameter.IsImplicitlyTyped())
                {
                    var typeVariable = TypeVariable.CreateNonGeneric();
                    typeVariables.Add(typeVariable);
                    decoratedParameters.Add(new Parameter(parameter.Position, typeVariable, parameter.Identifier));
                }
                else
                {
                    decoratedParameters.Add(parameter);
                }
            }

            return decoratedParameters.ToArray();
        }

        private Vector<Parameter> NormalizeTypes(IEnumerable<Parameter> typedParameters)
        {
            return typedParameters.Select(p => new Parameter(p.Position, unifier.Normalize(p.Type), p.Identifier)).ToVector();
        }

        public Expression TypeCheck(If conditional, Scope scope)
        {
            var Position = conditional.Position;
            var Condition = conditional.Condition;
            var BodyWhenTrue = conditional.BodyWhenTrue;
            var BodyWhenFalse = conditional.BodyWhenFalse;

            var typeCheckedCondition = TypeCheck(Condition, scope);
            var typeCheckedWhenTrue = TypeCheck(BodyWhenTrue, scope);
            var typeCheckedWhenFalse = TypeCheck(BodyWhenFalse, scope);

            if (typeCheckedCondition == null || typeCheckedWhenTrue == null || typeCheckedWhenFalse == null)
                return null;

            var unifiedA = Unify(typeCheckedCondition.Position, NamedType.Boolean, typeCheckedCondition.Type);
            var unifiedB = Unify(typeCheckedWhenFalse.Position, typeCheckedWhenTrue.Type, typeCheckedWhenFalse.Type);

            if (!unifiedA || !unifiedB)
                return null;

            return new If(Position, typeCheckedCondition, typeCheckedWhenTrue, typeCheckedWhenFalse, typeCheckedWhenTrue.Type);
        }

        public Expression TypeCheck(Call call, Scope scope)
        {
            var Position = call.Position;
            var Callable = call.Callable;
            var Arguments = call.Arguments;
            var IsOperator = call.IsOperator;

            var typeCheckedCallable = TypeCheck(Callable, scope);
            var typeCheckedArguments = TypeCheckAll(Arguments, scope);

            if (typeCheckedCallable == null || typeCheckedArguments.Any(x => x == null))
                return null;

            var calleeType = typeCheckedCallable.Type;
            var calleeFunctionType = calleeType as NamedType;

            if (calleeFunctionType == null || calleeFunctionType.Name != "System.Func")
            {
                LogError(CompilerError.ObjectNotCallable(Position));
                return null;
            }

            var returnType = calleeType.InnerTypes.Last();
            var argumentTypes = typeCheckedArguments.Select(x => x.Type).ToVector();

            if (!Unify(Position, calleeType, NamedType.Function(argumentTypes, returnType)))
                return null;

            var callType = unifier.Normalize(returnType);

            return new Call(Position, typeCheckedCallable, typeCheckedArguments, IsOperator, callType);
        }

        public Expression TypeCheck(MethodInvocation methodInvocation, Scope scope)
        {
            var Position = methodInvocation.Position;
            var Instance = methodInvocation.Instance;
            var MethodName = methodInvocation.MethodName;
            var Arguments = methodInvocation.Arguments;

            var typeCheckedInstance = TypeCheck(Instance, scope);

            if (typeCheckedInstance == null)
                return null;

            var instanceType = typeCheckedInstance.Type;
            var instanceNamedType = instanceType as NamedType;

            if (instanceNamedType == null)
            {
                LogError(CompilerError.AmbiguousMethodInvocation(Position));
                return null;
            }

            Vector<Binding> typeMembers;
            if (typeRegistry.TryGetMembers(instanceNamedType, out typeMembers))
            {
                Scope typeMemberScope = new TypeMemberScope(typeMembers);

                //This block is SUSPICIOUSLY like all of TypeCheck(Call, Scope), but Callable/MethodName is evaluated in a special scope and the successful return is different.

                var Callable = MethodName;

                //EXPERIMENTAL - TRY EXTENSION METHOD WHEN WE FAILED TO FIND THE METHOD IN THE TYPE MEMBER SCOPE
                if (!typeMemberScope.Contains(Callable.Identifier))
                {
                    var extensionMethodCall = TypeCheck(new Call(Position, MethodName, new[] { Instance }.Concat(Arguments)), scope);

                    if (extensionMethodCall != null)
                        return extensionMethodCall;
                }

                var typeCheckedCallable = TypeCheck(Callable, typeMemberScope);
                
                //END EXPERIMENT

                var typeCheckedArguments = TypeCheckAll(Arguments, scope);

                if (typeCheckedCallable == null || typeCheckedArguments.Any(x => x == null))
                    return null;

                var calleeType = typeCheckedCallable.Type;
                var calleeFunctionType = calleeType as NamedType;

                if (calleeFunctionType == null || calleeFunctionType.Name != "System.Func")
                {
                    LogError(CompilerError.ObjectNotCallable(Position));
                    return null;
                }

                var returnType = calleeType.InnerTypes.Last();
                var argumentTypes = typeCheckedArguments.Select(x => x.Type).ToVector();

                if (!Unify(Position, calleeType, NamedType.Function(argumentTypes, returnType)))
                    return null;

                var callType = unifier.Normalize(returnType);
                //End of suspiciously duplicated code.

                var typedMethodName = (Name)typeCheckedCallable;
                return new MethodInvocation(Position, typeCheckedInstance, typedMethodName, typeCheckedArguments, callType);
            }
            else
            {
                //HACK: Because TypeRegistry cannot yet look up members for concretions of generic types like int*,
                //  we have to double-check whether this is an extension method call for a regular built-in generic function.
                //  Once TypeRegistry lets you look up the members for a type like int*, this block should be removed.
                if (scope.Contains(MethodName.Identifier))
                {
                    var typeCheckedCallable = TypeCheck(MethodName, scope);
                    if (typeCheckedCallable != null)
                    {
                        var extensionMethodCall = TypeCheck(new Call(Position, MethodName, new[] { Instance }.Concat(Arguments)), scope);

                        if (extensionMethodCall != null)
                            return extensionMethodCall;
                    }
                }
                //END HACK

                LogError(CompilerError.UndefinedType(Instance.Position, instanceNamedType));
                return null;
            }
        }

        public Expression TypeCheck(New @new, Scope scope)
        {
            var Position = @new.Position;
            var TypeName = @new.TypeName;

            var typeCheckedTypeName = TypeCheck(TypeName, scope);

            if (typeCheckedTypeName == null)
                return null;

            var typedTypeName = (Name)typeCheckedTypeName;

            var constructorType = typedTypeName.Type as NamedType;

            if (constructorType == null || constructorType.Name != "Rook.Core.Constructor")
            {
                LogError(CompilerError.TypeNameExpectedForConstruction(TypeName.Position, TypeName));
                return null;
            }

            var constructedType = constructorType.InnerTypes.Last();

            return new New(Position, typedTypeName, constructedType);
        }

        public Expression TypeCheck(BooleanLiteral booleanLiteral, Scope scope)
        {
            return booleanLiteral;
        }

        public Expression TypeCheck(IntegerLiteral integerLiteral, Scope scope)
        {
            var Position = integerLiteral.Position;
            var Digits = integerLiteral.Digits;

            int value;

            if (int.TryParse(Digits, out value))
                return new IntegerLiteral(Position, Digits, NamedType.Integer);

            LogError(CompilerError.InvalidConstant(Position, Digits));
            return null;
        }

        public Expression TypeCheck(StringLiteral stringLiteral, Scope scope)
        {
            return stringLiteral;
        }

        public Expression TypeCheck(Null nullLiteral, Scope scope)
        {
            var Position = nullLiteral.Position;

            return new Null(Position, NamedType.Nullable(TypeVariable.CreateGeneric()));
        }

        public Expression TypeCheck(VectorLiteral vectorLiteral, Scope scope)
        {
            var Position = vectorLiteral.Position;
            var Items = vectorLiteral.Items;

            var typeCheckedItems = TypeCheckAll(Items, scope);

            if (typeCheckedItems.Any(typeCheckedItem => typeCheckedItem == null))
                return null;

            var firstItemType = typeCheckedItems.First().Type;

            var unificationResults = typeCheckedItems.Select(typedItem => Unify(typedItem.Position, firstItemType, typedItem.Type)).ToArray();

            if (unificationResults.Any(x => x == false))
                return null;

            return new VectorLiteral(Position, typeCheckedItems, NamedType.Vector(firstItemType));
        }

        private Vector<Expression> TypeCheckAll(Vector<Expression> expressions, Scope scope)
        {
            return expressions.Select(x => TypeCheck(x, scope)).ToVector();
        }

        private Vector<Function> TypeCheckAll(Vector<Function> functions, Scope scope)
        {
            return functions.Select(x => TypeCheck(x, scope)).ToVector();
        }

        private Vector<Class> TypeCheckAll(Vector<Class> classes, Scope scope)
        {
            return classes.Select(x => TypeCheck(x, scope)).ToVector();
        }

        private bool Unify(Position position, DataType typeA, DataType typeB)
        {
            var errors = unifier.Unify(typeA, typeB).Select(error => new CompilerError(position, error)).ToVector();

            var succeeded = !errors.Any();

            if (!succeeded)
                LogErrors(errors);

            return succeeded;
        }

        private void LogErrors(IEnumerable<CompilerError> errors)
        {
            errorLog.AddRange(errors);
        }

        private void LogError(CompilerError error)
        {
            errorLog.Add(error);
        }
    }
}