namespace Parsley
{
    public abstract class ErrorMessage
    {
        public static ErrorMessage Unknown()
        {
            return new UnknownErrorMessage();
        }

        public static ErrorMessage Expected(string expectation)
        {
            return new ExpectedErrorMessage(expectation);
        }
    }

    public class UnknownErrorMessage : ErrorMessage
    {
        public override string ToString()
        {
            return "Parse error.";
        }
    }

    public class ExpectedErrorMessage : ErrorMessage
    {
        public ExpectedErrorMessage(string expectation)
        {
            Expectation = expectation;
        }

        public string Expectation { get; private set; }

        public override string ToString()
        {
            return Expectation + " expected";
        }
    }
}