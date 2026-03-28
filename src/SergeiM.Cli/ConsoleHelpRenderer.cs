// SPDX-FileCopyrightText: Copyright (c) 2026 Sergei Mukhin
// SPDX-License-Identifier: MIT

using System.Text;
using SergeiM.Cli.Abstractions;

namespace SergeiM.Cli;

/// <summary>
/// Renders human-readable help text for a command tree node to a <see cref="TextWriter"/>.
/// Sections shown depend on the node type: ARGUMENTS and SUBCOMMANDS are shown only
/// for <see cref="ICommand"/> and <see cref="IBranch"/> nodes respectively.
/// </summary>
public sealed class ConsoleHelpRenderer : IHelpRenderer
{
    /// <inheritdoc/>
    public void Render(INode node, TextWriter output)
    {
        output.WriteLine(node.Description);
        output.WriteLine();
        RenderUsage(node, output);
        if (node is ICommand cmd && cmd.Arguments.Count > 0)
            RenderArguments(cmd.Arguments, output);
        if (node.Options.Count > 0)
            RenderOptions(node.Options, output);
        if (node is IBranch branch && branch.Subcommands.Count > 0)
            RenderSubcommands(branch.Subcommands, output);
    }

    private static void RenderUsage(INode node, TextWriter output)
    {
        var sb = new StringBuilder("  ").Append(node.Name);
        if (node is IBranch)
            sb.Append(" <subcommand>");
        if (node.Options.Count > 0)
            sb.Append(" [options]");
        if (node is ICommand cmd)
        {
            foreach (var arg in cmd.Arguments)
                sb.Append(arg.IsRequired ? $" {arg.Name}" : $" [{arg.Name}]");
        }
        output.WriteLine("USAGE:");
        output.WriteLine(sb.ToString());
        output.WriteLine();
    }

    private static void RenderArguments(IReadOnlyList<IArgument> arguments, TextWriter output)
    {
        var rows = arguments.Select(a => (a.Name, a.Description)).ToList();
        RenderSection("ARGUMENTS", rows, output);
    }

    private static void RenderOptions(IReadOnlyList<IOption> options, TextWriter output)
    {
        var rows = options.Select(o => (OptionLabel(o), o.Description)).ToList();
        RenderSection("OPTIONS", rows, output);
    }

    private static void RenderSubcommands(IReadOnlyList<INode> subcommands, TextWriter output)
    {
        var rows = subcommands.Select(s => (s.Name, s.Description)).ToList();
        RenderSection("SUBCOMMANDS", rows, output);
    }

    private static void RenderSection(string header, List<(string Left, string Right)> rows, TextWriter output)
    {
        var maxWidth = rows.Max(r => r.Left.Length);
        output.WriteLine($"{header}:");
        foreach (var (left, right) in rows)
            output.WriteLine($"  {left.PadRight(maxWidth)}  {right}");
        output.WriteLine();
    }

    private static string OptionLabel(IOption option)
        => option.ShortName is not null ? $"{option.Name}, {option.ShortName}" : option.Name;
}
