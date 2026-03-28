// SPDX-FileCopyrightText: Copyright (c) 2026 Sergei Mukhin
// SPDX-License-Identifier: MIT

namespace SergeiM.Cli.Parsing;

/// <summary>Classifies a raw command-line argument token.</summary>
public enum TokenKind
{
    /// <summary>A long option flag, e.g. <c>--name</c>.</summary>
    LongOption,

    /// <summary>A short option flag, e.g. <c>-n</c>.</summary>
    ShortOption,

    /// <summary>
    /// The bare <c>--</c> separator. All tokens after this are treated as
    /// <see cref="Value"/> regardless of their form.
    /// </summary>
    EndOfOptions,

    /// <summary>
    /// A plain value: a subcommand name, a positional argument, or the value
    /// following an option flag.
    /// </summary>
    Value,
}

/// <summary>A single tokenized command-line argument.</summary>
/// <param name="Kind">The classification of this token.</param>
/// <param name="Raw">The raw string as supplied on the command line.</param>
public sealed record Token(TokenKind Kind, string Raw);
