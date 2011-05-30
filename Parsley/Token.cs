namespace Parsley
{
    public class Token
    {
        public Position Position { get; private set; }
        public string Literal { get; private set; }

        public Token(Position position, string value)
        {
            Position = position;
            Literal = value;
        }
    }
}