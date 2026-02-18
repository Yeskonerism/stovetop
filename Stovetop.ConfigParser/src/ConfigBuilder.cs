namespace Stovetop.ConfigParser;

/// <summary>
/// Builds a ConfigModel from a parsed AST
/// </summary>
public class ConfigBuilder
{
    private readonly ConfigModel _config = new();

    // Known function names that map to config properties
    private static readonly HashSet<string> KnownFunctions = new(StringComparer.OrdinalIgnoreCase)
    {
        "project",
        "version",
        "runtime",
        "build_command",
        "run_command",
        "executable",
        "test_command",
        "clean_command",
        "deploy_command",
        "alias",
        "pre_build_hook",
        "post_build_hook",
        "pre_run_hook",
        "post_run_hook",
        "pre_deploy_hook",
        "post_deploy_hook",
        "script",
    };

    public ConfigModel Build(ConfigFileNode ast)
    {
        foreach (var statement in ast.Statements)
        {
            ProcessStatement(statement);
        }

        return _config;
    }

    private void ProcessStatement(AstNode node)
    {
        switch (node)
        {
            case VariableDeclarationNode varDecl:
                ProcessVariableDeclaration(varDecl);
                break;
            case FunctionCallNode funcCall:
                ProcessFunctionCall(funcCall);
                break;
        }
    }

    private void ProcessVariableDeclaration(VariableDeclarationNode node)
    {
        string value = EvaluateNode(node.Value);
        _config.Variables[node.Name] = value;
    }

    private void ProcessFunctionCall(FunctionCallNode node)
    {
        string name = node.Name.ToLower();

        switch (name)
        {
            case "project":
                _config.Project = GetFirstArgument(node);
                break;
            case "version":
                _config.Version = GetFirstArgument(node);
                break;
            case "runtime":
                _config.Runtime = GetFirstArgument(node);
                if (node.Arguments.Count > 1)
                {
                    _config.RuntimeVersion = EvaluateNode(node.Arguments[1]);
                }
                break;
            case "build_command":
                _config.Commands["build"] = GetFirstArgument(node);
                break;
            case "run_command":
                _config.Commands["run"] = GetFirstArgument(node);
                break;
            case "executable":
                _config.Commands["executable"] = GetFirstArgument(node);
                break;
            case "test_command":
                _config.Commands["test"] = GetFirstArgument(node);
                break;
            case "clean_command":
                _config.Commands["clean"] = GetFirstArgument(node);
                break;
            case "deploy_command":
                _config.Commands["deploy"] = GetFirstArgument(node);
                break;
            case "alias":
                if (node.Arguments.Count >= 2)
                {
                    string aliasName = EvaluateNode(node.Arguments[0]);
                    string aliasCommand = EvaluateNode(node.Arguments[1]);
                    _config.Aliases[aliasName] = aliasCommand;
                }
                break;
            case "pre_build_hook":
                _config.Hooks["pre_build"] = GetFirstArgument(node);
                break;
            case "post_build_hook":
                _config.Hooks["post_build"] = GetFirstArgument(node);
                break;
            case "pre_run_hook":
                _config.Hooks["pre_run"] = GetFirstArgument(node);
                break;
            case "post_run_hook":
                _config.Hooks["post_run"] = GetFirstArgument(node);
                break;
            case "pre_deploy_hook":
                _config.Hooks["pre_deploy"] = GetFirstArgument(node);
                break;
            case "post_deploy_hook":
                _config.Hooks["post_deploy"] = GetFirstArgument(node);
                break;
            case "script":
                if (node.Arguments.Count >= 2)
                {
                    string scriptName = EvaluateNode(node.Arguments[0]);
                    string scriptContent = EvaluateNode(node.Arguments[1]);
                    _config.Scripts[scriptName] = scriptContent;
                }
                break;
            default:
                // Unknown function - could be extended in the future
                break;
        }
    }

    private string GetFirstArgument(FunctionCallNode node)
    {
        if (node.Arguments.Count == 0)
            return "";
        return EvaluateNode(node.Arguments[0]);
    }

    private string EvaluateNode(AstNode node)
    {
        return node switch
        {
            StringLiteralNode str => str.Value,
            VariableReferenceNode varRef => _config.Variables.TryGetValue(varRef.Name, out var val)
                ? val
                : $"{{{varRef.Name}}}",
            InterpolatedStringNode interp => EvaluateInterpolatedString(interp),
            _ => "",
        };
    }

    private string EvaluateInterpolatedString(InterpolatedStringNode node)
    {
        var result = new System.Text.StringBuilder();

        foreach (var part in node.Parts)
        {
            result.Append(EvaluateNode(part));
        }

        return result.ToString();
    }
}

/// <summary>
/// Main entry point for parsing .stove config files
/// </summary>
public static class StovetopConfigParser
{
    /// <summary>
    /// Parse a .stove config file from a string
    /// </summary>
    public static ConfigModel Parse(string source)
    {
        var lexer = new Lexer(source);
        var tokens = lexer.Tokenize();

        var parser = new Parser(tokens);
        var ast = parser.Parse();

        var builder = new ConfigBuilder();
        return builder.Build(ast);
    }

    /// <summary>
    /// Parse a .stove config file from a file path
    /// </summary>
    public static ConfigModel ParseFile(string filePath)
    {
        string source = File.ReadAllText(filePath);
        return Parse(source);
    }
}
