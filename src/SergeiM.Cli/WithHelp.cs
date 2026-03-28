using SergeiM.Cli.Abstractions;

namespace SergeiM.Cli;

/// <summary>
/// Decorator that wraps any <see cref="INode"/> and adds a <c>--help</c>/<c>-h</c> option.
/// Implements both <see cref="ICommand"/> and <see cref="IBranch"/> so the parser can traverse
/// it regardless of the wrapped node type. At execution time it checks whether the help flag
/// was supplied and, if so, renders help instead of delegating to the inner node.
/// </summary>
public sealed class WithHelp : ICommand, IBranch
{
    private static readonly HelpOptionDescriptor HelpOption = new();

    private readonly INode _inner;
    private readonly IHelpRenderer _renderer;

    /// <summary>
    /// Initializes a new <see cref="WithHelp"/> decorator around <paramref name="inner"/>.
    /// </summary>
    /// <param name="inner">The node to wrap. May be an <see cref="ICommand"/> or <see cref="IBranch"/>.</param>
    /// <param name="renderer">The renderer used to write help text.</param>
    public WithHelp(INode inner, IHelpRenderer renderer)
    {
        _inner = inner;
        _renderer = renderer;
    }

    /// <inheritdoc/>
    public string Name => _inner.Name;

    /// <inheritdoc/>
    public string Description => _inner.Description;

    /// <inheritdoc/>
    public IReadOnlyList<IOption> Options
    {
        get
        {
            var list = new List<IOption>(_inner.Options) { HelpOption };
            return list.AsReadOnly();
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<IArgument> Arguments
        => _inner is ICommand cmd ? cmd.Arguments : [];

    /// <inheritdoc/>
    public IReadOnlyList<INode> Subcommands
        => _inner is IBranch branch ? branch.Subcommands : [];

    /// <inheritdoc/>
    public Task<int> ExecuteAsync(ICommandContext ctx, CancellationToken ct = default)
    {
        if (ctx.GetOption(HelpOption))
        {
            _renderer.Render(_inner, Console.Out);
            return Task.FromResult(0);
        }
        if (_inner is ICommand innerCmd)
            return innerCmd.ExecuteAsync(ctx, ct);
        _renderer.Render(_inner, Console.Out);
        return Task.FromResult(0);
    }

    private sealed class HelpOptionDescriptor : IOption<bool>
    {
        public string Name => "--help";
        public string? ShortName => "-h";
        public string Description => "Show help and exit.";
        public Type ValueType => typeof(bool);
        public bool IsRequired => false;
        public bool HasDefault => true;
        public bool Default => false;
        public object? DefaultValue => false;
    }
}
