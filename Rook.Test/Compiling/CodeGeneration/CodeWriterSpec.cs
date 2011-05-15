using System.Text;
using NUnit.Framework;

namespace Rook.Compiling.CodeGeneration
{
    [TestFixture]
    public class CodeWriterSpec
    {
        private CodeWriter code;

        [SetUp]
        public void SetUp()
        {
            code = new CodeWriter();
        }

        [Test]
        public void ShouldWriteLiteralText()
        {
            code.Literal("abc");
            code.Literal("def");
            code.Literal("ghi");
            code.ToString().ShouldEqual("abcdefghi");
        }

        [Test]
        public void ShouldWriteLineEndings()
        {
            code.Literal("abc");
            code.EndLine();
            code.Literal("def");
            code.EndLine();
            code.Literal("ghi");
            code.ToString().ShouldEqual("abc\r\ndef\r\nghi");
        }

        [Test]
        public void ShouldStartWithZeroIndentation()
        {
            code.Indentation();
            code.Literal("abc");
            code.ToString().ShouldEqual("abc");
        }

        [Test]
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

            StringBuilder expected = new StringBuilder();
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
