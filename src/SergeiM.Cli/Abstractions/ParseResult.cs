// SPDX-FileCopyrightText: Copyright (c) 2026 Sergei Mukhin
// SPDX-License-Identifier: MIT

namespace SergeiM.Cli.Abstractions;

/// <summary>
/// Holds the complete outcome of parsing a raw argument array against a command tree.
/// </summary>
/// <param name="ReachedNode">
/// The deepest node reached during tree traversal.
/// Used to render contextual help when parsing stops at a branch without a matched subcommand.
/// </param>
/// <param name="MatchedCommand">
/// The resolved leaf command, or <see langword="null"/> when parsing stopped at a branch.
/// </param>
/// <param name="OptionValues">
/// Parsed option values keyed by the declaring <see cref="IOption"/> instance.
/// Includes options defined on parent branch nodes.
/// </param>
/// <param name="ArgumentValues">
/// Parsed positional argument values keyed by the declaring <see cref="IArgument"/> instance.
/// </param>
/// <param name="RemainingArgs">Tokens not consumed by any option or argument.</param>
/// <param name="Errors">All errors discovered during parsing; empty on success.</param>
public sealed record ParseResult(
    INode ReachedNode,
    ICommand? MatchedCommand,
    IReadOnlyDictionary<IOption, object?> OptionValues,
    IReadOnlyDictionary<IArgument, object?> ArgumentValues,
    string[] RemainingArgs,
    IReadOnlyList<ParseError> Errors);
