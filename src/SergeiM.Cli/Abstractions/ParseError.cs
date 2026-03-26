namespace SergeiM.Cli.Abstractions;

/// <summary>
/// Describes a single error encountered while parsing command-line arguments.
/// </summary>
/// <param name="Message">A human-readable description of the error.</param>
public sealed record ParseError(string Message);
