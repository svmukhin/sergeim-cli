using SergeiM.Cli.Abstractions;

namespace SergeiM.Cli.Options;

/// <summary>
/// Immutable descriptor for a strongly-typed named command-line option.
/// </summary>
/// <typeparam name="T">The CLR type of the option value.</typeparam>
public sealed class Option<T> : IOption<T>
{
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
    public T? Default { get; }

    /// <inheritdoc/>
    public object? DefaultValue => Default;

    /// <summary>
    /// Initializes a new option without a short alias.
    /// </summary>
    /// <param name="name">The long name including the <c>--</c> prefix (e.g. <c>--name</c>).</param>
    /// <param name="description">Human-readable description shown in help output.</param>
    /// <param name="isRequired">Whether the caller must supply this option.</param>
    /// <param name="defaultValue">Value used when the option is not supplied.</param>
    public Option(string name, string description, bool isRequired = false, T? defaultValue = default)
    {
        Name = name;
        ShortName = null;
        Description = description;
        IsRequired = isRequired;
        Default = defaultValue;
    }

    /// <summary>
    /// Initializes a new option with a short alias.
    /// </summary>
    /// <param name="name">The long name including the <c>--</c> prefix (e.g. <c>--name</c>).</param>
    /// <param name="shortName">The short alias including the <c>-</c> prefix (e.g. <c>-n</c>).</param>
    /// <param name="description">Human-readable description shown in help output.</param>
    /// <param name="isRequired">Whether the caller must supply this option.</param>
    /// <param name="defaultValue">Value used when the option is not supplied.</param>
    public Option(string name, string shortName, string description, bool isRequired = false, T? defaultValue = default)
    {
        Name = name;
        ShortName = shortName;
        Description = description;
        IsRequired = isRequired;
        Default = defaultValue;
    }
}
