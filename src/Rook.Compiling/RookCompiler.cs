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
            var typeCheckedCompilationUnit = TypeCheck(compilationUnit);
            
            if (typeCheckedCompilationUnit.HasErrors)
                return new CompilerResult(Language.Rook, typeCheckedCompilationUnit.Errors);

            string translatedCode = Translate(typeCheckedCompilationUnit.Syntax);
            return csCompiler.Build(translatedCode);
        }

        private static Reply<CompilationUnit> Parse(string rookCode)
        {
            var tokens = new RookLexer().Tokenize(rookCode);
            return new RookGrammar().CompilationUnit.Parse(new TokenStream(tokens));
        }

        private static TypeChecked<CompilationUnit> TypeCheck(CompilationUnit compilationUnit)
        {
            return new TypeChecker().TypeCheck(compilationUnit);
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