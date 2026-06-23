// SPDX-FileCopyrightText: Copyright (c) 2026 Sergei Mukhin
// SPDX-License-Identifier: MIT

using SergeiM.Cli.Abstractions;

namespace SergeiM.Cli.Parsing;

/// <summary>
/// Compares <see cref="IOption"/> instances by <see cref="IOption.Name"/>.
/// Used as the dictionary key comparer so that different instances declaring
/// the same option name are treated as equivalent keys.
/// </summary>
internal sealed class OptionKeyComparer : IEqualityComparer<IOption>
{
    public static OptionKeyComparer Instance { get; } = new();

    public bool Equals(IOption? x, IOption? y)
        => string.Equals(x?.Name, y?.Name, StringComparison.Ordinal);

    public int GetHashCode(IOption obj)
        => obj.Name.GetHashCode(StringComparison.Ordinal);
}
