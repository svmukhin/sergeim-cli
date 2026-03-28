// SPDX-FileCopyrightText: Copyright (c) 2026 Sergei Mukhin
// SPDX-License-Identifier: MIT

using SergeiM.Cli.Abstractions;

namespace SergeiM.Cli.Arguments;

/// <summary>
/// Immutable descriptor for a strongly-typed positional command-line argument.
/// </summary>
/// <typeparam name="T">The CLR type of the argument value.</typeparam>
public sealed class Argument<T> : IArgument<T>
{
    private readonly bool _hasDefault;

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public string Description { get; }

    /// <inheritdoc/>
    public Type ValueType => typeof(T);

    /// <inheritdoc/>
    public bool IsRequired { get; }

    /// <inheritdoc/>
    public bool HasDefault => _hasDefault;

    /// <inheritdoc/>
    public T? Default { get; }

    /// <inheritdoc/>
    public object? DefaultValue => _hasDefault ? (object?)Default : null;

    /// <summary>
    /// Initializes a new positional argument without a default value.
    /// </summary>
    /// <param name="name">Display name shown in help output (e.g. <c>&lt;file&gt;</c>).</param>
    /// <param name="description">Human-readable description shown in help output.</param>
    /// <param name="isRequired">Whether the caller must supply this argument. Defaults to <see langword="true"/>.</param>
    public Argument(string name, string description, bool isRequired = true)
    {
        Name = name;
        Description = description;
        IsRequired = isRequired;
        Default = default;
        _hasDefault = false;
    }

    /// <summary>
    /// Initializes a new optional positional argument with an explicit default value.
    /// </summary>
    /// <param name="name">Display name shown in help output (e.g. <c>&lt;file&gt;</c>).</param>
    /// <param name="description">Human-readable description shown in help output.</param>
    /// <param name="defaultValue">Value used when the argument is not supplied.</param>
    public Argument(string name, string description, T defaultValue)
    {
        Name = name;
        Description = description;
        IsRequired = false;
        Default = defaultValue;
        _hasDefault = true;
    }
}
