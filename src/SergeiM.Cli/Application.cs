using SergeiM.Cli.Abstractions;
using SergeiM.Cli.Parsing;

namespace SergeiM.Cli;

/// <summary>
/// Default <see cref="IApplication"/> implementation.
/// Wraps the root node in a <see cref="WithHelp"/> decorator, parses the supplied arguments,
/// then executes the matched command or prints an error summary.
/// </summary>
public sealed class Application : IApplication
{
    private readonly WithHelp _root;
    private readonly IParser _parser;
    private readonly TextWriter _errorOutput;

    /// <summary>
    /// Initializes a new application with a custom help renderer and error output writer.
    /// </summary>
    /// <param name="root">The root node of the command tree.</param>
    /// <param name="helpRenderer">The renderer used to produce help text.</param>
    /// <param name="errorOutput">Writer for parse-error messages.</param>
    public Application(INode root, IHelpRenderer helpRenderer, TextWriter errorOutput)
    {
        _root = new WithHelp(root, helpRenderer);
        _parser = new Parser();
        _errorOutput = errorOutput;
    }

    /// <summary>
    /// Initializes a new application with a custom help renderer, writing errors to <see cref="Console.Error"/>.
    /// </summary>
    /// <param name="root">The root node of the command tree.</param>
    /// <param name="helpRenderer">The renderer used to produce help text.</param>
    public Application(INode root, IHelpRenderer helpRenderer)
        : this(root, helpRenderer, Console.Error)
    {
    }

    /// <summary>
    /// Initializes a new application using <see cref="ConsoleHelpRenderer"/> and writing errors to <see cref="Console.Error"/>.
    /// </summary>
    /// <param name="root">The root node of the command tree.</param>
    public Application(INode root)
        : this(root, new ConsoleHelpRenderer(), Console.Error)
    {
    }

    /// <inheritdoc/>
    public async Task<int> RunAsync(string[] args, CancellationToken ct = default)
    {
        ParseResult result;
        try
        {
            result = _parser.Parse(_root, args);
        }
        catch (Exception ex)
        {
            await _errorOutput.WriteLineAsync(ex.Message);
            return 1;
        }
        if (result.Errors.Count > 0)
        {
            foreach (var error in result.Errors)
                await _errorOutput.WriteLineAsync(error.Message);
            return 2;
        }
        if (result.MatchedCommand == null)
        {
            var ctx = new CommandContext(result, ct);
            return await _root.ExecuteAsync(ctx, ct);
        }
        try
        {
            var ctx = new CommandContext(result, ct);
            return await result.MatchedCommand.ExecuteAsync(ctx, ct);
        }
        catch (Exception ex)
        {
            await _errorOutput.WriteLineAsync(ex.Message);
            return 1;
        }
    }
}
