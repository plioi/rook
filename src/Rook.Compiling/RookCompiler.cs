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
            Reply<Program> reply = Parse(code);

            if (reply.Success)
                return Build(reply.Value);

            return new CompilerResult(new CompilerError(reply.UnparsedTokens.Position, reply.ErrorMessages.ToString()));
        }

        public CompilerResult Build(Program program)
        {
            TypeChecked<Program> typeCheckedProgram = TypeCheck(program);
            
            if (typeCheckedProgram.HasErrors)
                return new CompilerResult(typeCheckedProgram.Errors);

            string translatedCode = Translate(typeCheckedProgram.Syntax);
            return csCompiler.Build(translatedCode);
        }

        private static Reply<Program> Parse(string rookCode)
        {
            var tokens = new RookLexer().Tokenize(rookCode);
            return new RookGrammar().Program.Parse(new TokenStream(tokens));
        }

        private static TypeChecked<Program> TypeCheck(Program program)
        {
            return program.WithTypes();
        }

        private string Translate(Program typedProgram)
        {
            var code = new CodeWriter();
            WriteAction write = typedProgram.Visit(csTranslator);
            write(code);
            return code.ToString();
        }
    }
}