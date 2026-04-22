// SPDX-FileCopyrightText: Copyright (c) 2026 Sergei Mukhin
// SPDX-License-Identifier: MIT

using SergeiM.Cli.Abstractions;

namespace SergeiM.Cli;

/// <summary>
/// Decorator that checks for parse errors before delegating execution to the inner node.
/// When errors are present, writes each error message to <see cref="_errorOutput"/> and
/// returns exit code <c>2</c>. Otherwise delegates to the inner node unchanged.
/// </summary>
public sealed class WithErrorHandling : ICommand, IBranch
{
    private readonly INode _inner;
    private readonly TextWriter _errorOutput;

    /// <summary>
    /// Initializes a new <see cref="WithErrorHandling"/> decorator around <paramref name="inner"/>.
    /// </summary>
    /// <param name="inner">The node to wrap.</param>
    /// <param name="errorOutput">Writer for parse-error messages.</param>
    public WithErrorHandling(INode inner, TextWriter errorOutput)
    {
        _inner = inner;
        _errorOutput = errorOutput;
    }

    /// <inheritdoc/>
    public string Name => _inner.Name;

    /// <inheritdoc/>
    public string Description => _inner.Description;

    /// <inheritdoc/>
    public IReadOnlyList<IOption> Options => _inner.Options;

    /// <inheritdoc/>
    public IReadOnlyList<IArgument> Arguments
        => _inner is ICommand cmd ? cmd.Arguments : [];

    /// <inheritdoc/>
    public IReadOnlyList<INode> Subcommands
        => _inner is IBranch branch ? branch.Subcommands : [];

    /// <inheritdoc/>
    public async Task<int> ExecuteAsync(ICommandContext ctx, CancellationToken ct = default)
    {
        if (ctx.Errors.Count > 0)
        {
            foreach (var error in ctx.Errors)
                await _errorOutput.WriteLineAsync(error.Message);
            return 2;
        }
        if (_inner is ICommand cmd)
            return await cmd.ExecuteAsync(ctx, ct);
        return 0;
    }
}
