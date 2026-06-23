// SPDX-FileCopyrightText: Copyright (c) 2026 Sergei Mukhin
// SPDX-License-Identifier: MIT

using SergeiM.Cli.Abstractions;

namespace SergeiM.Cli.Options;

/// <summary>
/// Immutable descriptor for a strongly-typed named command-line option.
/// </summary>
/// <typeparam name="T">The CLR type of the option value.</typeparam>
public sealed class Option<T> : IOption<T>
{
    private readonly bool _hasDefault;
    private readonly Func<T?>? _defaultFactory;

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public string? ShortName { get; }

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

    /// <inheritdoc/>
    public bool HasDefaultFactory => _defaultFactory != null;

    /// <inheritdoc/>
    public Func<object?>? DefaultFactory => _defaultFactory is not null ? () => _defaultFactory() : null;

    /// <inheritdoc/>
    public Func<T?>? TypedDefaultFactory => _defaultFactory;

    /// <summary>
    /// Initializes a new option without a short alias and without a default value.
    /// </summary>
    /// <param name="name">The long name including the <c>--</c> prefix (e.g. <c>--name</c>).</param>
    /// <param name="description">Human-readable description shown in help output.</param>
    /// <param name="isRequired">Whether the caller must supply this option.</param>
    public Option(string name, string description, bool isRequired = false)
    {
        Name = name;
        ShortName = null;
        Description = description;
        IsRequired = isRequired;
        Default = default;
        _hasDefault = false;
        _defaultFactory = null;
    }

    /// <summary>
    /// Initializes a new option without a short alias but with an explicit default value.
    /// </summary>
    /// <param name="name">The long name including the <c>--</c> prefix (e.g. <c>--name</c>).</param>
    /// <param name="description">Human-readable description shown in help output.</param>
    /// <param name="isRequired">Whether the caller must supply this option.</param>
    /// <param name="defaultValue">Value used when the option is not supplied.</param>
    public Option(string name, string description, bool isRequired, T defaultValue)
    {
        Name = name;
        ShortName = null;
        Description = description;
        IsRequired = isRequired;
        Default = defaultValue;
        _hasDefault = true;
        _defaultFactory = null;
    }

    /// <summary>
    /// Initializes a new option with a short alias and without a default value.
    /// </summary>
    /// <param name="name">The long name including the <c>--</c> prefix (e.g. <c>--name</c>).</param>
    /// <param name="shortName">The short alias including the <c>-</c> prefix (e.g. <c>-n</c>).</param>
    /// <param name="description">Human-readable description shown in help output.</param>
    /// <param name="isRequired">Whether the caller must supply this option.</param>
    public Option(string name, string shortName, string description, bool isRequired = false)
    {
        Name = name;
        ShortName = shortName;
        Description = description;
        IsRequired = isRequired;
        Default = default;
        _hasDefault = false;
        _defaultFactory = null;
    }

    /// <summary>
    /// Initializes a new option with a short alias and an explicit default value.
    /// </summary>
    /// <param name="name">The long name including the <c>--</c> prefix (e.g. <c>--name</c>).</param>
    /// <param name="shortName">The short alias including the <c>-</c> prefix (e.g. <c>-n</c>).</param>
    /// <param name="description">Human-readable description shown in help output.</param>
    /// <param name="isRequired">Whether the caller must supply this option.</param>
    /// <param name="defaultValue">Value used when the option is not supplied.</param>
    public Option(string name, string shortName, string description, bool isRequired, T defaultValue)
    {
        Name = name;
        ShortName = shortName;
        Description = description;
        IsRequired = isRequired;
        Default = defaultValue;
        _hasDefault = true;
        _defaultFactory = null;
    }

    /// <summary>
    /// Initializes a new option without a short alias but with a factory callback for its default value.
    /// </summary>
    /// <param name="name">The long name including the <c>--</c> prefix (e.g. <c>--name</c>).</param>
    /// <param name="description">Human-readable description shown in help output.</param>
    /// <param name="isRequired">Whether the caller must supply this option.</param>
    /// <param name="defaultFactory">Callback invoked to produce a default value when the option is not supplied.</param>
    public Option(string name, string description, bool isRequired, Func<T?> defaultFactory)
    {
        Name = name;
        ShortName = null;
        Description = description;
        IsRequired = isRequired;
        Default = default;
        _hasDefault = false;
        _defaultFactory = defaultFactory;
    }

    /// <summary>
    /// Initializes a new option with a short alias and a factory callback for its default value.
    /// </summary>
    /// <param name="name">The long name including the <c>--</c> prefix (e.g. <c>--name</c>).</param>
    /// <param name="shortName">The short alias including the <c>-</c> prefix (e.g. <c>-n</c>).</param>
    /// <param name="description">Human-readable description shown in help output.</param>
    /// <param name="isRequired">Whether the caller must supply this option.</param>
    /// <param name="defaultFactory">Callback invoked to produce a default value when the option is not supplied.</param>
    public Option(string name, string shortName, string description, bool isRequired, Func<T?> defaultFactory)
    {
        Name = name;
        ShortName = shortName;
        Description = description;
        IsRequired = isRequired;
        Default = default;
        _hasDefault = false;
        _defaultFactory = defaultFactory;
    }
}
