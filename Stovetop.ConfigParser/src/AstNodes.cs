namespace Stovetop.ConfigParser;

/// <summary>
/// Base class for all AST nodes
/// </summary>
public abstract class AstNode
{
    public int Line { get; }
    public int Column { get; }

    protected AstNode(int line, int column)
    {
        Line = line;
        Column = column;
    }
}

/// <summary>
/// Represents a function call like: project("My Project") or runtime(gcc)
/// </summary>
public class FunctionCallNode : AstNode
{
    public string Name { get; }
    public List<AstNode> Arguments { get; }

    public FunctionCallNode(string name, List<AstNode> arguments, int line, int column)
        : base(line, column)
    {
        Name = name;
        Arguments = arguments;
    }
}

/// <summary>
/// Represents a variable declaration: var name = value
/// </summary>
public class VariableDeclarationNode : AstNode
{
    public string Name { get; }
    public AstNode Value { get; }

    public VariableDeclarationNode(string name, AstNode value, int line, int column)
        : base(line, column)
    {
        Name = name;
        Value = value;
    }
}

/// <summary>
/// Represents a string literal (quoted or unquoted)
/// </summary>
public class StringLiteralNode : AstNode
{
    public string Value { get; }

    public StringLiteralNode(string value, int line, int column)
        : base(line, column)
    {
        Value = value;
    }
}

/// <summary>
/// Represents a variable reference: {var_name}
/// </summary>
public class VariableReferenceNode : AstNode
{
    public string Name { get; }

    public VariableReferenceNode(string name, int line, int column)
        : base(line, column)
    {
        Name = name;
    }
}

/// <summary>
/// Represents an interpolated string containing literals and variable references
/// Example: "{file_list} -I{include} -o bin/app"
/// </summary>
public class InterpolatedStringNode : AstNode
{
    public List<AstNode> Parts { get; }

    public InterpolatedStringNode(List<AstNode> parts, int line, int column)
        : base(line, column)
    {
        Parts = parts;
    }
}

/// <summary>
/// Root node containing all statements in the config file
/// </summary>
public class ConfigFileNode : AstNode
{
    public List<AstNode> Statements { get; }

    public ConfigFileNode(List<AstNode> statements)
        : base(1, 1)
    {
        Statements = statements;
    }
}
