// SPDX-FileCopyrightText: Copyright (c) 2026 Sergei Mukhin
// SPDX-License-Identifier: MIT

namespace SergeiM.Cli.Abstractions;

/// <summary>
/// Entry point for a CLI application that parses arguments and dispatches to the matched command.
/// </summary>
public interface IApplication
{
    /// <summary>
    /// Parses <paramref name="args"/> and executes the matched command.
    /// </summary>
    /// <param name="args">The command-line argument array, typically <c>args</c> from <c>Program.cs</c>.</param>
    /// <param name="ct">Token that can be used to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> whose result is the process exit code:
    /// <c>0</c> on success, <c>1</c> on an unhandled exception, <c>2</c> on a parse error.
    /// </returns>
    Task<int> RunAsync(string[] args, CancellationToken ct = default);
}
