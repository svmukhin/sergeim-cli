using SergeiM.Cli.Abstractions;
using SergeiM.Cli.Options;

namespace SergeiM.Cli.Tests;

[TestClass]
public class BranchTests
{
    private static ICommand MakeCommand(string name) => new StubCommand(name);

    [TestMethod]
    public void Constructor_WithoutOptions_SetsNameDescriptionAndSubcommands()
    {
        var child = MakeCommand("child");
        var branch = new Branch("remote", "Manage remotes", [child]);
        Assert.AreEqual("remote", branch.Name);
        Assert.AreEqual("Manage remotes", branch.Description);
        Assert.AreEqual(0, branch.Options.Count);
        Assert.AreEqual(1, branch.Subcommands.Count);
        Assert.AreSame(child, branch.Subcommands[0]);
    }

    [TestMethod]
    public void Constructor_WithOptions_ExposesOptions()
    {
        IReadOnlyList<IOption> options = [new Option<string>("--token", "Auth token")];
        var branch = new Branch("remote", "Manage remotes", options, [MakeCommand("add")]);
        Assert.AreEqual(1, branch.Options.Count);
        Assert.AreSame(options[0], branch.Options[0]);
    }

    [TestMethod]
    public void Constructor_WithMultipleSubcommands_PreservesOrder()
    {
        var add = MakeCommand("add");
        var remove = MakeCommand("remove");
        var branch = new Branch("remote", "Manage remotes", [add, remove]);
        Assert.AreSame(add, branch.Subcommands[0]);
        Assert.AreSame(remove, branch.Subcommands[1]);
    }

    [TestMethod]
    public void Branch_ImplementsIBranch()
    {
        IBranch branch = new Branch("x", "desc", []);
        Assert.IsInstanceOfType<IBranch>(branch);
    }

    [TestMethod]
    public void Branch_ImplementsINode()
    {
        INode node = new Branch("x", "desc", []);
        Assert.IsInstanceOfType<INode>(node);
    }

    private sealed class StubCommand(string name) : ICommand
    {
        public string Name => name;
        public string Description => string.Empty;
        public IReadOnlyList<IOption> Options => [];
        public IReadOnlyList<IArgument> Arguments => [];
        public Task<int> ExecuteAsync(ICommandContext ctx, CancellationToken ct = default)
            => Task.FromResult(0);
    }
}
