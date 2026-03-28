// SPDX-FileCopyrightText: Copyright (c) 2026 Sergei Mukhin
// SPDX-License-Identifier: MIT

namespace SergeiM.Cli.Abstractions;

/// <summary>
/// Renders help text for a command tree node to a <see cref="TextWriter"/>.
/// </summary>
public interface IHelpRenderer
{
    /// <summary>
    /// Writes formatted help text for <paramref name="node"/> to <paramref name="output"/>.
    /// </summary>
    /// <param name="node">The command tree node whose help text is to be rendered.</param>
    /// <param name="output">The destination writer that receives the formatted output.</param>
    void Render(INode node, TextWriter output);
}
