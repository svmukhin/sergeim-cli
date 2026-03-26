namespace SergeiM.Cli.Abstractions;

/// <summary>
/// Provides type-safe access to the parsed option and argument values for the matched command.
/// </summary>
public interface ICommandContext
{
    /// <summary>Gets the cancellation token originally passed to <see cref="IApplication.RunAsync"/>.</summary>
    CancellationToken CancellationToken { get; }

    /// <summary>Gets all command-line tokens that were not consumed by any option or argument.</summary>
    string[] RemainingArgs { get; }

    /// <summary>
    /// Returns the parsed value for <paramref name="option"/>,
    /// or <c>default(T)</c> when the option was not supplied.
    /// </summary>
    /// <typeparam name="T">The option value type.</typeparam>
    /// <param name="option">The option descriptor declared on the command.</param>
    T? GetOption<T>(IOption<T> option);

    /// <summary>
    /// Returns the parsed value for <paramref name="argument"/>,
    /// or <c>default(T)</c> when the argument was not supplied.
    /// </summary>
    /// <typeparam name="T">The argument value type.</typeparam>
    /// <param name="argument">The argument descriptor declared on the command.</param>
    T? GetArgument<T>(IArgument<T> argument);
}
