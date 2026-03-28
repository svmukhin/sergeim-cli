// SPDX-FileCopyrightText: Copyright (c) 2026 Sergei Mukhin
// SPDX-License-Identifier: MIT

using SergeiM.Cli.Abstractions;
using SergeiM.Cli.Arguments;
using SergeiM.Cli.Options;
using SergeiM.Cli.Parsing;

namespace SergeiM.Cli.Tests.Parsing;

[TestClass]
public class ParserTests
{
    private static Parser Parser => new();

    private static StubCommand Cmd(string name,
        IReadOnlyList<IOption>? options = null,
        IReadOnlyList<IArgument>? arguments = null)
        => new(name, options, arguments);

    [TestMethod]
    public void Parse_FlatCommand_NoArgs_MatchesCommand()
    {
        var cmd = Cmd("greet");
        var result = Parser.Parse(cmd, []);
        Assert.AreSame(cmd, result.MatchedCommand);
        Assert.AreEqual(0, result.Errors.Count);
    }

    [TestMethod]
    public void Parse_FlatCommand_WithLongOption_ValueStoredInOptionValues()
    {
        var nameOpt = new Option<string>("--name", "Name");
        var cmd = Cmd("greet", [nameOpt]);
        var result = Parser.Parse(cmd, ["--name", "alice"]);
        Assert.AreEqual(0, result.Errors.Count);
        Assert.IsTrue(result.OptionValues.ContainsKey(nameOpt));
        Assert.AreEqual("alice", result.OptionValues[nameOpt]);
    }

    [TestMethod]
    public void Parse_FlatCommand_WithShortOption_ValueStoredInOptionValues()
    {
        var nameOpt = new Option<string>("--name", "-n", "Name");
        var cmd = Cmd("greet", [nameOpt]);
        var result = Parser.Parse(cmd, ["-n", "bob"]);
        Assert.AreEqual(0, result.Errors.Count);
        Assert.AreEqual("bob", result.OptionValues[nameOpt]);
    }

    [TestMethod]
    public void Parse_FlatCommand_BoolFlag_SetsTrue()
    {
        var verboseOpt = new Option<bool>("--verbose", "Verbose");
        var cmd = Cmd("run", [verboseOpt]);
        var result = Parser.Parse(cmd, ["--verbose"]);
        Assert.AreEqual(0, result.Errors.Count);
        Assert.AreEqual(true, result.OptionValues[verboseOpt]);
    }

    [TestMethod]
    public void Parse_FlatCommand_BoolFlag_NotPresent_NotInOptionValues()
    {
        var verboseOpt = new Option<bool>("--verbose", "Verbose");
        var cmd = Cmd("run", [verboseOpt]);
        var result = Parser.Parse(cmd, []);
        Assert.IsFalse(result.OptionValues.ContainsKey(verboseOpt));
    }

    [TestMethod]
    public void Parse_FlatCommand_IntOption_Converted()
    {
        var countOpt = new Option<int>("--count", "Count");
        var cmd = Cmd("run", [countOpt]);
        var result = Parser.Parse(cmd, ["--count", "42"]);
        Assert.AreEqual(0, result.Errors.Count);
        Assert.AreEqual(42, result.OptionValues[countOpt]);
    }

    [TestMethod]
    public void Parse_FlatCommand_PositionalArgument_StoredInArgumentValues()
    {
        var fileArg = new Argument<string>("<file>", "Input file");
        var cmd = Cmd("add", arguments: [fileArg]);
        var result = Parser.Parse(cmd, ["readme.txt"]);
        Assert.AreEqual(0, result.Errors.Count);
        Assert.AreEqual("readme.txt", result.ArgumentValues[fileArg]);
    }

    [TestMethod]
    public void Parse_NestedBranch_MatchesCorrectLeafCommand()
    {
        var addCmd = Cmd("add");
        var removeCmd = Cmd("remove");
        var remoteBranch = new Branch("remote", "Manage remotes", [addCmd, removeCmd]);
        var root = new Branch("git", "Git", [remoteBranch]);
        var result = Parser.Parse(root, ["remote", "add"]);
        Assert.AreSame(addCmd, result.MatchedCommand);
        Assert.AreEqual(0, result.Errors.Count);
    }

    [TestMethod]
    public void Parse_BranchOptions_InheritedByLeafCommand()
    {
        var tokenOpt = new Option<string>("--token", "Auth token");
        var addCmd = Cmd("add");
        var remoteBranch = new Branch("remote", "Manage remotes", [tokenOpt], [addCmd]);
        var result = Parser.Parse(remoteBranch, ["add", "--token", "abc"]);
        Assert.AreEqual(0, result.Errors.Count);
        Assert.AreEqual("abc", result.OptionValues[tokenOpt]);
    }

    [TestMethod]
    public void Parse_ChildOptionOverridesParentOptionWithSameName()
    {
        var parentVerbose = new Option<bool>("--verbose", "Parent verbose");
        var childVerbose = new Option<bool>("--verbose", "Child verbose");
        var cmd = Cmd("run", [childVerbose]);
        var root = new Branch("app", "App", [parentVerbose], [cmd]);
        var result = Parser.Parse(root, ["run", "--verbose"]);
        Assert.AreEqual(0, result.Errors.Count);
        Assert.IsTrue(result.OptionValues.ContainsKey(childVerbose));
        Assert.IsFalse(result.OptionValues.ContainsKey(parentVerbose));
    }

    [TestMethod]
    public void Parse_IBranchWithoutMatchingSubcommand_ProducesError()
    {
        var root = new Branch("git", "Git", [Cmd("init")]);
        var result = Parser.Parse(root, ["unknown"]);
        Assert.IsTrue(result.Errors.Count > 0);
        Assert.IsNull(result.MatchedCommand);
    }

    [TestMethod]
    public void Parse_UnknownOption_ProducesError()
    {
        var cmd = Cmd("greet");
        var result = Parser.Parse(cmd, ["--unknown"]);
        Assert.AreEqual(1, result.Errors.Count);
        StringAssert.Contains(result.Errors[0].Message, "--unknown");
    }

    [TestMethod]
    public void Parse_RequiredOption_NotSupplied_ProducesError()
    {
        var nameOpt = new Option<string>("--name", "Name", isRequired: true);
        var cmd = Cmd("greet", [nameOpt]);
        var result = Parser.Parse(cmd, []);
        Assert.AreEqual(1, result.Errors.Count);
        StringAssert.Contains(result.Errors[0].Message, "--name");
    }

    [TestMethod]
    public void Parse_RequiredArgument_NotSupplied_ProducesError()
    {
        var fileArg = new Argument<string>("<file>", "File");
        var cmd = Cmd("add", arguments: [fileArg]);
        var result = Parser.Parse(cmd, []);
        Assert.AreEqual(1, result.Errors.Count);
        StringAssert.Contains(result.Errors[0].Message, "<file>");
    }

    [TestMethod]
    public void Parse_InvalidTypeConversion_ProducesError()
    {
        var countOpt = new Option<int>("--count", "Count");
        var cmd = Cmd("run", [countOpt]);
        var result = Parser.Parse(cmd, ["--count", "not-a-number"]);
        Assert.AreEqual(1, result.Errors.Count);
    }

    [TestMethod]
    public void Parse_EndOfOptionsSeparator_TokensAfterBecomeArguments()
    {
        var fileArg = new Argument<string>("<file>", "File");
        var cmd = Cmd("add", arguments: [fileArg]);
        var result = Parser.Parse(cmd, ["--", "--looks-like-option"]);
        Assert.AreEqual(0, result.Errors.Count);
        Assert.AreEqual("--looks-like-option", result.ArgumentValues[fileArg]);
    }

    [TestMethod]
    public void Parse_OptionalOption_WithDefault_DefaultFilledWhenNotSupplied()
    {
        var countOpt = new Option<int>("--count", "Count", false, 5);
        var cmd = Cmd("run", [countOpt]);
        var result = Parser.Parse(cmd, []);
        Assert.AreEqual(0, result.Errors.Count);
        Assert.AreEqual(5, result.OptionValues[countOpt]);
    }

    [TestMethod]
    public void Parse_ReachedNode_IsDeepestMatchedNode()
    {
        var addCmd = Cmd("add");
        var remoteBranch = new Branch("remote", "Manage remotes", [addCmd]);
        var result = Parser.Parse(remoteBranch, ["add"]);
        Assert.AreSame(addCmd, result.ReachedNode);
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
