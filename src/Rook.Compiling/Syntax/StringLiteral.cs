using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Parsley;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public class StringLiteral : Expression
    {
        public StringLiteral(Position position, string quotedLiteral)
        {
            Position = position;
            QuotedLiteral = quotedLiteral;
        }

        public Position Position { get; private set; }

        public string QuotedLiteral { get; private set; }

        public string Value
        {
            get
            {
                string result = QuotedLiteral.Substring(1, QuotedLiteral.Length - 2); //Remove leading and trailing quotation marks

                result = Regex.Replace(result, @"\\u[0-9a-fA-F]{4}",
                            match => Char.ConvertFromUtf32(int.Parse(match.Value.Replace("\\u", ""), NumberStyles.HexNumber)));

                result = result
                    .Replace("\\\"", "\"")
                    .Replace("\\\\", "\\")
                    .Replace("\\n", "\n")
                    .Replace("\\r", "\r")
                    .Replace("\\t", "\t");

                return result;
            }
        }

        public DataType Type { get { return NamedType.String; } }

        public Expression WithTypes(TypeChecker visitor, Scope scope)
        {
            return visitor.TypeCheck(this, scope);
        }

        public TResult Visit<TResult>(Visitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}