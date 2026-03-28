using SergeiM.Cli.Abstractions;

namespace SergeiM.Cli;

/// <summary>
/// Default implementation of <see cref="ICommandContext"/> backed by a <see cref="ParseResult"/>.
/// </summary>
public sealed class CommandContext : ICommandContext
{
    private readonly ParseResult _result;

    /// <summary>
    /// Initializes a new <see cref="CommandContext"/> from an already-parsed result.
    /// </summary>
    /// <param name="result">The parse outcome containing option and argument values.</param>
    /// <param name="cancellationToken">Token forwarded from <see cref="IApplication.RunAsync"/>.</param>
    public CommandContext(ParseResult result, CancellationToken cancellationToken)
    {
        _result = result;
        CancellationToken = cancellationToken;
    }

    /// <inheritdoc/>
    public CancellationToken CancellationToken { get; }

    /// <inheritdoc/>
    public string[] RemainingArgs => _result.RemainingArgs;

    /// <inheritdoc/>
    public T? GetOption<T>(IOption<T> option)
    {
        if (_result.OptionValues.TryGetValue(option, out var raw))
            return (T?)raw;
        return default;
    }

    /// <inheritdoc/>
    public T? GetArgument<T>(IArgument<T> argument)
    {
        if (_result.ArgumentValues.TryGetValue(argument, out var raw))
            return (T?)raw;
        return default;
    }
}
