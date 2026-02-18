namespace Stovetop.ConfigParser;

public enum TokenType
{
    // Keywords
    Var, // var keyword for variable declarations

    // Literals
    Identifier, // project, version, runtime, alias, etc.
    String, // quoted "string" or raw text

    // Delimiters
    OpenParen, // (
    CloseParen, // )
    OpenBrace, // { for variable interpolation
    CloseBrace, // } for variable interpolation
    Equals, // = for variable assignment
    Comma, // , for separating arguments

    // Whitespace/Structure
    Newline, // end of statement

    // End
    EOF,
}

public class Token
{
    public TokenType Type { get; }
    public string Value { get; }
    public int Line { get; }
    public int Column { get; }

    public Token(TokenType type, string value, int line, int column)
    {
        Type = type;
        Value = value;
        Line = line;
        Column = column;
    }

    public override string ToString() => $"[{Type}] '{Value}' at {Line}:{Column}";
}
