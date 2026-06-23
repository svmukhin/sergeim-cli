// SPDX-FileCopyrightText: Copyright (c) 2026 Sergei Mukhin
// SPDX-License-Identifier: MIT

using SergeiM.Cli.Abstractions;

namespace SergeiM.Cli.Tests;

[TestClass]
public class SyncCommandTests
{
    [TestMethod]
    public void ExecuteAsync_DelegatesToExecute_ReturnsExitCode()
    {
        var cmd = new TestSyncCommand();
        var ctx = new StubCommandContext();
        var task = cmd.ExecuteAsync(ctx);
        Assert.IsTrue(task.IsCompleted);
        Assert.AreEqual(42, task.Result);
    }

    [TestMethod]
    public void Arguments_DefaultsToEmpty()
    {
        var cmd = new TestSyncCommand();
        Assert.AreEqual(0, cmd.Arguments.Count);
    }

    private sealed class TestSyncCommand : SyncCommand
    {
        public override string Name => "test";
        public override string Description => "A test command";
        public override IReadOnlyList<IOption> Options => [];

        public override int Execute(ICommandContext ctx) => 42;
    }

    private sealed class StubCommandContext : ICommandContext
    {
        public CancellationToken CancellationToken => CancellationToken.None;
        public string[] RemainingArgs => [];
        public IReadOnlyList<ParseError> Errors => [];
        public T? GetOption<T>(IOption<T> option) => default;
        public T? GetArgument<T>(IArgument<T> argument) => default;
    }
}
