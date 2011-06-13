namespace Parsley
{
    public class ErrorMessage
    {
        public ErrorMessage(string expectation = null)
        {
            Expectation = expectation;
        }

        public string Expectation { get; private set; }
    }
}