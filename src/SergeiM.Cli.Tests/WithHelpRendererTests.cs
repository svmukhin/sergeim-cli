// SPDX-FileCopyrightText: Copyright (c) 2026 Sergei Mukhin
// SPDX-License-Identifier: MIT

using SergeiM.Cli.Abstractions;
using SergeiM.Cli.Options;
using SergeiM.Cli.Parsing;

namespace SergeiM.Cli.Tests;

[TestClass]
public class WithHelpRendererTests
{
    private static IOption<bool> MakeHelpOption() => new Option<bool>("--help", "-h", "Help");

    private static (int exitCode, string output) Execute(
        WithHelpRenderer sut, IOption<bool> helpOption, string[] args)
    {
        var result = new Parser().Parse(new WithHelpOption(sut, helpOption), args);
        var ctx = new CommandContext(result, CancellationToken.None);
        var prev = Console.Out;
        Console.SetOut(new StringWriter());
        var exitCode = sut.ExecuteAsync(ctx).GetAwaiter().GetResult();
        Console.SetOut(prev);
        return (exitCode, new StringWriter().ToString());
    }

    private sealed class SpyRenderer : IHelpRenderer
    {
        public INode? LastNode { get; private set; }
        public int CallCount { get; private set; }
        public void Render(INode node, TextWriter output)
        {
            LastNode = node;
            CallCount++;
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
    public void ExecuteAsync_HelpFlag_RendersInnerNode()
    {
        var inner = new StubCommand("run");
        var renderer = new SpyRenderer();
        var helpOption = MakeHelpOption();
        Execute(new WithHelpRenderer(inner, renderer, helpOption), helpOption, ["--help"]);
        Assert.AreSame(inner, renderer.LastNode);
        Assert.AreEqual(1, renderer.CallCount);
    }

    [TestMethod]
    public void ExecuteAsync_HelpFlag_ReturnsZero()
    {
        var helpOption = MakeHelpOption();
        var (exitCode, _) = Execute(new WithHelpRenderer(new StubCommand("run"), new SpyRenderer(), helpOption), helpOption, ["--help"]);
        Assert.AreEqual(0, exitCode);
    }

    [TestMethod]
    public void ExecuteAsync_ShortHelpFlag_RendersHelp()
    {
        var renderer = new SpyRenderer();
        var helpOption = MakeHelpOption();
        Execute(new WithHelpRenderer(new StubCommand("run"), renderer, helpOption), helpOption, ["-h"]);
        Assert.AreEqual(1, renderer.CallCount);
    }

    [TestMethod]
    public void ExecuteAsync_NoHelpFlag_DelegatesToInnerCommand()
    {
        var inner = new StubCommand("run");
        var helpOption = MakeHelpOption();
        Execute(new WithHelpRenderer(inner, new SpyRenderer(), helpOption), helpOption, []);
        Assert.AreEqual(1, inner.ExecuteCallCount);
    }

    [TestMethod]
    public void ExecuteAsync_NoHelpFlag_RendererNotCalled()
    {
        var renderer = new SpyRenderer();
        var helpOption = MakeHelpOption();
        Execute(new WithHelpRenderer(new StubCommand("run"), renderer, helpOption), helpOption, []);
        Assert.AreEqual(0, renderer.CallCount);
    }

    [TestMethod]
    public void ExecuteAsync_NoHelpFlag_InnerIsBranch_RendersHelp()
    {
        var renderer = new SpyRenderer();
        var helpOption = MakeHelpOption();
        Execute(new WithHelpRenderer(new StubBranch("git", [new StubCommand("add")]), renderer, helpOption), helpOption, []);
        Assert.AreEqual(1, renderer.CallCount);
    }

    [TestMethod]
    public void ExecuteAsync_NoHelpFlag_InnerIsBranch_RendersInnerBranch()
    {
        var branch = new StubBranch("git", [new StubCommand("add")]);
        var renderer = new SpyRenderer();
        var helpOption = MakeHelpOption();
        Execute(new WithHelpRenderer(branch, renderer, helpOption), helpOption, []);
        Assert.AreSame(branch, renderer.LastNode);
    }

    [TestMethod]
    public void ExecuteAsync_HelpFlag_InnerIsBranch_RendersInnerBranch()
    {
        var branch = new StubBranch("git", [new StubCommand("add")]);
        var renderer = new SpyRenderer();
        var helpOption = MakeHelpOption();
        Execute(new WithHelpRenderer(branch, renderer, helpOption), helpOption, ["--help"]);
        Assert.AreSame(branch, renderer.LastNode);
    }

    [TestMethod]
    public void Name_DelegatedToInner()
    {
        var helpOption = MakeHelpOption();
        var sut = new WithHelpRenderer(new StubCommand("deploy"), new SpyRenderer(), helpOption);
        Assert.AreEqual("deploy", sut.Name);
    }
}
