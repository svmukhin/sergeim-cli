// SPDX-FileCopyrightText: Copyright (c) 2026 Sergei Mukhin
// SPDX-License-Identifier: MIT

using SergeiM.Cli.Abstractions;
using SergeiM.Cli.Parsing;

namespace SergeiM.Cli.Tests;

[TestClass]
public class WithErrorHandlingTests
{
    private static ParseResult MakeResult(IReadOnlyList<ParseError> errors)
    {
        var stub = new StubCommand("cmd");
        var raw = new Parser().Parse(stub, []);
        return new ParseResult(raw.ReachedNode, raw.MatchedCommand,
            raw.OptionValues, raw.ArgumentValues, raw.RemainingArgs, errors);
    }

    private sealed class StubCommand : ICommand
    {
        public int ExecuteCallCount { get; private set; }
        public int ReturnCode { get; }
        public string Name { get; }
        public string Description => string.Empty;
        public IReadOnlyList<IOption> Options => [];
        public IReadOnlyList<IArgument> Arguments => [];
        public StubCommand(string name = "cmd", int returnCode = 0)
        {
            Name = name;
            ReturnCode = returnCode;
        }
        public Task<int> ExecuteAsync(ICommandContext ctx, CancellationToken ct = default)
        {
            ExecuteCallCount++;
            return Task.FromResult(ReturnCode);
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
    public void ExecuteAsync_NoErrors_DelegatesToInner()
    {
        var ctx = new CommandContext(MakeResult([]), CancellationToken.None);
        var sub = new StubCommand("run", returnCode: 7);
        var exitCode = new WithErrorHandling(sub, new StringWriter()).ExecuteAsync(ctx).GetAwaiter().GetResult();
        Assert.AreEqual(1, sub.ExecuteCallCount);
        Assert.AreEqual(7, exitCode);
    }

    [TestMethod]
    public void ExecuteAsync_NoErrors_WritesNothingToErrorOutput()
    {
        var ctx = new CommandContext(MakeResult([]), CancellationToken.None);
        new WithErrorHandling(new StubCommand(), new StringWriter()).ExecuteAsync(ctx).GetAwaiter().GetResult();
        Assert.AreEqual(string.Empty, new StringWriter().ToString());
    }

    [TestMethod]
    public void ExecuteAsync_WithErrors_ReturnsTwo()
    {
        var ctx = new CommandContext(MakeResult([new ParseError("oops")]), CancellationToken.None);
        var exitCode = new WithErrorHandling(new StubCommand(), new StringWriter()).ExecuteAsync(ctx).GetAwaiter().GetResult();
        Assert.AreEqual(2, exitCode);
    }

    [TestMethod]
    public void ExecuteAsync_WithErrors_WritesEachErrorMessage()
    {
        var errSw = new StringWriter();
        var ctx = new CommandContext(
            MakeResult([new ParseError("error one"), new ParseError("error two")]),
            CancellationToken.None);
        new WithErrorHandling(new StubCommand(), errSw).ExecuteAsync(ctx).GetAwaiter().GetResult();
        var output = errSw.ToString();
        StringAssert.Contains(output, "error one");
        StringAssert.Contains(output, "error two");
    }

    [TestMethod]
    public void ExecuteAsync_WithErrors_DoesNotCallInner()
    {
        var inner = new StubCommand();
        var ctx = new CommandContext(MakeResult([new ParseError("oops")]), CancellationToken.None);
        new WithErrorHandling(inner, new StringWriter()).ExecuteAsync(ctx).GetAwaiter().GetResult();
        Assert.AreEqual(0, inner.ExecuteCallCount);
    }

    [TestMethod]
    public void ExecuteAsync_WithErrors_InnerIsBranch_ReturnsTwo()
    {
        var ctx = new CommandContext(MakeResult([new ParseError("oops")]), CancellationToken.None);
        var exitCode = new WithErrorHandling(new StubBranch("git", [new StubCommand("add")]), new StringWriter()).ExecuteAsync(ctx).GetAwaiter().GetResult();
        Assert.AreEqual(2, exitCode);
    }

    [TestMethod]
    public void ExecuteAsync_NoErrors_InnerIsBranch_ReturnsZero()
    {
        var ctx = new CommandContext(MakeResult([]), CancellationToken.None);
        var exitCode = new WithErrorHandling(new StubBranch("git", [new StubCommand("add")]), new StringWriter()).ExecuteAsync(ctx).GetAwaiter().GetResult();
        Assert.AreEqual(0, exitCode);
    }

    [TestMethod]
    public void Name_DelegatedToInner()
    {
        var sut = new WithErrorHandling(new StubCommand("deploy"), new StringWriter());
        Assert.AreEqual("deploy", sut.Name);
    }
}
