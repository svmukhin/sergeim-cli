// SPDX-FileCopyrightText: Copyright (c) 2026 Sergei Mukhin
// SPDX-License-Identifier: MIT

using SergeiM.Cli.Abstractions;

namespace SergeiM.Cli.Parsing;

/// <summary>
/// Compares <see cref="IArgument"/> instances by <see cref="IArgument.Name"/>.
/// Used as the dictionary key comparer so that different instances declaring
/// the same argument name are treated as equivalent keys.
/// </summary>
internal sealed class ArgumentKeyComparer : IEqualityComparer<IArgument>
{
    public static ArgumentKeyComparer Instance { get; } = new();

    public bool Equals(IArgument? x, IArgument? y)
        => string.Equals(x?.Name, y?.Name, StringComparison.Ordinal);

    public int GetHashCode(IArgument obj)
        => obj.Name.GetHashCode(StringComparison.Ordinal);
}
