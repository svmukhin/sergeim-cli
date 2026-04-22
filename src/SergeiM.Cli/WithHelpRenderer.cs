// SPDX-FileCopyrightText: Copyright (c) 2026 Sergei Mukhin
// SPDX-License-Identifier: MIT

using SergeiM.Cli.Abstractions;

namespace SergeiM.Cli;

/// <summary>
/// Decorator that intercepts the <c>--help</c> flag at execution time and renders help for
/// the inner node. When the flag is not set, delegates to the inner node; for branch nodes
/// without a matched subcommand, renders help unconditionally.
/// </summary>
public sealed class WithHelpRenderer : ICommand, IBranch
{
    private readonly INode _inner;
    private readonly IHelpRenderer _renderer;
    private readonly IOption<bool> _helpOption;

    /// <summary>
    /// Initializes a new <see cref="WithHelpRenderer"/> decorator around <paramref name="inner"/>.
    /// </summary>
    /// <param name="inner">The node to wrap.</param>
    /// <param name="renderer">The renderer used to write help text.</param>
    /// <param name="helpOption">
    /// The shared help option instance — must be the same object used by <see cref="WithHelpOption"/>
    /// so that the key lookup in <c>ParseResult.OptionValues</c> succeeds.
    /// </param>
    public WithHelpRenderer(INode inner, IHelpRenderer renderer, IOption<bool> helpOption)
    {
        _inner = inner;
        _renderer = renderer;
        _helpOption = helpOption;
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
    public Task<int> ExecuteAsync(ICommandContext ctx, CancellationToken ct = default)
    {
        if (ctx.GetOption(_helpOption))
        {
            _renderer.Render(_inner, Console.Out);
            return Task.FromResult(0);
        }
        if (_inner is ICommand cmd)
            return cmd.ExecuteAsync(ctx, ct);
        _renderer.Render(_inner, Console.Out);
        return Task.FromResult(0);
    }
}
