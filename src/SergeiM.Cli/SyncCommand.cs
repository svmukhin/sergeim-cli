// SPDX-FileCopyrightText: Copyright (c) 2026 Sergei Mukhin
// SPDX-License-Identifier: MIT

namespace SergeiM.Cli.Abstractions;

/// <summary>
/// Base class for commands that perform synchronous work.
/// Override <see cref="Execute"/> instead of <see cref="ICommand.ExecuteAsync"/>.
/// </summary>
public abstract class SyncCommand : ICommand
{
    /// <inheritdoc/>
    public abstract string Name { get; }

    /// <inheritdoc/>
    public abstract string Description { get; }

    /// <inheritdoc/>
    public abstract IReadOnlyList<IOption> Options { get; }

    /// <inheritdoc/>
    public virtual IReadOnlyList<IArgument> Arguments { get; } = [];

    /// <summary>
    /// Executes the command synchronously and returns the process exit code.
    /// </summary>
    /// <param name="ctx">Provides type-safe access to parsed option and argument values.</param>
    /// <returns>Process exit code: 0 for success, non-zero for failure.</returns>
    public abstract int Execute(ICommandContext ctx);

    /// <inheritdoc/>
    public Task<int> ExecuteAsync(ICommandContext ctx, CancellationToken ct = default)
        => Task.FromResult(Execute(ctx));
}
