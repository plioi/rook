namespace Parsley
{
    public class ErrorMessage
    {
        private readonly string expectation;

        public ErrorMessage(string expectation = null)
        {
            this.expectation = expectation;
        }

        public override string ToString()
        {
            return expectation == null ? "Parse error." : expectation + " expected";
        }
    }
}