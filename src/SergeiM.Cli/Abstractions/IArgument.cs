namespace SergeiM.Cli.Abstractions;

/// <summary>
/// Describes an untyped positional command-line argument (e.g. <c>&lt;file&gt;</c>).
/// <see cref="IArgument{T}"/> provides the strongly-typed variant.
/// </summary>
public interface IArgument
{
    /// <summary>Gets the display name shown in help output (e.g. <c>&lt;file&gt;</c>).</summary>
    string Name { get; }

    /// <summary>Gets the human-readable description displayed in help output.</summary>
    string Description { get; }

    /// <summary>Gets the CLR type of the argument value.</summary>
    Type ValueType { get; }

    /// <summary>Gets a value indicating whether this argument must be supplied by the caller.</summary>
    bool IsRequired { get; }

    /// <summary>Gets a value indicating whether an explicit default has been defined for this argument.</summary>
    bool HasDefault { get; }

    /// <summary>
    /// Gets the untyped default value used when the argument is not supplied,
    /// or <see langword="null"/> when no default is defined (i.e. <see cref="HasDefault"/> is <see langword="false"/>).
    /// </summary>
    object? DefaultValue { get; }
}

/// <summary>
/// Describes a strongly-typed positional command-line argument.
/// </summary>
/// <typeparam name="T">The CLR type of the argument value.</typeparam>
public interface IArgument<T> : IArgument
{
    /// <summary>
    /// Gets the typed default value used when the argument is not supplied,
    /// or <c>default(T)</c> when no default is defined.
    /// </summary>
    T? Default { get; }
}
