using Parsley;

namespace Rook.Compiling.Syntax
{
    public partial class Grammar
    {
        public static Parser<Program> Program
        {
            get
            {
                return from leadingEndOfLine in Optional(Token(RookLexer.EndOfLine))
                       from functions in ZeroOrMore(Function.TerminatedBy(EndOfLine)).TerminatedBy(EndOfInput)
                       select new Program(new Position(1, 1), functions);
            }
        }
    }
}