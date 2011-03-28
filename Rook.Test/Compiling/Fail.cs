using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace Rook.Compiling
{
    public static class Fail
    {
        public static void WithErrors(IEnumerable<CompilerError> errors)
        {
            StringBuilder builder = new StringBuilder();

            foreach (var error in errors)
                builder.AppendLine(ErrorSummary(error));

            Assert.Fail(builder.ToString());
        }

        public static void WithErrors(IEnumerable<CompilerError> errors, int line, int column, string expectedMessage)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Expected error:");
            builder.AppendLine("\t" + ErrorSummary(new CompilerError(line, column, expectedMessage)));

            builder.AppendLine();
            builder.AppendLine("Actual errors:");
            foreach (var error in errors)
                builder.AppendLine("\t" + ErrorSummary(error));

            Assert.Fail(builder.ToString());
        }

        private static string ErrorSummary(CompilerError error)
        {
            return String.Format("({0}, {1}): {2}", error.Line, error.Column, error.Message);
        }
    }
}