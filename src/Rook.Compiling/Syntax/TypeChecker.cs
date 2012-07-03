using System.Collections.Generic;
using System.Linq;
using Rook.Compiling.Types;
using Rook.Core.Collections;

namespace Rook.Compiling.Syntax
{
    public class TypeChecker
    {
        public TypeChecked<CompilationUnit> TypeCheck(CompilationUnit compilationUnit)
        {
            var Position = compilationUnit.Position;
            var Classes = compilationUnit.Classes;
            var Functions = compilationUnit.Functions;

            var unifier = new TypeUnifier();

            var scope = Scope.CreateRoot(unifier.CreateTypeVariable, Classes);

            foreach (var @class in Classes)
                if (!scope.TryIncludeUniqueBinding(@class))
                    return TypeChecked<CompilationUnit>.DuplicateIdentifierError(@class);

            foreach (var function in Functions)
                if (!scope.TryIncludeUniqueBinding(function))
                    return TypeChecked<CompilationUnit>.DuplicateIdentifierError(function);

            var typeCheckedClasses = TypeCheck(Classes, scope, unifier);
            var typeCheckedFunctions = TypeCheck(Functions, scope, unifier);

            var classErrors = typeCheckedClasses.Errors();
            var functionErrors = typeCheckedFunctions.Errors();

            if (classErrors.Any() || functionErrors.Any())
                return TypeChecked<CompilationUnit>.Failure(classErrors.Concat(functionErrors).ToVector());

            return TypeChecked<CompilationUnit>.Success(new CompilationUnit(Position, typeCheckedClasses.Classes(), typeCheckedFunctions.Functions()));
        }

        public TypeChecked<Class> TypeCheck(Class @class, Scope scope, TypeUnifier unifier)
        {
            var Position = @class.Position;
            var Name = @class.Name;
            var Methods = @class.Methods;

            var localScope = scope.CreateLocalScope();

            foreach (var method in Methods)
                if (!localScope.TryIncludeUniqueBinding(method))
                    return TypeChecked<Class>.DuplicateIdentifierError(method);

            var typeCheckedMethods = TypeCheck(Methods, localScope, unifier);

            var errors = typeCheckedMethods.Errors();
            if (errors.Any())
                return TypeChecked<Class>.Failure(errors);

            return TypeChecked<Class>.Success(new Class(Position, Name, typeCheckedMethods.Functions()));
        }

        public TypeChecked<Function> TypeCheck(Function function, Scope scope, TypeUnifier unifier)
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

            var typeCheckedBody = Body.WithTypes(this, localScope, unifier);
            if (typeCheckedBody.HasErrors)
                return TypeChecked<Function>.Failure(typeCheckedBody.Errors);

            var typedBody = typeCheckedBody.Syntax;
            var unifyErrors = unifier.Unify(ReturnType, typedBody);
            if (unifyErrors.Count > 0)
                return TypeChecked<Function>.Failure(unifyErrors);

            return TypeChecked<Function>.Success(new Function(Position, ReturnType, Name, Parameters, typedBody, DeclaredType));
        }

        public TypeChecked<Expression> TypeCheck(Expression expression, Scope scope, TypeUnifier unifier)
        {
            return expression.WithTypes(this, scope, unifier);
        }

        public TypeChecked<Expression> TypeCheck(Name name, Scope scope, TypeUnifier unifier)
        {
            var Position = name.Position;
            var Identifier = name.Identifier;

            DataType type;

            //TODO: We should probably normalize 'type' before freshening its variables.

            if (scope.TryGet(Identifier, out type))
                return TypeChecked<Expression>.Success(new Name(Position, Identifier, FreshenGenericTypeVariables(scope, type, unifier)));

            return TypeChecked<Expression>.UndefinedIdentifierError(Position, Identifier);
        }

        private static DataType FreshenGenericTypeVariables(Scope scope, DataType type, TypeUnifier unifier)
        {
            var substitutions = new Dictionary<TypeVariable, DataType>();
            var genericTypeVariables = type.FindTypeVariables().Where(scope.IsGeneric);
            foreach (var genericTypeVariable in genericTypeVariables)
                substitutions[genericTypeVariable] = unifier.CreateTypeVariable();

            return type.ReplaceTypeVariables(substitutions);
        }

        public TypeChecked<Expression> TypeCheck(Block block, Scope scope, TypeUnifier unifier)
        {
            var Position = block.Position;
            var VariableDeclarations = block.VariableDeclarations;
            var InnerExpressions = block.InnerExpressions;

            var localScope = scope.CreateLocalScope();

            var typedVariableDeclarations = new List<VariableDeclaration>();
            foreach (var variable in VariableDeclarations)
            {
                var typeCheckedValue = variable.Value.WithTypes(this, localScope, unifier);

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

                var unifyErrors = unifier.Unify(binding.Type, typedValue);

                if (unifyErrors.Count > 0)
                    return TypeChecked<Expression>.Failure(unifyErrors);
            }

            var typeCheckedInnerExpressions = TypeCheck(InnerExpressions, localScope, unifier);

            var errors = typeCheckedInnerExpressions.Errors();
            if (errors.Any())
                return TypeChecked<Expression>.Failure(errors);

            var typedInnerExpressions = typeCheckedInnerExpressions.Expressions();

            var blockType = typedInnerExpressions.Last().Type;

            return TypeChecked<Expression>.Success(new Block(Position, typedVariableDeclarations.ToVector(), typedInnerExpressions, blockType));
        }

        public TypeChecked<Expression> TypeCheck(Lambda lambda, Scope scope, TypeUnifier unifier)
        {
            var Position = lambda.Position;
            var Parameters = lambda.Parameters;
            var Body = lambda.Body;

            var localScope = scope.CreateLocalScope();

            var typedParameters = ReplaceImplicitTypesWithNewNonGenericTypeVariables(Parameters, localScope, unifier);

            foreach (var parameter in typedParameters)
                if (!localScope.TryIncludeUniqueBinding(parameter))
                    return TypeChecked<Expression>.DuplicateIdentifierError(parameter);

            var typeCheckedBody = Body.WithTypes(this, localScope, unifier);
            if (typeCheckedBody.HasErrors)
                return typeCheckedBody;

            Expression typedBody = typeCheckedBody.Syntax;

            var normalizedParameters = NormalizeTypes(typedParameters, unifier);
            //TODO: Determine whether I should also normalize typedBody.Type for the return below.

            var parameterTypes = normalizedParameters.Select(p => p.Type).ToArray();

            return TypeChecked<Expression>.Success(new Lambda(Position, normalizedParameters, typedBody, NamedType.Function(parameterTypes, typedBody.Type)));
        }

        private static Parameter[] ReplaceImplicitTypesWithNewNonGenericTypeVariables(IEnumerable<Parameter> parameters, Scope localScope, TypeUnifier unifier)
        {
            var decoratedParameters = new List<Parameter>();
            var typeVariables = new List<TypeVariable>();

            foreach (var parameter in parameters)
            {
                if (parameter.IsImplicitlyTyped())
                {
                    var typeVariable = unifier.CreateTypeVariable();
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

        private static Vector<Parameter> NormalizeTypes(IEnumerable<Parameter> typedParameters, TypeUnifier unifier)
        {
            return typedParameters.Select(p => new Parameter(p.Position, unifier.Normalize(p.Type), p.Identifier)).ToVector();
        }

        public TypeChecked<Expression> TypeCheck(If conditional, Scope scope, TypeUnifier unifier)
        {
            var Position = conditional.Position;
            var Condition = conditional.Condition;
            var BodyWhenTrue = conditional.BodyWhenTrue;
            var BodyWhenFalse = conditional.BodyWhenFalse;

            TypeChecked<Expression> typeCheckedCondition = Condition.WithTypes(this, scope, unifier);
            TypeChecked<Expression> typeCheckedWhenTrue = BodyWhenTrue.WithTypes(this, scope, unifier);
            TypeChecked<Expression> typeCheckedWhenFalse = BodyWhenFalse.WithTypes(this, scope, unifier);

            if (typeCheckedCondition.HasErrors || typeCheckedWhenTrue.HasErrors || typeCheckedWhenFalse.HasErrors)
                return TypeChecked<Expression>.Failure(new[] { typeCheckedCondition, typeCheckedWhenTrue, typeCheckedWhenFalse }.ToVector().Errors());

            Expression typedCondition = typeCheckedCondition.Syntax;
            Expression typedWhenTrue = typeCheckedWhenTrue.Syntax;
            Expression typedWhenFalse = typeCheckedWhenFalse.Syntax;

            var unifyErrorsA = unifier.Unify(NamedType.Boolean, typedCondition);
            var unifyErrorsB = unifier.Unify(typedWhenTrue.Type, typedWhenFalse);

            if (unifyErrorsA.Any() || unifyErrorsB.Any())
                return TypeChecked<Expression>.Failure(unifyErrorsA.Concat(unifyErrorsB).ToVector());

            return TypeChecked<Expression>.Success(new If(Position, typedCondition, typedWhenTrue, typedWhenFalse, typedWhenTrue.Type));
        }

        public TypeChecked<Expression> TypeCheck(Call call, Scope scope, TypeUnifier unifier)
        {
            var Position = call.Position;
            var Callable = call.Callable;
            var Arguments = call.Arguments;
            var IsOperator = call.IsOperator;

            TypeChecked<Expression> typeCheckedCallable = Callable.WithTypes(this, scope, unifier);
            var typeCheckedArguments = TypeCheck(Arguments, scope, unifier);

            var errors = new[] { typeCheckedCallable }.Concat(typeCheckedArguments).ToVector().Errors();
            if (errors.Any())
                return TypeChecked<Expression>.Failure(errors);

            Expression typedCallable = typeCheckedCallable.Syntax;
            var typedArguments = typeCheckedArguments.Expressions();

            DataType calleeType = typedCallable.Type;
            NamedType calleeFunctionType = calleeType as NamedType;

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

        public TypeChecked<Expression> TypeCheck(New @new, Scope scope, TypeUnifier unifier)
        {
            var Position = @new.Position;
            var TypeName = @new.TypeName;

            var typeCheckedTypeName = TypeName.WithTypes(this, scope, unifier);

            if (typeCheckedTypeName.HasErrors)
                return typeCheckedTypeName;

            var typedTypeName = (Name)typeCheckedTypeName.Syntax;

            var constructorType = typedTypeName.Type as NamedType;

            if (constructorType == null || constructorType.Name != "Rook.Core.Constructor")
                return TypeChecked<Expression>.TypeNameExpectedForConstructionError(TypeName.Position, TypeName);

            var constructedType = constructorType.InnerTypes.Last();

            return TypeChecked<Expression>.Success(new New(Position, typedTypeName, constructedType));
        }

        public TypeChecked<Expression> TypeCheck(BooleanLiteral booleanLiteral, Scope scope, TypeUnifier unifier)
        {
            return TypeChecked<Expression>.Success(booleanLiteral);
        }

        public TypeChecked<Expression> TypeCheck(IntegerLiteral integerLiteral, Scope scope, TypeUnifier unifier)
        {
            var Position = integerLiteral.Position;
            var Digits = integerLiteral.Digits;

            int value;

            if (int.TryParse(Digits, out value))
                return TypeChecked<Expression>.Success(new IntegerLiteral(Position, Digits, NamedType.Integer));

            return TypeChecked<Expression>.InvalidConstantError(Position, Digits);
        }


        public TypeChecked<Expression> TypeCheck(StringLiteral stringLiteral, Scope scope, TypeUnifier unifier)
        {
            return TypeChecked<Expression>.Success(stringLiteral);
        }

        public TypeChecked<Expression> TypeCheck(Null nullLiteral, Scope scope, TypeUnifier unifier)
        {
            var Position = nullLiteral.Position;
            var result = new Null(Position, NamedType.Nullable(unifier.CreateTypeVariable()));
            return TypeChecked<Expression>.Success(result);
        }

        public TypeChecked<Expression> TypeCheck(VectorLiteral vectorLiteral, Scope scope, TypeUnifier unifier)
        {
            var Position = vectorLiteral.Position;
            var Items = vectorLiteral.Items;

            var typeCheckedItems = TypeCheck(Items, scope, unifier);

            var errors = typeCheckedItems.Errors();
            if (errors.Any())
                return TypeChecked<Expression>.Failure(errors);

            var typedItems = typeCheckedItems.Expressions();

            var firstItemType = typedItems.First().Type;

            var unifyErrors = new List<CompilerError>();
            foreach (var typedItem in typedItems)
                unifyErrors.AddRange(unifier.Unify(firstItemType, typedItem));

            if (unifyErrors.Count > 0)
                return TypeChecked<Expression>.Failure(unifyErrors.ToVector());

            return TypeChecked<Expression>.Success(new VectorLiteral(Position, typedItems, NamedType.Vector(firstItemType)));
        }

        private Vector<TypeChecked<Expression>> TypeCheck(Vector<Expression> expressions, Scope scope, TypeUnifier unifier)
        {
            return expressions.Select(x => x.WithTypes(this, scope, unifier)).ToVector();
        }

        private Vector<TypeChecked<Function>> TypeCheck(Vector<Function> functions, Scope scope, TypeUnifier unifier)
        {
            return functions.Select(x => x.WithTypes(this, scope, unifier)).ToVector();
        }

        private Vector<TypeChecked<Class>> TypeCheck(Vector<Class> classes, Scope scope, TypeUnifier unifier)
        {
            return classes.Select(x => x.WithTypes(this, scope, unifier)).ToVector();
        }
    }
}