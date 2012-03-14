using System.Text;
using Should;
using Xunit;

namespace Rook.Compiling.CodeGeneration
{
    public class CodeWriterTests
    {
        private readonly CodeWriter code;

        public CodeWriterTests()
        {
            code = new CodeWriter();
        }

        [Fact]
        public void ShouldWriteLiteralText()
        {
            code.Literal("abc");
            code.Literal("def");
            code.Literal("ghi");
            code.ToString().ShouldEqual("abcdefghi");
        }

        [Fact]
        public void ShouldWriteLineEndings()
        {
            code.Literal("abc");
            code.EndLine();
            code.Literal("def");
            code.EndLine();
            code.Literal("ghi");
            code.ToString().ShouldEqual("abc\r\ndef\r\nghi");
        }

        [Fact]
        public void ShouldStartWithZeroIndentation()
        {
            code.Indentation();
            code.Literal("abc");
            code.ToString().ShouldEqual("abc");
        }

        [Fact]
        public void ShouldManageIndentationLevelFromOpeningAndClosingBraceLines()
        {
            code.Line("0 Indentation");
            code.Line("{");
            code.Line("1 Indentation");
            code.Line("1 Indentation With Interior {Braces}");
            code.Line("{");
            code.Line("2 Indentation");
            code.Indentation();
            code.Literal("Manually Indented");
            code.EndLine();
            code.Line("}");
            code.Line("}");
            code.Line("0 Indnetation");

            var expected = new StringBuilder();
            expected.AppendLine("0 Indentation");
            expected.AppendLine("{");
            expected.AppendLine("    1 Indentation");
            expected.AppendLine("    1 Indentation With Interior {Braces}");
            expected.AppendLine("    {");
            expected.AppendLine("        2 Indentation");
            expected.AppendLine("        Manually Indented");
            expected.AppendLine("    }");
            expected.AppendLine("}");
            expected.AppendLine("0 Indnetation");
        }
    }
}
