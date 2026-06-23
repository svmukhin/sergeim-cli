// SPDX-FileCopyrightText: Copyright (c) 2026 Sergei Mukhin
// SPDX-License-Identifier: MIT

using SergeiM.Cli.Abstractions;

namespace SergeiM.Cli.Options;

/// <summary>
/// Descriptor for the built-in <c>--help</c>/<c>-h</c> flag.
/// A single instance is created by <see cref="Application"/> and shared between
/// <see cref="WithHelpOption"/> and <see cref="WithHelpRenderer"/> so that the same
/// object acts as the dictionary key in <c>ParseResult.OptionValues</c>.
/// </summary>
internal sealed class HelpOptionDescriptor : IOption<bool>
{
    /// <inheritdoc/>
    public string Name => "--help";

    /// <inheritdoc/>
    public string? ShortName => "-h";

    /// <inheritdoc/>
    public string Description => "Show help and exit.";

    /// <inheritdoc/>
    public Type ValueType => typeof(bool);

    /// <inheritdoc/>
    public bool IsRequired => false;

    /// <inheritdoc/>
    public bool HasDefault => true;

    /// <inheritdoc/>
    public bool Default => false;

    /// <inheritdoc/>
    public object? DefaultValue => false;

    /// <inheritdoc/>
    public bool HasDefaultFactory => false;

    /// <inheritdoc/>
    public Func<object?>? DefaultFactory => null;

    /// <inheritdoc/>
    public Func<bool>? TypedDefaultFactory => null;
}
