// SPDX-FileCopyrightText: Copyright (c) 2026 Sergei Mukhin
// SPDX-License-Identifier: MIT

using SergeiM.Cli.Abstractions;

namespace SergeiM.Cli;

/// <summary>
/// Decorator that adds a <c>--help</c>/<c>-h</c> option to any node and propagates it
/// recursively to all subcommands. Intended for use during parsing only — execution is
/// delegated unchanged to the inner node.
/// </summary>
public sealed class WithHelpOption : ICommand, IBranch
{
    private readonly INode _inner;
    private readonly IOption<bool> _helpOption;

    /// <summary>
    /// Initializes a new <see cref="WithHelpOption"/> decorator around <paramref name="inner"/>.
    /// </summary>
    /// <param name="inner">The node to wrap.</param>
    /// <param name="helpOption">
    /// The shared help option instance. The same instance must be passed
    /// to <see cref="WithHelpRenderer"/> so that both use the same key in
    /// <c>ParseResult.OptionValues</c>.
    /// </param>
    public WithHelpOption(INode inner, IOption<bool> helpOption)
    {
        _inner = inner;
        _helpOption = helpOption;
    }

    /// <inheritdoc/>
    public string Name => _inner.Name;

    /// <inheritdoc/>
    public string Description => _inner.Description;

    /// <inheritdoc/>
    public IReadOnlyList<IOption> Options
    {
        get
        {
            var list = new List<IOption>(_inner.Options) { _helpOption };
            return list.AsReadOnly();
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<IArgument> Arguments
        => _inner is ICommand cmd ? cmd.Arguments : [];

    /// <inheritdoc/>
    public IReadOnlyList<INode> Subcommands
    {
        get
        {
            if (_inner is not IBranch branch)
                return [];
            var list = new List<INode>(branch.Subcommands.Count);
            foreach (var s in branch.Subcommands)
                list.Add(new WithHelpOption(s, _helpOption));
            return list.AsReadOnly();
        }
    }

    /// <inheritdoc/>
    public Task<int> ExecuteAsync(ICommandContext ctx, CancellationToken ct = default)
        => _inner is ICommand cmd ? cmd.ExecuteAsync(ctx, ct) : Task.FromResult(0);
}
