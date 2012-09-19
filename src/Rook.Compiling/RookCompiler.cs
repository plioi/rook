using System.Collections.Generic;
using Parsley;
using Rook.Compiling.CodeGeneration;
using Rook.Compiling.Syntax;

namespace Rook.Compiling
{
    public class RookCompiler : Compiler
    {
        private readonly CSharpCompiler csCompiler;
        private readonly CSharpTranslator csTranslator;

        public RookCompiler(CompilerParameters parameters)
        {
            csTranslator = new CSharpTranslator();
            csCompiler = new CSharpCompiler(parameters);
        }

        public CompilerResult Build(string code)
        {
            var reply = Parse(code);

            if (reply.Success)
                return Build(reply.Value);

            return new CompilerResult(Language.Rook, new CompilerError(reply.UnparsedTokens.Position, reply.ErrorMessages.ToString()));
        }

        public CompilerResult Build(CompilationUnit compilationUnit)
        {
            var typeChecker = new TypeChecker();
            var typedCompilationUnit = typeChecker.TypeCheck(compilationUnit);

            if (typeChecker.HasErrors)
                return new CompilerResult(Language.Rook, typeChecker.Errors);

            string translatedCode = Translate(typedCompilationUnit);
            return csCompiler.Build(translatedCode);
        }

        private static Reply<CompilationUnit> Parse(string rookCode)
        {
            var tokens = rookCode.Tokenize();
            return new RookGrammar().CompilationUnit.Parse(tokens);
        }

        private string Translate(CompilationUnit typedCompilationUnit)
        {
            var code = new CodeWriter();
            WriteAction write = typedCompilationUnit.Visit(csTranslator);
            write(code);
            return code.ToString();
        }
    }
}