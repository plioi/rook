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
        private readonly RookGrammar grammar;
        private readonly RookCompiler compiler;
        private readonly Dictionary<string, Class> classes;
        private readonly Dictionary<string, Function> functions;

        public Interpreter()
        {
            grammar = new RookGrammar();
            compiler = new RookCompiler(CompilerParameters.ForBasicEvaluation());
            classes = new Dictionary<string, Class>();
            functions = new Dictionary<string, Function>();
        }

        public bool CanParse(string code)
        {
            var tokens = new TokenStream(new RookLexer().Tokenize(code));
            Class @class;
            Function function;
            Expression expression;

            return TryParse(tokens, grammar.Class, out @class) ||
                   TryParse(tokens, grammar.Function, out function) ||
                   TryParse(tokens, grammar.Expression, out expression);
        }

        public InterpreterResult Interpret(string code)
        {
            var tokens = new TokenStream(new RookLexer().Tokenize(code));
            var pos = tokens.Position;
            
            Expression expression;
            if (TryParse(tokens, grammar.Expression, out expression))
                return InterpretExpression(expression, pos);

            Function function;
            if (TryParse(tokens, grammar.Function, out function))
                return InterpretFunction(function, pos);

            Class @class;
            if (TryParse(tokens, grammar.Class, out @class))
                return InterpretClass(@class, pos);

            return Error("Cannot evaluate this code: must be a class, function or expression.");
        }

        public string Translate()
        {
            var program = new Program(new Text("").Position, classes.Values, functions.Values);
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

            if (classes.ContainsKey(function.Name.Identifier))
                classes.Remove(function.Name.Identifier);

            return new InterpreterResult(function);
        }

        private InterpreterResult InterpretClass(Class @class, Position pos)
        {
            if (@class.Name.Identifier == "Main")
                return Error("The Main function is reserved for expression evaluation, and cannot be explicitly defined.");

            var compilerResult = compiler.Build(ProgramWithNewClass(@class, pos));
            if (compilerResult.Errors.Any())
                return new InterpreterResult(compilerResult.Errors);

            classes[@class.Name.Identifier] = @class;

            if (functions.ContainsKey(@class.Name.Identifier))
                functions.Remove(@class.Name.Identifier);

            return new InterpreterResult(@class);
        }

        private Program ProgramWithNewFunction(Function function, Position pos)
        {
            var classesExceptPotentialOverwrite = classes.Values.Where(c => c.Name.Identifier != function.Name.Identifier);
            var functionsExceptPotentialOverwrite = functions.Values.Where(f => f.Name.Identifier != function.Name.Identifier);
            return new Program(pos, classesExceptPotentialOverwrite, new[] { function }.Concat(functionsExceptPotentialOverwrite));
        }

        private Program ProgramWithNewClass(Class @class, Position pos)
        {
            var classesExceptPotentialOverwrite = classes.Values.Where(c => c.Name.Identifier != @class.Name.Identifier);
            var functionsExceptPotentialOverwrite = functions.Values.Where(f => f.Name.Identifier != @class.Name.Identifier);
            return new Program(pos, new[]{@class}.Concat(classesExceptPotentialOverwrite), functionsExceptPotentialOverwrite);
        }

        private static bool TryParse<T>(TokenStream tokens, Parser<T> parser, out T syntax) where T : SyntaxTree
        {
            var reply = parser.Parse(tokens);

            if (!reply.Success || NonWhitespaceRemains(reply))
            {
                syntax = default(T);
                return false;
            }

            syntax = reply.Value;
            return true;
        }

        private static bool NonWhitespaceRemains<T>(Reply<T> reply)
        {
            var stream = reply.UnparsedTokens;

            while (stream.Current.Kind != TokenKind.EndOfInput)
            {
                if (stream.Current.Literal.Trim().Length > 0)
                    return true;

                stream = stream.Advance();
            }

            return false;
        }

        private Environment EnvironmentForExpression()
        {
            var environment = new Environment();
            foreach (var c in classes.Values)
                if (c.Name.Identifier != "Main")
                    environment.TryIncludeUniqueBinding(c);
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
            return new InterpreterResult(new CompilerError(new Position(1, 1), message));
        }
    }
}