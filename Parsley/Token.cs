namespace Parsley
{
    public class Token
    {
        public object Kind { get; private set; }
        public Position Position { get; private set; }
        public string Literal { get; private set; }

        public Token(object kind, Position position, string value)
        {
            Kind = kind;
            Position = position;
            Literal = value;
        }
    }
}