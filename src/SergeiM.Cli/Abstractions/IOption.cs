// SPDX-FileCopyrightText: Copyright (c) 2026 Sergei Mukhin
// SPDX-License-Identifier: MIT

namespace SergeiM.Cli.Abstractions;

/// <summary>
/// Describes an untyped named command-line option (e.g. <c>--name value</c> or <c>-n value</c>).
/// <see cref="IOption{T}"/> provides the strongly-typed variant.
/// </summary>
public interface IOption
{
    /// <summary>Gets the long name including the <c>--</c> prefix (e.g. <c>--name</c>).</summary>
    string Name { get; }

    /// <summary>
    /// Gets the optional short alias including the <c>-</c> prefix (e.g. <c>-n</c>),
    /// or <see langword="null"/> when no short alias is defined.
    /// </summary>
    string? ShortName { get; }

    /// <summary>Gets the human-readable description displayed in help output.</summary>
    string Description { get; }

    /// <summary>Gets the CLR type of the option value.</summary>
    Type ValueType { get; }

    /// <summary>Gets a value indicating whether this option must be supplied by the caller.</summary>
    bool IsRequired { get; }

    /// <summary>Gets a value indicating whether an explicit default has been defined for this option.</summary>
    bool HasDefault { get; }

    /// <summary>
    /// Gets the untyped default value used when the option is not supplied,
    /// or <see langword="null"/> when no default is defined (i.e. <see cref="HasDefault"/> is <see langword="false"/>).
    /// </summary>
    object? DefaultValue { get; }
}

/// <summary>
/// Describes a strongly-typed named command-line option.
/// </summary>
/// <typeparam name="T">The CLR type of the option value.</typeparam>
public interface IOption<T> : IOption
{
    /// <summary>
    /// Gets the typed default value used when the option is not supplied,
    /// or <c>default(T)</c> when no default is defined.
    /// </summary>
    T? Default { get; }
}
