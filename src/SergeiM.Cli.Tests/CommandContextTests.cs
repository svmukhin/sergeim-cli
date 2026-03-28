using SergeiM.Cli.Abstractions;
using SergeiM.Cli.Arguments;
using SergeiM.Cli.Options;
using SergeiM.Cli.Parsing;

namespace SergeiM.Cli.Tests;

[TestClass]
public class CommandContextTests
{
    private static CommandContext Build(ICommand cmd, string[] args, CancellationToken ct = default)
    {
        var result = new Parser().Parse(cmd, args);
        return new CommandContext(result, ct);
    }

    private static StubCommand Cmd(
        IReadOnlyList<IOption>? options = null,
        IReadOnlyList<IArgument>? arguments = null)
        => new("cmd", options, arguments);

    [TestMethod]
    public void GetOption_SuppliedOption_ReturnsTypedValue()
    {
        var nameOpt = new Option<string>("--name", "Name");
        var ctx = Build(Cmd([nameOpt]), ["--name", "alice"]);
        Assert.AreEqual("alice", ctx.GetOption(nameOpt));
    }

    [TestMethod]
    public void GetOption_MissingOption_ReturnsDefault()
    {
        var nameOpt = new Option<string>("--name", "Name");
        var ctx = Build(Cmd([nameOpt]), []);
        Assert.IsNull(ctx.GetOption(nameOpt));
    }

    [TestMethod]
    public void GetOption_BoolFlagPresent_ReturnsTrue()
    {
        var verboseOpt = new Option<bool>("--verbose", "Verbose");
        var ctx = Build(Cmd([verboseOpt]), ["--verbose"]);
        Assert.IsTrue(ctx.GetOption(verboseOpt));
    }

    [TestMethod]
    public void GetOption_BoolFlagAbsent_ReturnsFalse()
    {
        var verboseOpt = new Option<bool>("--verbose", "Verbose");
        var ctx = Build(Cmd([verboseOpt]), []);
        Assert.IsFalse(ctx.GetOption(verboseOpt));
    }

    [TestMethod]
    public void GetOption_WithExplicitDefault_ReturnsDefault()
    {
        var countOpt = new Option<int>("--count", "Count", false, 5);
        var ctx = Build(Cmd([countOpt]), []);
        Assert.AreEqual(5, ctx.GetOption(countOpt));
    }

    [TestMethod]
    public void GetArgument_SuppliedArgument_ReturnsTypedValue()
    {
        var fileArg = new Argument<string>("<file>", "File");
        var ctx = Build(Cmd(arguments: [fileArg]), ["report.txt"]);
        Assert.AreEqual("report.txt", ctx.GetArgument(fileArg));
    }

    [TestMethod]
    public void GetArgument_MissingOptionalArgument_ReturnsDefault()
    {
        var fileArg = new Argument<string>("<file>", "File", isRequired: false);
        var ctx = Build(Cmd(arguments: [fileArg]), []);
        Assert.IsNull(ctx.GetArgument(fileArg));
    }

    [TestMethod]
    public void GetArgument_WithExplicitDefault_ReturnsDefault()
    {
        var countArg = new Argument<int>("<n>", "N", defaultValue: 7);
        var ctx = Build(Cmd(arguments: [countArg]), []);
        Assert.AreEqual(7, ctx.GetArgument(countArg));
    }

    [TestMethod]
    public void RemainingArgs_ForwardedFromParseResult()
    {
        var cmd = Cmd();
        var ctx = Build(cmd, ["--", "extra", "args"]);
        CollectionAssert.AreEqual(new[] { "extra", "args" }, ctx.RemainingArgs);
    }

    [TestMethod]
    public void CancellationToken_ForwardedFromConstructor()
    {
        var cts = new CancellationTokenSource();
        var ctx = Build(Cmd(), [], cts.Token);
        Assert.AreEqual(cts.Token, ctx.CancellationToken);
    }

    private sealed class StubCommand : ICommand
    {
        public StubCommand(string name, IReadOnlyList<IOption>? options, IReadOnlyList<IArgument>? arguments)
        {
            Name = name;
            Options = options ?? [];
            Arguments = arguments ?? [];
        }
        public string Name { get; }
        public string Description => string.Empty;
        public IReadOnlyList<IOption> Options { get; }
        public IReadOnlyList<IArgument> Arguments { get; }
        public Task<int> ExecuteAsync(ICommandContext ctx, CancellationToken ct = default)
            => Task.FromResult(0);
    }
}
