using Parsley;

namespace Rook.Compiling.Syntax
{
    public sealed partial class Grammar
    {
        public static Parser<Program> Program
        {
            get
            {
                return from leadingWhiteSpace in Optional(Token(RookLexer.EndOfLine))
                       from position in Position
                       from functions in ZeroOrMoreTerminated(Function.TerminatedBy(EndOfLine), EndOfInput)
                       select new Program(position, functions);
            }
        }
    }
}