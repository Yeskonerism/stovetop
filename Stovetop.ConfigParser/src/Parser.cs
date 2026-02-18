namespace Stovetop.ConfigParser;

public class Parser
{
    private readonly List<Token> _tokens;
    private int _position;

    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
        _position = 0;
    }

    private Token Current => _position < _tokens.Count ? _tokens[_position] : _tokens[^1];

    private Token Peek(int offset = 1) =>
        _position + offset < _tokens.Count ? _tokens[_position + offset] : _tokens[^1];

    private bool IsAtEnd => Current.Type == TokenType.EOF;

    private Token Advance()
    {
        if (!IsAtEnd)
            _position++;
        return _tokens[_position - 1];
    }

    private bool Check(TokenType type) => !IsAtEnd && Current.Type == type;

    private bool Match(params TokenType[] types)
    {
        foreach (var type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }
        return false;
    }

    private Token Expect(TokenType type, string message)
    {
        if (Check(type))
            return Advance();
        throw new ParseException(
            $"{message} at line {Current.Line}, column {Current.Column}. Got {Current.Type} instead."
        );
    }

    private void SkipNewlines()
    {
        while (Match(TokenType.Newline)) { }
    }

    public ConfigFileNode Parse()
    {
        var statements = new List<AstNode>();

        SkipNewlines();

        while (!IsAtEnd)
        {
            var statement = ParseStatement();
            if (statement != null)
            {
                statements.Add(statement);
            }
            SkipNewlines();
        }

        return new ConfigFileNode(statements);
    }

    private AstNode? ParseStatement()
    {
        // Variable declaration: var name = value
        if (Check(TokenType.Var))
        {
            return ParseVariableDeclaration();
        }

        // Function call: name(args)
        if (Check(TokenType.Identifier) && Peek().Type == TokenType.OpenParen)
        {
            return ParseFunctionCall();
        }

        // Skip unknown tokens
        if (!IsAtEnd)
        {
            Advance();
        }

        return null;
    }

    private VariableDeclarationNode ParseVariableDeclaration()
    {
        var varToken = Expect(TokenType.Var, "Expected 'var'");
        var nameToken = Expect(TokenType.Identifier, "Expected variable name");
        Expect(TokenType.Equals, "Expected '=' after variable name");

        var value = ParseValue();

        return new VariableDeclarationNode(nameToken.Value, value, varToken.Line, varToken.Column);
    }

    private FunctionCallNode ParseFunctionCall()
    {
        var nameToken = Expect(TokenType.Identifier, "Expected function name");
        Expect(TokenType.OpenParen, "Expected '(' after function name");

        var arguments = new List<AstNode>();

        // Parse arguments until we hit closing paren
        while (!Check(TokenType.CloseParen) && !IsAtEnd)
        {
            var arg = ParseArgument();
            arguments.Add(arg);

            // Optional comma between arguments
            Match(TokenType.Comma);
        }

        Expect(TokenType.CloseParen, "Expected ')' after arguments");

        return new FunctionCallNode(nameToken.Value, arguments, nameToken.Line, nameToken.Column);
    }

    private AstNode ParseArgument()
    {
        int startLine = Current.Line;
        int startColumn = Current.Column;

        // If argument starts with a quoted string, return it as-is (no interpolation)
        // This preserves ${VAR} syntax for runtime variable substitution
        if (Check(TokenType.String))
        {
            var str = Current.Value;
            Advance();
            return new StringLiteralNode(str, startLine, startColumn);
        }

        // Collect all tokens until comma or closing paren
        var parts = new List<AstNode>();
        var textBuilder = new System.Text.StringBuilder();
        bool needsSpace = false;

        while (!Check(TokenType.CloseParen) && !Check(TokenType.Comma) && !IsAtEnd)
        {
            if (Check(TokenType.OpenBrace))
            {
                // Flush accumulated text (without trailing space)
                if (textBuilder.Length > 0)
                {
                    parts.Add(
                        new StringLiteralNode(textBuilder.ToString(), startLine, startColumn)
                    );
                    textBuilder.Clear();
                    needsSpace = false;
                }

                // Parse variable reference
                Advance(); // skip {
                var varName = Expect(
                    TokenType.Identifier,
                    "Expected variable name in interpolation"
                );
                Expect(TokenType.CloseBrace, "Expected '}' after variable name");
                parts.Add(new VariableReferenceNode(varName.Value, varName.Line, varName.Column));
                needsSpace = true; // next text token should have space before it
            }
            else if (Check(TokenType.String) || Check(TokenType.Identifier))
            {
                // Add space between tokens (but not before first or after variable ref without space)
                if (needsSpace && textBuilder.Length == 0 && parts.Count > 0)
                {
                    textBuilder.Append(' ');
                }
                else if (textBuilder.Length > 0)
                {
                    textBuilder.Append(' ');
                }
                textBuilder.Append(Current.Value);
                Advance();
                needsSpace = false;
            }
            else
            {
                // Skip other tokens within arguments
                Advance();
            }
        }

        // Flush remaining text
        if (textBuilder.Length > 0)
        {
            parts.Add(new StringLiteralNode(textBuilder.ToString(), startLine, startColumn));
        }

        // Return appropriate node type
        if (parts.Count == 0)
        {
            return new StringLiteralNode("", startLine, startColumn);
        }
        if (parts.Count == 1)
        {
            return parts[0];
        }
        return new InterpolatedStringNode(parts, startLine, startColumn);
    }

    private AstNode ParseValue()
    {
        // Similar to ParseArgument but for variable values (until newline)
        var parts = new List<AstNode>();
        var textBuilder = new System.Text.StringBuilder();
        int startLine = Current.Line;
        int startColumn = Current.Column;
        bool needsSpace = false;

        while (!Check(TokenType.Newline) && !IsAtEnd)
        {
            if (Check(TokenType.OpenBrace))
            {
                // Flush accumulated text
                if (textBuilder.Length > 0)
                {
                    parts.Add(
                        new StringLiteralNode(textBuilder.ToString(), startLine, startColumn)
                    );
                    textBuilder.Clear();
                    needsSpace = false;
                }

                // Parse variable reference
                Advance(); // skip {
                var varName = Expect(
                    TokenType.Identifier,
                    "Expected variable name in interpolation"
                );
                Expect(TokenType.CloseBrace, "Expected '}' after variable name");
                parts.Add(new VariableReferenceNode(varName.Value, varName.Line, varName.Column));
                needsSpace = true;
            }
            else if (Check(TokenType.String) || Check(TokenType.Identifier))
            {
                if (needsSpace && textBuilder.Length == 0 && parts.Count > 0)
                {
                    textBuilder.Append(' ');
                }
                else if (textBuilder.Length > 0)
                {
                    textBuilder.Append(' ');
                }
                textBuilder.Append(Current.Value);
                Advance();
                needsSpace = false;
            }
            else
            {
                Advance();
            }
        }

        // Flush remaining text
        if (textBuilder.Length > 0)
        {
            parts.Add(new StringLiteralNode(textBuilder.ToString(), startLine, startColumn));
        }

        if (parts.Count == 0)
        {
            return new StringLiteralNode("", startLine, startColumn);
        }
        if (parts.Count == 1)
        {
            return parts[0];
        }
        return new InterpolatedStringNode(parts, startLine, startColumn);
    }
}

public class ParseException : Exception
{
    public ParseException(string message)
        : base(message) { }
}
