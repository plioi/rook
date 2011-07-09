using System.Text;

namespace Rook.Compiling.CodeGeneration
{
    public class CodeWriter
    {
        private int indentation;
        private readonly StringBuilder builder = new StringBuilder();

        public void Literal(string literal)
        {
            builder.Append(literal);
        }

        public void EndLine()
        {
            builder.AppendLine();
        }

        public void Indentation()
        {
            Literal(IndentationString());
        }

        public void Line(string line)
        {
            if (line == "}")
                indentation--;

            Indentation();
            Literal(line);
            EndLine();

            if (line == "{")
                indentation++;
        }

        private string IndentationString()
        {
            string result = "";

            for (int i = 0; i < indentation; i++)
                result += "    ";

            return result;
        }

        public override string ToString()
        {
            return builder.ToString();
        }
    }
}
