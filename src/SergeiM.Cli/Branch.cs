using SergeiM.Cli.Abstractions;

namespace SergeiM.Cli;

/// <summary>
/// Default implementation of <see cref="IBranch"/> that groups subcommands declaratively
/// without requiring a dedicated class.
/// </summary>
public sealed class Branch : IBranch
{
    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public string Description { get; }

    /// <inheritdoc/>
    public IReadOnlyList<IOption> Options { get; }

    /// <inheritdoc/>
    public IReadOnlyList<INode> Subcommands { get; }

    /// <summary>
    /// Initializes a branch without shared options.
    /// </summary>
    /// <param name="name">The name used by the parser when traversing the tree (e.g. <c>remote</c>).</param>
    /// <param name="description">Human-readable description shown in help output.</param>
    /// <param name="subcommands">The child nodes of this branch.</param>
    public Branch(string name, string description, IReadOnlyList<INode> subcommands)
    {
        Name = name;
        Description = description;
        Options = [];
        Subcommands = subcommands;
    }

    /// <summary>
    /// Initializes a branch with shared options that are inherited by all subcommands.
    /// </summary>
    /// <param name="name">The name used by the parser when traversing the tree (e.g. <c>remote</c>).</param>
    /// <param name="description">Human-readable description shown in help output.</param>
    /// <param name="options">Options declared at this level and inherited by all subcommands.</param>
    /// <param name="subcommands">The child nodes of this branch.</param>
    public Branch(string name, string description, IReadOnlyList<IOption> options, IReadOnlyList<INode> subcommands)
    {
        Name = name;
        Description = description;
        Options = options;
        Subcommands = subcommands;
    }
}
