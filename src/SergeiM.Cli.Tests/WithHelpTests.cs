using SergeiM.Cli.Abstractions;
using SergeiM.Cli.Arguments;
using SergeiM.Cli.Options;
using SergeiM.Cli.Parsing;

namespace SergeiM.Cli.Tests;

[TestClass]
public class WithHelpTests
{
    private static (int exitCode, string output) Execute(WithHelp node, string[] args)
    {
        var result = new Parser().Parse(node, args);
        var ctx = new CommandContext(result, CancellationToken.None);
        var sw = new StringWriter();
        var prev = Console.Out;
        Console.SetOut(sw);
        var exitCode = node.ExecuteAsync(ctx).GetAwaiter().GetResult();
        Console.SetOut(prev);
        return (exitCode, sw.ToString());
    }

    private static SpyRenderer MakeRenderer() => new();

    private sealed class SpyRenderer : IHelpRenderer
    {
        public INode? LastNode { get; private set; }
        public int CallCount { get; private set; }
        public void Render(INode node, TextWriter output)
        {
            LastNode = node;
            CallCount++;
            output.WriteLine($"HELP:{node.Name}");
        }
    }

    private sealed class StubCommand : ICommand
    {
        public int ExecuteCallCount { get; private set; }
        public string Name { get; }
        public string Description => string.Empty;
        public IReadOnlyList<IOption> Options => [];
        public IReadOnlyList<IArgument> Arguments => [];
        public StubCommand(string name) { Name = name; }
        public Task<int> ExecuteAsync(ICommandContext ctx, CancellationToken ct = default)
        {
            ExecuteCallCount++;
            return Task.FromResult(0);
        }
    }

    private sealed class StubBranch : IBranch
    {
        public string Name { get; }
        public string Description => string.Empty;
        public IReadOnlyList<IOption> Options => [];
        public IReadOnlyList<INode> Subcommands { get; }
        public StubBranch(string name, IReadOnlyList<INode> subcommands)
        {
            Name = name;
            Subcommands = subcommands;
        }
    }

    [TestMethod]
    public void Name_DelegatedToInner()
    {
        var cmd = new StubCommand("deploy");
        var sut = new WithHelp(cmd, MakeRenderer());
        Assert.AreEqual("deploy", sut.Name);
    }

    [TestMethod]
    public void Description_DelegatedToInner()
    {
        var cmd = new StubCommand("deploy");
        var sut = new WithHelp(cmd, MakeRenderer());
        Assert.AreEqual(cmd.Description, sut.Description);
    }

    [TestMethod]
    public void Options_ContainsHelpOption()
    {
        var cmd = new StubCommand("run");
        var sut = new WithHelp(cmd, MakeRenderer());
        var helpOpt = sut.Options.FirstOrDefault(o => o.Name == "--help");
        Assert.IsNotNull(helpOpt);
    }

    [TestMethod]
    public void Options_HelpOption_HasShortAlias()
    {
        var cmd = new StubCommand("run");
        var sut = new WithHelp(cmd, MakeRenderer());
        var helpOpt = sut.Options.First(o => o.Name == "--help");
        Assert.AreEqual("-h", helpOpt.ShortName);
    }

    [TestMethod]
    public void Options_InnerOptionsPreserved()
    {
        var verboseOpt = new Option<bool>("--verbose", "Verbose");
        var cmd = new StubCommandWithOptions("run", [verboseOpt]);
        var sut = new WithHelp(cmd, MakeRenderer());
        Assert.IsTrue(sut.Options.Any(o => o.Name == "--verbose"));
    }

    [TestMethod]
    public void Arguments_DelegatedToInnerCommand()
    {
        var arg = new Argument<string>("<file>", "File");
        var cmd = new StubCommandWithArgs("run", [arg]);
        var sut = new WithHelp(cmd, MakeRenderer());
        Assert.AreEqual(1, sut.Arguments.Count);
        Assert.AreSame(arg, sut.Arguments[0]);
    }

    [TestMethod]
    public void Arguments_EmptyWhenInnerIsBranch()
    {
        var branch = new StubBranch("git", [new StubCommand("add")]);
        var sut = new WithHelp(branch, MakeRenderer());
        Assert.AreEqual(0, sut.Arguments.Count);
    }

    [TestMethod]
    public void Subcommands_DelegatedToInnerBranch()
    {
        var add = new StubCommand("add");
        var branch = new StubBranch("git", [add]);
        var sut = new WithHelp(branch, MakeRenderer());
        Assert.AreEqual(1, sut.Subcommands.Count);
        Assert.AreSame(add, sut.Subcommands[0]);
    }

    [TestMethod]
    public void Subcommands_EmptyWhenInnerIsCommand()
    {
        var cmd = new StubCommand("run");
        var sut = new WithHelp(cmd, MakeRenderer());
        Assert.AreEqual(0, sut.Subcommands.Count);
    }

    [TestMethod]
    public void ExecuteAsync_HelpFlagSupplied_RendersInnerNode()
    {
        var cmd = new StubCommand("run");
        var renderer = MakeRenderer();
        var sut = new WithHelp(cmd, renderer);
        Execute(sut, ["--help"]);
        Assert.AreSame(cmd, renderer.LastNode);
        Assert.AreEqual(1, renderer.CallCount);
    }

    [TestMethod]
    public void ExecuteAsync_HelpFlagSupplied_ReturnsZero()
    {
        var cmd = new StubCommand("run");
        var (exitCode, _) = Execute(new WithHelp(cmd, MakeRenderer()), ["--help"]);
        Assert.AreEqual(0, exitCode);
    }

    [TestMethod]
    public void ExecuteAsync_ShortHelpFlagSupplied_RendersHelp()
    {
        var cmd = new StubCommand("run");
        var renderer = MakeRenderer();
        Execute(new WithHelp(cmd, renderer), ["-h"]);
        Assert.AreEqual(1, renderer.CallCount);
    }

    [TestMethod]
    public void ExecuteAsync_NoHelpFlag_InnerCommandExecuted()
    {
        var inner = new StubCommand("run");
        var sut = new WithHelp(inner, MakeRenderer());
        Execute(sut, []);
        Assert.AreEqual(1, inner.ExecuteCallCount);
    }

    [TestMethod]
    public void ExecuteAsync_NoHelpFlag_RendererNotCalled()
    {
        var inner = new StubCommand("run");
        var renderer = MakeRenderer();
        Execute(new WithHelp(inner, renderer), []);
        Assert.AreEqual(0, renderer.CallCount);
    }

    [TestMethod]
    public void ExecuteAsync_BranchWithNoSubcommand_HelpFlagNotSet_RendersHelp()
    {
        var branch = new StubBranch("git", [new StubCommand("add")]);
        var renderer = MakeRenderer();
        Execute(new WithHelp(branch, renderer), []);
        Assert.AreEqual(1, renderer.CallCount);
    }

    [TestMethod]
    public void ExecuteAsync_BranchHelpFlagSet_RendersInnerBranch()
    {
        var branch = new StubBranch("git", [new StubCommand("add")]);
        var renderer = MakeRenderer();
        var sut = new WithHelp(branch, renderer);
        Execute(sut, ["--help"]);
        Assert.AreSame(branch, renderer.LastNode);
    }

    private sealed class StubCommandWithOptions : ICommand
    {
        public string Name { get; }
        public string Description => string.Empty;
        public IReadOnlyList<IOption> Options { get; }
        public IReadOnlyList<IArgument> Arguments => [];
        public StubCommandWithOptions(string name, IReadOnlyList<IOption> options)
        {
            Name = name;
            Options = options;
        }
        public Task<int> ExecuteAsync(ICommandContext ctx, CancellationToken ct = default)
            => Task.FromResult(0);
    }

    private sealed class StubCommandWithArgs : ICommand
    {
        public string Name { get; }
        public string Description => string.Empty;
        public IReadOnlyList<IOption> Options => [];
        public IReadOnlyList<IArgument> Arguments { get; }
        public StubCommandWithArgs(string name, IReadOnlyList<IArgument> arguments)
        {
            Name = name;
            Arguments = arguments;
        }
        public Task<int> ExecuteAsync(ICommandContext ctx, CancellationToken ct = default)
            => Task.FromResult(0);
    }
}
