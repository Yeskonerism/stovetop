namespace Stovetop.ConfigParser;

public class Lexer
{
    private readonly string _source;
    private int _position;
    private int _line = 1;
    private int _column = 1;

    private static readonly HashSet<string> Keywords = new() { "var" };

    public Lexer(string source)
    {
        _source = source;
        _position = 0;
    }

    private char Current => _position < _source.Length ? _source[_position] : '\0';

    private char Peek(int offset = 1) =>
        _position + offset < _source.Length ? _source[_position + offset] : '\0';

    private bool IsAtEnd => _position >= _source.Length;

    private char Advance()
    {
        char c = Current;
        _position++;
        if (c == '\n')
        {
            _line++;
            _column = 1;
        }
        else
        {
            _column++;
        }
        return c;
    }

    private void SkipWhitespace()
    {
        while (!IsAtEnd && Current is ' ' or '\t' or '\r')
        {
            Advance();
        }
    }

    private void SkipComment()
    {
        // Skip // comments until end of line
        if (Current == '/' && Peek() == '/')
        {
            while (!IsAtEnd && Current != '\n')
            {
                Advance();
            }
        }
    }

    private void SkipLineContinuation()
    {
        // Handle backslash line continuation
        if (Current == '\\' && Peek() == '\n')
        {
            Advance(); // skip backslash
            Advance(); // skip newline
        }
    }

    public List<Token> Tokenize()
    {
        var tokens = new List<Token>();

        while (!IsAtEnd)
        {
            SkipWhitespace();
            SkipLineContinuation();
            SkipComment();

            if (IsAtEnd)
                break;

            var token = NextToken();
            if (token != null)
            {
                tokens.Add(token);
            }
        }

        tokens.Add(new Token(TokenType.EOF, "", _line, _column));
        return tokens;
    }

    private Token? NextToken()
    {
        SkipWhitespace();
        if (IsAtEnd)
            return null;

        int startLine = _line;
        int startColumn = _column;

        char c = Current;

        // Single character tokens
        switch (c)
        {
            case '(':
                Advance();
                return new Token(TokenType.OpenParen, "(", startLine, startColumn);
            case ')':
                Advance();
                return new Token(TokenType.CloseParen, ")", startLine, startColumn);
            case '{':
                Advance();
                return new Token(TokenType.OpenBrace, "{", startLine, startColumn);
            case '}':
                Advance();
                return new Token(TokenType.CloseBrace, "}", startLine, startColumn);
            case '=':
                Advance();
                return new Token(TokenType.Equals, "=", startLine, startColumn);
            case ',':
                Advance();
                return new Token(TokenType.Comma, ",", startLine, startColumn);
            case '\n':
                Advance();
                return new Token(TokenType.Newline, "\\n", startLine, startColumn);
            case ';':
                Advance();
                return new Token(TokenType.Newline, ";", startLine, startColumn);
        }

        // Quoted string
        if (c == '"')
        {
            return ReadQuotedString(startLine, startColumn);
        }

        // Identifier or keyword
        if (IsIdentifierStart(c))
        {
            return ReadIdentifier(startLine, startColumn);
        }

        // Skip unknown characters
        Advance();
        return null;
    }

    private Token ReadQuotedString(int startLine, int startColumn)
    {
        Advance(); // skip opening quote
        var value = new System.Text.StringBuilder();

        while (!IsAtEnd && Current != '"')
        {
            // Handle escape sequences
            if (Current == '\\' && Peek() == '"')
            {
                Advance(); // skip backslash
                value.Append('"');
                Advance();
            }
            // Handle line continuation inside strings
            else if (Current == '\\' && Peek() == '\n')
            {
                Advance(); // skip backslash
                Advance(); // skip newline
            }
            else
            {
                value.Append(Advance());
            }
        }

        if (Current == '"')
        {
            Advance(); // skip closing quote
        }

        return new Token(TokenType.String, value.ToString(), startLine, startColumn);
    }

    private Token ReadIdentifier(int startLine, int startColumn)
    {
        var value = new System.Text.StringBuilder();

        while (!IsAtEnd && IsIdentifierChar(Current))
        {
            value.Append(Advance());
        }

        string text = value.ToString();

        // Check if it's a keyword
        if (Keywords.Contains(text.ToLower()))
        {
            return new Token(TokenType.Var, text, startLine, startColumn);
        }

        return new Token(TokenType.Identifier, text, startLine, startColumn);
    }

    private static bool IsIdentifierStart(char c) =>
        char.IsLetter(c) || c == '_' || c == '-' || char.IsDigit(c);

    private static bool IsIdentifierChar(char c) =>
        char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == '.' || c == '/';
}
