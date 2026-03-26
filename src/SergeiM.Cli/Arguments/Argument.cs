using SergeiM.Cli.Abstractions;

namespace SergeiM.Cli.Arguments;

/// <summary>
/// Immutable descriptor for a strongly-typed positional command-line argument.
/// </summary>
/// <typeparam name="T">The CLR type of the argument value.</typeparam>
public sealed class Argument<T> : IArgument<T>
{
    /// <inheritdoc/>
    public string Name { get; }

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
    /// Initializes a new positional argument.
    /// </summary>
    /// <param name="name">Display name shown in help output (e.g. <c>&lt;file&gt;</c>).</param>
    /// <param name="description">Human-readable description shown in help output.</param>
    /// <param name="isRequired">Whether the caller must supply this argument. Defaults to <see langword="true"/>.</param>
    /// <param name="defaultValue">Value used when the argument is not supplied.</param>
    public Argument(string name, string description, bool isRequired = true, T? defaultValue = default)
    {
        Name = name;
        Description = description;
        IsRequired = isRequired;
        Default = defaultValue;
    }
}
