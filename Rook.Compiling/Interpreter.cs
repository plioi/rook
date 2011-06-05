using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Parsley;
using Rook.Compiling.CodeGeneration;
using Rook.Compiling.Syntax;
using Rook.Compiling.Types;

namespace Rook.Compiling
{
    public class Interpreter
    {
        private readonly RookCompiler compiler;
        private readonly Dictionary<string, Function> functions;

        public Interpreter()
        {
            compiler = new RookCompiler(CompilerParameters.ForBasicEvaluation());
            functions = new Dictionary<string, Function>();
        }

        public bool CanParse(string code)
        {
            var tokens = new RookLexer(code);
            Function function;
            Expression expression;

            return TryParse(tokens, Grammar.Function, out function) ||
                   TryParse(tokens, Grammar.Expression, out expression);
        }

        public InterpreterResult Interpret(string code)
        {
            var tokens = new RookLexer(code);
            var pos = tokens.Position;
            
            Expression expression;
            if (TryParse(tokens, Grammar.Expression, out expression))
                return InterpretExpression(expression, pos);

            Function function;
            if (TryParse(tokens, Grammar.Function, out function))
                return InterpretFunction(function, pos);

            return Error("Cannot evaluate this code: must be a function or expression.");
        }

        public string Translate()
        {
            var program = new Program(new Text("").Position, functions.Values);
            Program typeCheckedProgram = program.WithTypes().Syntax;
            var code = new CodeWriter();
            WriteAction write = typeCheckedProgram.Visit(new CSharpTranslator());
            write(code);
            return code.ToString();
        }

        private InterpreterResult InterpretExpression(Expression expression, Position pos)
        {
            var typedCheckedExpression = expression.WithTypes(EnvironmentForExpression());
            if (typedCheckedExpression.HasErrors)
                return new InterpreterResult(typedCheckedExpression.Errors);
                
            var main = WrapAsMain(typedCheckedExpression.Syntax, pos);
            var compilerResult = compiler.Build(ProgramWithNewFunction(main, pos));

            functions[main.Name.Identifier] = main;

            return new InterpreterResult(CallMain(compilerResult.CompiledAssembly));
        }

        private InterpreterResult InterpretFunction(Function function, Position pos)
        {
            if (function.Name.Identifier == "Main")
                return Error("The Main function is reserved for expression evaluation, and cannot be explicitly defined.");

            var compilerResult = compiler.Build(ProgramWithNewFunction(function, pos));
            if (compilerResult.Errors.Any())
                return new InterpreterResult(compilerResult.Errors);

            functions[function.Name.Identifier] = function;

            return new InterpreterResult(function);
        }

        private Program ProgramWithNewFunction(Function function, Position pos)
        {
            var functionsExceptPotentialOverwrite = functions.Values.Where(f => f.Name.Identifier != function.Name.Identifier);
            return new Program(pos, new[] { function }.Concat(functionsExceptPotentialOverwrite));
        }

        private static bool TryParse<T>(Lexer tokens, Parser<T> parse, out T syntax) where T : SyntaxTree
        {
            var parsed = parse(tokens);

            if (parsed.IsError ||
                parsed.UnparsedTokens.ToString().Trim().Length > 0)
            {
                syntax = default(T);
                return false;
            }

            syntax = parsed.Value;
            return true;
        }

        private Environment EnvironmentForExpression()
        {
            var environment = new Environment();
            foreach (var f in functions.Values)
                if (f.Name.Identifier != "Main")
                    environment.TryIncludeUniqueBinding(f);
            return environment;
        }

        private static Function WrapAsMain(Expression typedExpression, Position pos)
        {
            return new Function(pos, (NamedType)typedExpression.Type, new Name(pos, "Main"), Enumerable.Empty<Parameter>(), typedExpression);
        }

        private static object CallMain(Assembly assembly)
        {
            return assembly.GetType("Program").GetMethod("Main").Invoke(null, null);
        }

        private static InterpreterResult Error(string message)
        {
            return new InterpreterResult(new CompilerError(1, 1, message));
        }
    }
}