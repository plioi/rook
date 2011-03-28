namespace Parsley
{
    public class Position
    {
        public int Line { get; private set; }
        public int Column { get; private set; }

        public Position(Text text)
        {
            Line = text.Line;
            Column = text.Column;
        }
    }
}
