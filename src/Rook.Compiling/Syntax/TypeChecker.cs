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

            TryIncludeUniqueBindings(scope, Classes, Functions);

            return new CompilationUnit(Position, TypeCheckAll(Classes, scope), TypeCheckAll(Functions, scope));
        }

        public Class TypeCheck(Class @class, Scope scope)
        {
            var Position = @class.Position;
            var Name = @class.Name;
            var Methods = @class.Methods;

            var localScope = new LocalScope(scope);

            TryIncludeUniqueBindings(localScope, Methods);

            return new Class(Position, Name, TypeCheckAll(Methods, localScope));
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

            TryIncludeUniqueBindings(localScope, Parameters);

            var typeCheckedBody = TypeCheck(Body, localScope);

            Unify(typeCheckedBody.Position, ReturnType, typeCheckedBody.Type);

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
                return new Name(Position, Identifier, type.FreshenGenericTypeVariables());

            LogError(CompilerError.UndefinedIdentifier(name));
            return name;
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

                var binding = variable;
                if (variable.IsImplicitlyTyped())
                    binding = new VariableDeclaration(variable.Position, /*Replaces implicit type.*/ typeCheckedValue.Type, variable.Identifier, variable.Value);

                if (!localScope.TryIncludeUniqueBinding(binding))
                    LogError(CompilerError.DuplicateIdentifier(binding.Position, binding));

                typedVariableDeclarations.Add(new VariableDeclaration(variable.Position,
                                                                      binding.Type,
                                                                      variable.Identifier,
                                                                      typeCheckedValue));

                Unify(typeCheckedValue.Position, binding.Type, typeCheckedValue.Type);
            }

            var typeCheckedInnerExpressions = TypeCheckAll(InnerExpressions, localScope);

            var blockType = typeCheckedInnerExpressions.Last().Type;

            return new Block(Position, typedVariableDeclarations.ToVector(), typeCheckedInnerExpressions, blockType);
        }

        public Expression TypeCheck(Lambda lambda, Scope scope)
        {
            var Position = lambda.Position;
            var Parameters = lambda.Parameters;
            var Body = lambda.Body;

            var lambdaScope = new LocalScope(scope);

            var typedParameters = ReplaceImplicitTypesWithNewNonGenericTypeVariables(Parameters).ToVector();

            TryIncludeUniqueBindings(lambdaScope, typedParameters);

            var typeCheckedBody = TypeCheck(Body, lambdaScope);

            var normalizedParameters = NormalizeTypes(typedParameters);

            var parameterTypes = normalizedParameters.Select(p => p.Type).ToArray();

            return new Lambda(Position, normalizedParameters, typeCheckedBody, NamedType.Function(parameterTypes, typeCheckedBody.Type));
        }

        private static IEnumerable<Parameter> ReplaceImplicitTypesWithNewNonGenericTypeVariables(IEnumerable<Parameter> parameters)
        {
            foreach (var parameter in parameters)
                if (parameter.IsImplicitlyTyped())
                    yield return new Parameter(parameter.Position, TypeVariable.CreateNonGeneric(), parameter.Identifier);
                else
                    yield return parameter;
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

            Unify(typeCheckedCondition.Position, NamedType.Boolean, typeCheckedCondition.Type);
            Unify(typeCheckedWhenFalse.Position, typeCheckedWhenTrue.Type, typeCheckedWhenFalse.Type);

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

            var calleeType = typeCheckedCallable.Type;
            var calleeFunctionType = calleeType as NamedType;

            if (calleeFunctionType == null || calleeFunctionType.Name != "System.Func")
            {
                LogError(CompilerError.ObjectNotCallable(Position));
                return call;
            }

            var returnType = calleeType.InnerTypes.Last();
            var argumentTypes = typeCheckedArguments.Select(x => x.Type).ToVector();

            Unify(Position, calleeType, NamedType.Function(argumentTypes, returnType));

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

            var instanceType = typeCheckedInstance.Type;
            var instanceNamedType = instanceType as NamedType;

            if (instanceNamedType == null)
            {
                LogError(CompilerError.AmbiguousMethodInvocation(Position));
                return methodInvocation;
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

                var calleeType = typeCheckedCallable.Type;
                var calleeFunctionType = calleeType as NamedType;

                if (calleeFunctionType == null || calleeFunctionType.Name != "System.Func")
                {
                    LogError(CompilerError.ObjectNotCallable(Position));
                    return methodInvocation;
                }

                var returnType = calleeType.InnerTypes.Last();
                var argumentTypes = typeCheckedArguments.Select(x => x.Type).ToVector();

                Unify(Position, calleeType, NamedType.Function(argumentTypes, returnType));

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
                return methodInvocation;
            }
        }

        public Expression TypeCheck(New @new, Scope scope)
        {
            var Position = @new.Position;
            var TypeName = @new.TypeName;

            var typeCheckedTypeName = TypeCheck(TypeName, scope);

            var typedTypeName = (Name)typeCheckedTypeName;

            var constructorType = typedTypeName.Type as NamedType;

            if (constructorType == null || constructorType.Name != "Rook.Core.Constructor")
            {
                LogError(CompilerError.TypeNameExpectedForConstruction(TypeName.Position, TypeName));
                return @new;
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
            return integerLiteral;
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

            var firstItemType = typeCheckedItems.First().Type;

            foreach (var typedItem in typeCheckedItems)
                Unify(typedItem.Position, firstItemType, typedItem.Type);

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

        private void Unify(Position position, DataType typeA, DataType typeB)
        {
            //Attempts to unify a type with UnknownType.Instance would produce
            //a redundant and unhelpful error message.  Errors that would lead
            //to unifying with UnknownType.Instance should already report useful
            //error messages prior to unification.

            if (typeA == UnknownType.Instance || typeB == UnknownType.Instance)
                return;

            var errors = unifier.Unify(typeA, typeB).Select(error => new CompilerError(position, error)).ToVector();

            var succeeded = !errors.Any();

            if (!succeeded)
                LogErrors(errors);
        }

        private void LogErrors(IEnumerable<CompilerError> errors)
        {
            errorLog.AddRange(errors);
        }

        private void LogError(CompilerError error)
        {
            errorLog.Add(error);
        }

        private void TryIncludeUniqueBindings(GlobalScope globals, Vector<Class> classes, Vector<Function> functions)
        {
            foreach (var @class in classes)
                if (!globals.TryIncludeUniqueBinding(@class))
                    LogError(CompilerError.DuplicateIdentifier(@class.Position, @class));

            foreach (var function in functions)
                if (!globals.TryIncludeUniqueBinding(function))
                    LogError(CompilerError.DuplicateIdentifier(function.Position, function));
        }

        private void TryIncludeUniqueBindings(LocalScope locals, Vector<Function> methods)
        {
            foreach (var method in methods)
                if (!locals.TryIncludeUniqueBinding(method))
                    LogError(CompilerError.DuplicateIdentifier(method.Position, method));
        }

        private void TryIncludeUniqueBindings(LocalScope locals, Vector<Parameter> parameters)
        {
            foreach (var parameter in parameters)
                if (!locals.TryIncludeUniqueBinding(parameter))
                    LogError(CompilerError.DuplicateIdentifier(parameter.Position, parameter));
        }
    }
}