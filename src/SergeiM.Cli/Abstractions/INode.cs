// SPDX-FileCopyrightText: Copyright (c) 2026 Sergei Mukhin
// SPDX-License-Identifier: MIT

namespace SergeiM.Cli.Abstractions;

/// <summary>
/// Common base for every node in the command tree (both leaf commands and branches).
/// </summary>
public interface INode
{
    /// <summary>Gets the name used by the parser when traversing the tree (e.g. <c>remote</c>).</summary>
    string Name { get; }

    /// <summary>Gets the human-readable description displayed in help output.</summary>
    string Description { get; }

    /// <summary>
    /// Gets the options declared at this node level.
    /// Options defined on a branch are inherited by all its subcommands.
    /// </summary>
    IReadOnlyList<IOption> Options { get; }
}
