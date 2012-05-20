using System;
using System.Collections.Generic;
using System.Text;
using Parsley;

namespace Rook.Compiling
{
    public static class Fail
    {
        public static void WithErrors(IEnumerable<CompilerError> errors)
        {
            var builder = new StringBuilder();
            builder.AppendLine();
            builder.AppendLine();

            foreach (var error in errors)
                builder.AppendLine(ErrorSummary(error));

            throw new Exception(builder.ToString());
        }

        public static void WithErrors(IEnumerable<CompilerError> errors, Position expectedPosition, string expectedMessage)
        {
            var builder = new StringBuilder();
            builder.AppendLine();
            builder.AppendLine();
            builder.AppendLine("Expected error:");
            builder.AppendLine("\t" + ErrorSummary(new CompilerError(expectedPosition, expectedMessage)));

            builder.AppendLine();
            builder.AppendLine("Actual errors:");

            bool anyError = false;
            foreach (var error in errors)
            {
                builder.AppendLine("\t" + ErrorSummary(error));
                anyError = true;
            }

            if (!anyError)
                builder.AppendLine("\t" + "None");

            throw new Exception(builder.ToString());
        }

        private static string ErrorSummary(CompilerError error)
        {
            return String.Format("{0}: {1}", error.Position, error.Message);
        }
    }
}