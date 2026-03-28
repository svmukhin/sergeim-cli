// SPDX-FileCopyrightText: Copyright (c) 2026 Sergei Mukhin
// SPDX-License-Identifier: MIT

using SergeiM.Cli.Abstractions;
using SergeiM.Cli.Options;

namespace SergeiM.Cli.Tests;

[TestClass]
public class ApplicationTests
{
    private static (Application app, StringWriter errors) Make(INode root)
    {
        var errors = new StringWriter();
        var app = new Application(root, new ConsoleHelpRenderer(), errors);
        return (app, errors);
    }

    private static int Run(Application app, string[] args)
        => app.RunAsync(args).GetAwaiter().GetResult();

    private sealed class OkCommand : ICommand
    {
        public string Name { get; }
        public string Description => string.Empty;
        public IReadOnlyList<IOption> Options => [];
        public IReadOnlyList<IArgument> Arguments => [];
        public OkCommand(string name) { Name = name; }
        public Task<int> ExecuteAsync(ICommandContext ctx, CancellationToken ct = default)
            => Task.FromResult(0);
    }

    private sealed class FailCommand : ICommand
    {
        public string Name { get; }
        public string Description => string.Empty;
        public IReadOnlyList<IOption> Options => [];
        public IReadOnlyList<IArgument> Arguments => [];
        public FailCommand(string name) { Name = name; }
        public Task<int> ExecuteAsync(ICommandContext ctx, CancellationToken ct = default)
            => throw new InvalidOperationException("boom");
    }

    private sealed class ExitCodeCommand : ICommand
    {
        private readonly int _code;
        public string Name { get; }
        public string Description => string.Empty;
        public IReadOnlyList<IOption> Options => [];
        public IReadOnlyList<IArgument> Arguments => [];
        public ExitCodeCommand(string name, int code) { Name = name; _code = code; }
        public Task<int> ExecuteAsync(ICommandContext ctx, CancellationToken ct = default)
            => Task.FromResult(_code);
    }

    private sealed class SpyRenderer : IHelpRenderer
    {
        public int CallCount { get; private set; }
        public void Render(INode node, TextWriter output) { CallCount++; }
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
    public void RunAsync_SuccessfulCommand_ReturnsZero()
    {
        var (app, _) = Make(new OkCommand("run"));
        Assert.AreEqual(0, Run(app, []));
    }

    [TestMethod]
    public void RunAsync_CommandReturnsNonZeroCode_ThatCodeIsReturned()
    {
        var (app, _) = Make(new ExitCodeCommand("run", 42));
        Assert.AreEqual(42, Run(app, []));
    }

    [TestMethod]
    public void RunAsync_CommandThrows_ReturnsOne()
    {
        var (app, _) = Make(new FailCommand("run"));
        Assert.AreEqual(1, Run(app, []));
    }

    [TestMethod]
    public void RunAsync_CommandThrows_ErrorWrittenToErrorOutput()
    {
        var (app, errors) = Make(new FailCommand("run"));
        Run(app, []);
        StringAssert.Contains(errors.ToString(), "boom");
    }

    [TestMethod]
    public void RunAsync_RequiredOptionMissing_ReturnsTwo()
    {
        var cmd = new RequiredOptionCommand("run");
        var (app, _) = Make(cmd);
        Assert.AreEqual(2, Run(app, []));
    }

    [TestMethod]
    public void RunAsync_RequiredOptionMissing_ErrorWrittenToErrorOutput()
    {
        var cmd = new RequiredOptionCommand("run");
        var (app, errors) = Make(cmd);
        Run(app, []);
        Assert.IsFalse(string.IsNullOrWhiteSpace(errors.ToString()));
    }

    [TestMethod]
    public void RunAsync_HelpFlag_ReturnsZero()
    {
        var renderer = new SpyRenderer();
        var errors = new StringWriter();
        var app = new Application(new OkCommand("run"), renderer, errors);
        var prev = Console.Out;
        Console.SetOut(new StringWriter());
        var code = app.RunAsync(["--help"]).GetAwaiter().GetResult();
        Console.SetOut(prev);
        Assert.AreEqual(0, code);
    }

    [TestMethod]
    public void RunAsync_HelpFlag_RendererCalled()
    {
        var renderer = new SpyRenderer();
        var errors = new StringWriter();
        var app = new Application(new OkCommand("run"), renderer, errors);
        var prev = Console.Out;
        Console.SetOut(new StringWriter());
        app.RunAsync(["--help"]).GetAwaiter().GetResult();
        Console.SetOut(prev);
        Assert.AreEqual(1, renderer.CallCount);
    }

    [TestMethod]
    public void RunAsync_BranchNoSubcommand_ReturnsZero()
    {
        var renderer = new SpyRenderer();
        var errors = new StringWriter();
        var branch = new StubBranch("git", [new OkCommand("add")]);
        var app = new Application(branch, renderer, errors);
        var prev = Console.Out;
        Console.SetOut(new StringWriter());
        var code = app.RunAsync([]).GetAwaiter().GetResult();
        Console.SetOut(prev);
        Assert.AreEqual(0, code);
    }

    [TestMethod]
    public void RunAsync_CancellationTokenForwardedToCommand()
    {
        var cts = new CancellationTokenSource();
        var spy = new TokenSpyCommand("run");
        var (app, _) = Make(spy);
        app.RunAsync([], cts.Token).GetAwaiter().GetResult();
        Assert.AreEqual(cts.Token, spy.ReceivedToken);
    }

    private sealed class RequiredOptionCommand : ICommand
    {
        public string Name { get; }
        public string Description => string.Empty;
        public IReadOnlyList<IOption> Options { get; }
        public IReadOnlyList<IArgument> Arguments => [];
        public RequiredOptionCommand(string name)
        {
            Name = name;
            Options = [new Option<string>("--env", "Environment", isRequired: true)];
        }
        public Task<int> ExecuteAsync(ICommandContext ctx, CancellationToken ct = default)
            => Task.FromResult(0);
    }

    private sealed class TokenSpyCommand : ICommand
    {
        public CancellationToken ReceivedToken { get; private set; }
        public string Name { get; }
        public string Description => string.Empty;
        public IReadOnlyList<IOption> Options => [];
        public IReadOnlyList<IArgument> Arguments => [];
        public TokenSpyCommand(string name) { Name = name; }
        public Task<int> ExecuteAsync(ICommandContext ctx, CancellationToken ct = default)
        {
            ReceivedToken = ct;
            return Task.FromResult(0);
        }
    }
}
