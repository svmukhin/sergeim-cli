namespace SergeiM.Cli.Abstractions;

/// <summary>
/// A leaf node in the command tree that performs an action when invoked.
/// </summary>
public interface ICommand : INode
{
    /// <summary>Gets the positional arguments accepted by this command.</summary>
    IReadOnlyList<IArgument> Arguments { get; }

    /// <summary>
    /// Executes the command using the supplied context.
    /// </summary>
    /// <param name="ctx">Provides type-safe access to parsed option and argument values.</param>
    /// <param name="ct">Token that can be used to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> whose result is the process exit code:
    /// <c>0</c> on success, <c>1</c> on an unhandled exception.
    /// </returns>
    Task<int> ExecuteAsync(ICommandContext ctx, CancellationToken ct = default);
}
