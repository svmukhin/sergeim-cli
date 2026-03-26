namespace SergeiM.Cli.Parsing;

/// <summary>
/// Converts a raw argument array into a sequence of <see cref="Token"/> values.
/// </summary>
public sealed class Tokenizer
{
    /// <summary>
    /// Tokenizes <paramref name="args"/> according to the following rules:
    /// <list type="bullet">
    ///   <item><description><c>--</c> (bare) → <see cref="TokenKind.EndOfOptions"/>; all subsequent tokens become <see cref="TokenKind.Value"/>.</description></item>
    ///   <item><description>Starts with <c>--</c> → <see cref="TokenKind.LongOption"/>.</description></item>
    ///   <item><description>Starts with <c>-</c> and exactly two characters → <see cref="TokenKind.ShortOption"/>.</description></item>
    ///   <item><description>Everything else → <see cref="TokenKind.Value"/>.</description></item>
    /// </list>
    /// </summary>
    /// <param name="args">The raw command-line argument array.</param>
    /// <returns>An array of tokens in the same order as <paramref name="args"/>.</returns>
    public Token[] Tokenize(string[] args)
    {
        var tokens = new List<Token>(args.Length);
        var endOfOptions = false;
        foreach (var arg in args)
        {
            if (endOfOptions)
            {
                tokens.Add(new Token(TokenKind.Value, arg));
                continue;
            }
            if (arg == "--")
            {
                tokens.Add(new Token(TokenKind.EndOfOptions, arg));
                endOfOptions = true;
                continue;
            }
            if (arg.StartsWith("--", StringComparison.Ordinal))
            {
                tokens.Add(new Token(TokenKind.LongOption, arg));
                continue;
            }
            if (arg.Length == 2 && arg[0] == '-')
            {
                tokens.Add(new Token(TokenKind.ShortOption, arg));
                continue;
            }
            tokens.Add(new Token(TokenKind.Value, arg));
        }
        return [.. tokens];
    }
}
