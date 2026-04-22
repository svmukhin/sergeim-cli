// SPDX-FileCopyrightText: Copyright (c) 2026 Sergei Mukhin
// SPDX-License-Identifier: MIT

using SergeiM.Cli.Abstractions;
using SergeiM.Cli.Arguments;
using SergeiM.Cli.Options;
using SergeiM.Cli.Parsing;

namespace SergeiM.Cli.Tests;

[TestClass]
public class WithHelpOptionTests
{
    private static IOption<bool> MakeHelpOption() => new Option<bool>("--help", "-h", "Help");

    private sealed class StubCommand : ICommand
    {
        public int ExecuteCallCount { get; private set; }
        public string Name { get; }
        public string Description => string.Empty;
        public IReadOnlyList<IOption> Options { get; }
        public IReadOnlyList<IArgument> Arguments { get; }
        public StubCommand(string name, IReadOnlyList<IOption>? options = null, IReadOnlyList<IArgument>? arguments = null)
        {
            Name = name;
            Options = options ?? [];
            Arguments = arguments ?? [];
        }
        public Task<int> ExecuteAsync(ICommandContext ctx, CancellationToken ct = default)
        {
            ExecuteCallCount++;
            return Task.FromResult(42);
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
        var sut = new WithHelpOption(new StubCommand("deploy"), MakeHelpOption());
        Assert.AreEqual("deploy", sut.Name);
    }

    [TestMethod]
    public void Description_DelegatedToInner()
    {
        var inner = new StubCommand("deploy");
        var sut = new WithHelpOption(inner, MakeHelpOption());
        Assert.AreEqual(inner.Description, sut.Description);
    }

    [TestMethod]
    public void Options_ContainsHelpOption()
    {
        var helpOption = MakeHelpOption();
        var sut = new WithHelpOption(new StubCommand("run"), helpOption);
        Assert.IsTrue(sut.Options.Contains(helpOption));
    }

    [TestMethod]
    public void Options_PreservesInnerOptions()
    {
        var verboseOpt = new Option<bool>("--verbose", "Verbose");
        var helpOption = MakeHelpOption();
        var sut = new WithHelpOption(new StubCommand("run", [verboseOpt]), helpOption);
        Assert.IsTrue(sut.Options.Any(o => o.Name == "--verbose"));
    }

    [TestMethod]
    public void Arguments_DelegatedToInnerCommand()
    {
        var arg = new Argument<string>("<file>", "File");
        var sut = new WithHelpOption(new StubCommand("run", arguments: [arg]), MakeHelpOption());
        Assert.AreEqual(1, sut.Arguments.Count);
        Assert.AreSame(arg, sut.Arguments[0]);
    }

    [TestMethod]
    public void Arguments_EmptyWhenInnerIsBranch()
    {
        var sut = new WithHelpOption(new StubBranch("git", []), MakeHelpOption());
        Assert.AreEqual(0, sut.Arguments.Count);
    }

    [TestMethod]
    public void Subcommands_EmptyWhenInnerIsCommand()
    {
        var sut = new WithHelpOption(new StubCommand("run"), MakeHelpOption());
        Assert.AreEqual(0, sut.Subcommands.Count);
    }

    [TestMethod]
    public void Subcommands_WrapsEachSubcommandInWithHelpOption()
    {
        var sut = new WithHelpOption(new StubBranch("git", [(new StubCommand("add"))]), MakeHelpOption());
        Assert.AreEqual(1, sut.Subcommands.Count);
        Assert.IsInstanceOfType<WithHelpOption>(sut.Subcommands[0]);
    }

    [TestMethod]
    public void Subcommands_WrappedSubcommandHasSameHelpOption()
    {
        var helpOption = MakeHelpOption();
        var wrappedSub = (WithHelpOption)new WithHelpOption(new StubBranch("git", [(new StubCommand("add"))]), helpOption).Subcommands[0];
        Assert.IsTrue(wrappedSub.Options.Contains(helpOption));
    }

    [TestMethod]
    public void Subcommands_EmptyWhenInnerBranchHasNoSubcommands()
    {
        var sut = new WithHelpOption(new StubBranch("git", []), MakeHelpOption());
        Assert.AreEqual(0, sut.Subcommands.Count);
    }

    [TestMethod]
    public void ExecuteAsync_DelegatestoInnerCommand()
    {
        var inner = new StubCommand("run");
        var sut = new WithHelpOption(inner, MakeHelpOption());
        var result = new Parser().Parse(sut, []);
        var ctx = new CommandContext(result, CancellationToken.None);
        var exitCode = sut.ExecuteAsync(ctx).GetAwaiter().GetResult();
        Assert.AreEqual(1, inner.ExecuteCallCount);
        Assert.AreEqual(42, exitCode);
    }

    [TestMethod]
    public void ExecuteAsync_ReturnsZeroWhenInnerIsBranch()
    {
        var sut = new WithHelpOption(new StubBranch("git", [new StubCommand("add")]), MakeHelpOption());
        var result = new Parser().Parse(sut, []);
        var ctx = new CommandContext(result, CancellationToken.None);
        var exitCode = sut.ExecuteAsync(ctx).GetAwaiter().GetResult();
        Assert.AreEqual(0, exitCode);
    }

    [TestMethod]
    public void Parser_RecognizesHelpOption_NoUnknownOptionError()
    {
        var sut = new WithHelpOption(new StubCommand("run"), MakeHelpOption());
        var result = new Parser().Parse(sut, ["--help"]);
        Assert.AreEqual(0, result.Errors.Count);
    }

    [TestMethod]
    public void Parser_RecognizesHelpOptionOnSubcommand_NoUnknownOptionError()
    {
        var sut = new WithHelpOption(new StubBranch("git", [(new StubCommand("add"))]), MakeHelpOption());
        var result = new Parser().Parse(sut, ["add", "--help"]);
        Assert.AreEqual(0, result.Errors.Count);
    }
}
