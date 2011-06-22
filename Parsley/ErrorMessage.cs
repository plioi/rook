namespace Parsley
{
    public class ErrorMessage
    {
        public static ErrorMessage Unknown()
        {
            return new ErrorMessage(null);
        }

        public static ErrorMessage Expected(string expectation)
        {
            return new ErrorMessage(expectation);
        }

        private ErrorMessage(string expectation)
        {
            Expectation = expectation;
        }

        public string Expectation { get; private set; }
    }
}