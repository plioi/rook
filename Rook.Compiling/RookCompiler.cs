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
            Parsed<Program> parsedProgram = Parse(code);
            
            if (parsedProgram.IsError)
                return new CompilerResult(new CompilerError(parsedProgram.UnparsedText.Position, parsedProgram.Message));

            return Build(parsedProgram.Value);
        }

        public CompilerResult Build(Program program)
        {
            TypeChecked<Program> typeCheckedProgram = TypeCheck(program);
            
            if (typeCheckedProgram.HasErrors)
                return new CompilerResult(typeCheckedProgram.Errors);

            string translatedCode = Translate(typeCheckedProgram.Syntax);
            return csCompiler.Build(translatedCode);
        }

        private static Parsed<Program> Parse(string rookCode)
        {
            return Grammar.Program(new Text(rookCode));
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