// SPDX-FileCopyrightText: Copyright (c) 2026 Sergei Mukhin
// SPDX-License-Identifier: MIT

namespace SergeiM.Cli.Abstractions;

/// <summary>
/// A branch node in the command tree that groups related subcommands.
/// Options declared on a branch are inherited by all of its subcommands.
/// </summary>
public interface IBranch : INode
{
    /// <summary>
    /// Gets the child nodes of this branch.
    /// Each element is either an <see cref="ICommand"/> or a nested <see cref="IBranch"/>.
    /// </summary>
    IReadOnlyList<INode> Subcommands { get; }
}
