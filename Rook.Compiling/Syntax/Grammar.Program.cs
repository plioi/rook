using Parsley;

namespace Rook.Compiling.Syntax
{
    public partial class Grammar
    {
        public static Parser<Program> Program
        {
            get
            {
                return from leadingWhiteSpace in Optional(Token(RookLexer.EndOfLine))
                       from functions in ZeroOrMoreTerminated(Function.TerminatedBy(EndOfLine), EndOfInput)
                       select new Program(new Position(1, 1), functions);
            }
        }
    }
}