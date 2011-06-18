using System.Collections.Generic;
using System.Text.RegularExpressions;
using Parsley;

namespace Rook.Compiling.Syntax
{
    public class RookLexer : Lexer
    {
        public static readonly TokenKind IntralineWhiteSpace = new TokenKind("IntralineWhiteSpace", @"[ \t]+");
        
        public static readonly TokenKind @int = new TokenKind("int", @"int \b");
        public static readonly TokenKind @bool = new TokenKind("bool", @"bool \b");
        public static readonly TokenKind @void = new TokenKind("void", @"void \b");
        public static readonly TokenKind @null = new TokenKind("null", @"null \b");
        public static readonly TokenKind @if = new TokenKind("if", @"if \b");
        public static readonly TokenKind @return = new TokenKind("return", @"return \b");
        public static readonly TokenKind @else = new TokenKind("else", @"else \b");
        public static readonly TokenKind @fn = new TokenKind("fn", @"fn \b");

        public static readonly TokenKind Boolean = new TokenKind("boolean", @"true \b | false \b");
        public static readonly TokenKind Integer = new TokenKind("integer", @"[0-9]+");

        public static readonly Dictionary<string, TokenKind> Operators = CreateOperators(
            "(", ")", "*", "/", "+", "-", "<=", "<", ">=", ">", "==", "!=", "||",
            "&&", "!", "=", ",", "{", "}", "[]", "[", "]", ":", "??", "?");

        public static readonly TokenKind Identifier = new TokenKind("identifier", @"[a-zA-Z]+[a-zA-Z0-9]*");
        public static readonly TokenKind EndOfLine = new TokenKind("end of line", @"(\r\n|;)\s*");

        public RookLexer(string source)
            : base(new Text(source),
            IntralineWhiteSpace,
            @int, @bool, @void, @null, @if, @return, @else, @fn,
            Boolean, Integer, Identifier,
            Operators["("], Operators[")"],
            Operators["*"], Operators["/"],
            Operators["+"], Operators["-"],
            Operators["<="], Operators["<"], Operators[">="], Operators[">"],
            Operators["=="], Operators["!="],
            Operators["||"], Operators["&&"], Operators["!"],
            Operators["="], Operators[","],
            Operators["{"], Operators["}"],
            Operators["[]"], Operators["["], Operators["]"], Operators[":"],
            Operators["??"], Operators["?"],
            EndOfLine) { }

        private static Dictionary<string, TokenKind> CreateOperators(params string[] symbols)
        {
            var result = new Dictionary<string, TokenKind>();
            foreach (var symbol in symbols)
                result[symbol] = new TokenKind(symbol, Regex.Escape(symbol));
            return result;
        }
    }
}