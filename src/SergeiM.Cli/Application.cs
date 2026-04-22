// SPDX-FileCopyrightText: Copyright (c) 2026 Sergei Mukhin
// SPDX-License-Identifier: MIT

using SergeiM.Cli.Abstractions;
using SergeiM.Cli.Options;
using SergeiM.Cli.Parsing;

namespace SergeiM.Cli;

/// <summary>
/// Default <see cref="IApplication"/> implementation.
/// Wraps the root node in a decorator chain, parses the supplied arguments,
/// then executes the matched command with help rendering and error handling.
/// </summary>
public sealed class Application : IApplication
{
    private readonly WithHelpOption _parseRoot;
    private readonly IHelpRenderer _renderer;
    private readonly IParser _parser;
    private readonly TextWriter _errorOutput;
    private readonly IOption<bool> _helpOption;

    /// <summary>
    /// Initializes a new application with a custom help renderer and error output writer.
    /// </summary>
    /// <param name="root">The root node of the command tree.</param>
    /// <param name="helpRenderer">The renderer used to produce help text.</param>
    /// <param name="errorOutput">Writer for parse-error messages.</param>
    public Application(INode root, IHelpRenderer helpRenderer, TextWriter errorOutput)
    {
        _helpOption = new HelpOptionDescriptor();
        _parseRoot = new WithHelpOption(root, _helpOption);
        _renderer = helpRenderer;
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
            result = _parser.Parse(_parseRoot, args);
        }
        catch (Exception ex)
        {
            await _errorOutput.WriteLineAsync(ex.Message);
            return 1;
        }
        INode nodeToExecute = result.MatchedCommand ?? _parseRoot;
        var executor = new WithHelpRenderer(new WithErrorHandling(nodeToExecute, _errorOutput), _renderer, _helpOption);
        try
        {
            var ctx = new CommandContext(result, ct);
            return await executor.ExecuteAsync(ctx, ct);
        }
        catch (Exception ex)
        {
            await _errorOutput.WriteLineAsync(ex.Message);
            return 1;
        }
    }
}
