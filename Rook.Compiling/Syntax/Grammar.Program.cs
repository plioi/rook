using Parsley;

namespace Rook.Compiling.Syntax
{
    public sealed partial class Grammar
    {
        public static Parser<Program> Program
        {
            get
            {
                return from spaces in ZeroOrMore(WhiteSpace)
                       from position in Position
                       from functions in ZeroOrMoreTerminated(Function.TerminatedBy(EndOfLine), EndOfProgram)
                       select new Program(position, functions);
            }
        }

        private static Parser<string> EndOfProgram
        {
            get
            {
                return from spaces in ZeroOrMore(WhiteSpace)
                       from endOfInput in EndOfInput
                       select endOfInput;
            }
        }
    }
}