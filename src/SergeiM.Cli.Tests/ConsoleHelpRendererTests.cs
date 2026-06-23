// SPDX-FileCopyrightText: Copyright (c) 2026 Sergei Mukhin
// SPDX-License-Identifier: MIT

using SergeiM.Cli.Abstractions;
using SergeiM.Cli.Arguments;
using SergeiM.Cli.Options;

namespace SergeiM.Cli.Tests;

[TestClass]
public class ConsoleHelpRendererTests
{
    private static ConsoleHelpRenderer Renderer => new();

    private static string Render(INode node)
    {
        using var sw = new StringWriter();
        Renderer.Render(node, sw);
        return sw.ToString();
    }

    private static string[] Lines(string output)
        => output.Split(new[] { '\r', '\n' }, StringSplitOptions.None);

    private sealed class StubCommand : ICommand
    {
        public StubCommand(
            string name,
            string description,
            IReadOnlyList<IOption>? options = null,
            IReadOnlyList<IArgument>? arguments = null)
        {
            Name = name;
            Description = description;
            Options = options ?? [];
            Arguments = arguments ?? [];
        }
        public string Name { get; }
        public string Description { get; }
        public IReadOnlyList<IOption> Options { get; }
        public IReadOnlyList<IArgument> Arguments { get; }
        public Task<int> ExecuteAsync(ICommandContext ctx, CancellationToken ct = default)
            => Task.FromResult(0);
    }

    private sealed class StubBranch : IBranch
    {
        public StubBranch(
            string name,
            string description,
            IReadOnlyList<INode> subcommands,
            IReadOnlyList<IOption>? options = null)
        {
            Name = name;
            Description = description;
            Subcommands = subcommands;
            Options = options ?? [];
        }
        public string Name { get; }
        public string Description { get; }
        public IReadOnlyList<INode> Subcommands { get; }
        public IReadOnlyList<IOption> Options { get; }
    }

    [TestMethod]
    public void Render_Command_FirstLineIsDescription()
    {
        var cmd = new StubCommand("greet", "Say hello");
        var lines = Lines(Render(cmd));
        Assert.AreEqual("Say hello", lines[0]);
    }

    [TestMethod]
    public void Render_Command_SecondLineIsBlank()
    {
        var cmd = new StubCommand("greet", "Say hello");
        var lines = Lines(Render(cmd));
        Assert.AreEqual(string.Empty, lines[1]);
    }

    [TestMethod]
    public void Render_Command_UsageSectionPresent()
    {
        var cmd = new StubCommand("greet", "Say hello");
        var output = Render(cmd);
        Assert.IsTrue(output.Contains("USAGE:"));
    }

    [TestMethod]
    public void Render_Command_NoOptionsNoArgs_UsageLineIsName()
    {
        var cmd = new StubCommand("greet", "Say hello");
        var output = Render(cmd);
        Assert.IsTrue(output.Contains("  greet\n") || output.Contains("  greet\r\n"));
    }

    [TestMethod]
    public void Render_Command_WithOptions_UsageContainsOptionsPlaceholder()
    {
        var cmd = new StubCommand("greet", "Say hello",
            options: [new Option<string>("--name", "Name")]);
        var output = Render(cmd);
        Assert.IsTrue(output.Contains("  greet [options]"));
    }

    [TestMethod]
    public void Render_Command_WithRequiredArg_UsageContainsArgName()
    {
        var arg = new Argument<string>("<name>", "Your name");
        var cmd = new StubCommand("greet", "Say hello", arguments: [arg]);
        var output = Render(cmd);
        Assert.IsTrue(output.Contains("  greet <name>"));
    }

    [TestMethod]
    public void Render_Command_WithOptionalArg_UsageContainsBracketedArgName()
    {
        var arg = new Argument<string>("<title>", "Title", isRequired: false);
        var cmd = new StubCommand("greet", "Say hello", arguments: [arg]);
        var output = Render(cmd);
        Assert.IsTrue(output.Contains("  greet [<title>]"));
    }

    [TestMethod]
    public void Render_Branch_UsageContainsSubcommandPlaceholder()
    {
        var branch = new StubBranch("git", "Version control",
            subcommands: [new StubCommand("add", "Stage files")]);
        var output = Render(branch);
        Assert.IsTrue(output.Contains("  git <subcommand>"));
    }

    [TestMethod]
    public void Render_Branch_WithOptions_UsageContainsOptionsPlaceholder()
    {
        var branch = new StubBranch("git", "Version control",
            subcommands: [new StubCommand("add", "Stage files")],
            options: [new Option<bool>("--verbose", "Verbose")]);
        var output = Render(branch);
        Assert.IsTrue(output.Contains("  git <subcommand> [options]"));
    }

    [TestMethod]
    public void Render_Branch_NoSubcommands_UsageContainsNoSubcommandPlaceholder()
    {
        var branch = new StubBranch("export", "Export data", subcommands: []);
        var output = Render(branch);
        Assert.IsFalse(output.Contains("<subcommand>"));
        Assert.IsTrue(output.Contains("  export"));
    }

    [TestMethod]
    public void Render_Command_WithArguments_ArgumentsSectionPresent()
    {
        var arg = new Argument<string>("<file>", "Input file");
        var cmd = new StubCommand("run", "Run it", arguments: [arg]);
        var output = Render(cmd);
        Assert.IsTrue(output.Contains("ARGUMENTS:"));
    }

    [TestMethod]
    public void Render_Command_WithArguments_AllArgumentsListed()
    {
        var arg1 = new Argument<string>("<src>", "Source");
        var arg2 = new Argument<string>("<dst>", "Destination");
        var cmd = new StubCommand("cp", "Copy", arguments: [arg1, arg2]);
        var output = Render(cmd);
        Assert.IsTrue(output.Contains("<src>"));
        Assert.IsTrue(output.Contains("Source"));
        Assert.IsTrue(output.Contains("<dst>"));
        Assert.IsTrue(output.Contains("Destination"));
    }

    [TestMethod]
    public void Render_Command_NoArguments_NoArgumentsSection()
    {
        var cmd = new StubCommand("greet", "Say hello");
        var output = Render(cmd);
        Assert.IsFalse(output.Contains("ARGUMENTS:"));
    }

    [TestMethod]
    public void Render_Branch_NoArgumentsSection()
    {
        var branch = new StubBranch("git", "Version control",
            subcommands: [new StubCommand("add", "Stage files")]);
        var output = Render(branch);
        Assert.IsFalse(output.Contains("ARGUMENTS:"));
    }

    [TestMethod]
    public void Render_Command_WithOptions_OptionsSectionPresent()
    {
        var cmd = new StubCommand("greet", "Say hello",
            options: [new Option<string>("--name", "Name")]);
        var output = Render(cmd);
        Assert.IsTrue(output.Contains("OPTIONS:"));
    }

    [TestMethod]
    public void Render_Command_WithLongAndShortOption_BothNamesInRow()
    {
        var cmd = new StubCommand("greet", "Say hello",
            options: [new Option<string>("--name", "-n", "Your name")]);
        var output = Render(cmd);
        Assert.IsTrue(output.Contains("--name, -n"));
        Assert.IsTrue(output.Contains("Your name"));
    }

    [TestMethod]
    public void Render_Command_LongOptionOnly_NoCommaInLabel()
    {
        var cmd = new StubCommand("greet", "Say hello",
            options: [new Option<string>("--verbose", "Verbose")]);
        var output = Render(cmd);
        StringAssert.Contains(output, "--verbose");
        Assert.IsFalse(output.Contains("--verbose,"));
    }

    [TestMethod]
    public void Render_Command_NoOptions_NoOptionsSection()
    {
        var cmd = new StubCommand("greet", "Say hello");
        var output = Render(cmd);
        Assert.IsFalse(output.Contains("OPTIONS:"));
    }

    [TestMethod]
    public void Render_Branch_SubcommandsSectionPresent()
    {
        var branch = new StubBranch("git", "Version control",
            subcommands: [new StubCommand("add", "Stage files")]);
        var output = Render(branch);
        Assert.IsTrue(output.Contains("SUBCOMMANDS:"));
    }

    [TestMethod]
    public void Render_Branch_AllSubcommandsListed()
    {
        var branch = new StubBranch("git", "Version control",
            subcommands: [
                new StubCommand("add", "Stage files"),
                new StubCommand("commit", "Record changes")
            ]);
        var output = Render(branch);
        Assert.IsTrue(output.Contains("add"));
        Assert.IsTrue(output.Contains("Stage files"));
        Assert.IsTrue(output.Contains("commit"));
        Assert.IsTrue(output.Contains("Record changes"));
    }

    [TestMethod]
    public void Render_Command_NoSubcommandsSection()
    {
        var cmd = new StubCommand("greet", "Say hello");
        var output = Render(cmd);
        Assert.IsFalse(output.Contains("SUBCOMMANDS:"));
    }

    [TestMethod]
    public void Render_Options_ColumnsAlignedByLongestLabel()
    {
        var cmd = new StubCommand("cmd", "Command",
            options: [
                new Option<string>("--name", "-n", "Short label option"),
                new Option<bool>("--verbose", "A much longer named option")
            ]);
        var lines = Lines(Render(cmd));
        var optLines = lines
            .SkipWhile(l => l != "OPTIONS:")
            .Skip(1)
            .TakeWhile(l => l.StartsWith("  "))
            .ToArray();
        Assert.AreEqual(2, optLines.Length);
        var col1 = optLines[0].IndexOf("Short label option", StringComparison.Ordinal);
        var col2 = optLines[1].IndexOf("A much longer named option", StringComparison.Ordinal);
        Assert.AreEqual(col1, col2, "Description columns must be aligned");
    }

    [TestMethod]
    public void Render_Command_SectionOrder_UsageThenArgumentsThenOptions()
    {
        var cmd = new StubCommand("run", "Run it",
            options: [new Option<string>("--env", "Environment")],
            arguments: [new Argument<string>("<script>", "Script to run")]);
        var output = Render(cmd);
        var usageIdx = output.IndexOf("USAGE:", StringComparison.Ordinal);
        var argsIdx = output.IndexOf("ARGUMENTS:", StringComparison.Ordinal);
        var optsIdx = output.IndexOf("OPTIONS:", StringComparison.Ordinal);
        Assert.IsTrue(usageIdx < argsIdx, "USAGE must precede ARGUMENTS");
        Assert.IsTrue(argsIdx < optsIdx, "ARGUMENTS must precede OPTIONS");
    }

    [TestMethod]
    public void Render_Branch_SectionOrder_UsageThenOptionsThenSubcommands()
    {
        var branch = new StubBranch("git", "Version control",
            subcommands: [new StubCommand("add", "Stage files")],
            options: [new Option<bool>("--verbose", "Verbose")]);
        var output = Render(branch);
        var usageIdx = output.IndexOf("USAGE:", StringComparison.Ordinal);
        var optsIdx = output.IndexOf("OPTIONS:", StringComparison.Ordinal);
        var subcIdx = output.IndexOf("SUBCOMMANDS:", StringComparison.Ordinal);
        Assert.IsTrue(usageIdx < optsIdx, "USAGE must precede OPTIONS");
        Assert.IsTrue(optsIdx < subcIdx, "OPTIONS must precede SUBCOMMANDS");
    }
}
