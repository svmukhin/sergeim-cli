namespace SergeiM.Cli.Abstractions;

/// <summary>
/// Parses a raw argument array against a command tree and returns a <see cref="ParseResult"/>.
/// </summary>
public interface IParser
{
    /// <summary>
    /// Traverses the command tree rooted at <paramref name="root"/> and matches the
    /// supplied <paramref name="args"/> to the appropriate command, options, and arguments.
    /// </summary>
    /// <param name="root">The root node of the command tree.</param>
    /// <param name="args">The raw command-line argument array.</param>
    /// <returns>A <see cref="ParseResult"/> describing the match result and any parse errors.</returns>
    ParseResult Parse(INode root, string[] args);
}
