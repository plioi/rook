namespace Rook.Compiling
{
    public interface Compiler
    {
        CompilerResult Build(string code);
    }
}