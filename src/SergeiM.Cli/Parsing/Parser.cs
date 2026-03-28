// SPDX-FileCopyrightText: Copyright (c) 2026 Sergei Mukhin
// SPDX-License-Identifier: MIT

using System.ComponentModel;
using System.Globalization;
using SergeiM.Cli.Abstractions;

namespace SergeiM.Cli.Parsing;

/// <summary>
/// Default implementation of <see cref="IParser"/> that tokenizes the argument array and
/// matches it against a command tree, collecting options, arguments, and parse errors.
/// </summary>
public sealed class Parser : IParser
{
    /// <inheritdoc/>
    public ParseResult Parse(INode root, string[] args)
    {
        var tokens = new Tokenizer().Tokenize(args);
        var (currentNode, effectiveOptions, subcmdIndices) = TraverseTree(root, tokens);
        return BuildResult(currentNode, effectiveOptions, tokens, subcmdIndices);
    }

    /// <summary>
    /// Phase 1: walks the token stream to find the deepest matching node in the command tree.
    /// Consumes subcommand-name tokens and skips option+value pairs to avoid mistaking an
    /// option value for a subcommand name.
    /// </summary>
    private static (INode node, List<IOption> options, HashSet<int> subcmdIndices) TraverseTree(
        INode root, Token[] tokens)
    {
        var effectiveOptions = new List<IOption>();
        AddOptionsWithOverride(effectiveOptions, root.Options);
        var subcmdIndices = new HashSet<int>();
        INode currentNode = root;
        var pos = 0;
        while (currentNode is IBranch branch && pos < tokens.Length)
        {
            var token = tokens[pos];
            if (token.Kind == TokenKind.EndOfOptions) break;
            if (token.Kind == TokenKind.LongOption || token.Kind == TokenKind.ShortOption)
            {
                var opt = FindOption(effectiveOptions, token.Raw);
                pos++;
                if (opt != null && opt.ValueType != typeof(bool) && pos < tokens.Length && tokens[pos].Kind == TokenKind.Value)
                    pos++;
                continue;
            }
            var sub = branch.Subcommands.FirstOrDefault(s => s.Name == token.Raw);
            if (sub == null) break;
            subcmdIndices.Add(pos++);
            currentNode = sub;
            AddOptionsWithOverride(effectiveOptions, sub.Options);
        }
        return (currentNode, effectiveOptions, subcmdIndices);
    }

    /// <summary>
    /// Phase 2: processes all non-subcommand tokens, building option values, argument values,
    /// remaining args, and parse errors. Also validates required inputs and fills defaults.
    /// </summary>
    private static ParseResult BuildResult(
        INode currentNode,
        List<IOption> effectiveOptions,
        Token[] tokens,
        HashSet<int> subcmdIndices)
    {
        var optionValues = new Dictionary<IOption, object?>();
        var argumentValues = new Dictionary<IArgument, object?>();
        var errors = new List<ParseError>();
        var remaining = new List<string>();
        var command = currentNode as ICommand;
        var argPos = 0;
        var pastEoo = false;
        if (currentNode is IBranch and not ICommand)
            errors.Add(new ParseError($"No subcommand specified for '{currentNode.Name}'. Run with --help for usage."));
        for (var i = 0; i < tokens.Length; i++)
        {
            if (subcmdIndices.Contains(i)) continue;
            var token = tokens[i];
            if (token.Kind == TokenKind.EndOfOptions)
            {
                pastEoo = true;
                continue;
            }
            if (pastEoo || token.Kind == TokenKind.Value)
            {
                if (command != null && argPos < command.Arguments.Count)
                {
                    var arg = command.Arguments[argPos++];
                    if (TryConvert(token.Raw, arg.ValueType, out var v))
                        argumentValues[arg] = v;
                    else
                        errors.Add(new ParseError($"Cannot convert '{token.Raw}' to {arg.ValueType.Name} for argument {arg.Name}."));
                }
                else
                {
                    remaining.Add(token.Raw);
                }
                continue;
            }
            var opt = FindOption(effectiveOptions, token.Raw);
            if (opt == null)
            {
                errors.Add(new ParseError($"Unknown option: {token.Raw}."));
                continue;
            }
            if (opt.ValueType == typeof(bool))
            {
                optionValues[opt] = true;
                continue;
            }
            if (i + 1 < tokens.Length && tokens[i + 1].Kind == TokenKind.Value && !subcmdIndices.Contains(i + 1))
            {
                i++;
                if (TryConvert(tokens[i].Raw, opt.ValueType, out var v))
                    optionValues[opt] = v;
                else
                    errors.Add(new ParseError($"Cannot convert '{tokens[i].Raw}' to {opt.ValueType.Name} for option {opt.Name}."));
            }
            else
            {
                errors.Add(new ParseError($"Option {opt.Name} requires a value."));
            }
        }
        ValidateRequired(effectiveOptions, command, optionValues, argumentValues, errors);
        FillDefaults(effectiveOptions, command, optionValues, argumentValues);
        return new ParseResult(currentNode, command, optionValues, argumentValues, [.. remaining], errors);
    }

    /// <summary>
    /// Appends <see cref="ParseError"/> entries for any required option or argument that was
    /// not supplied.
    /// </summary>
    private static void ValidateRequired(
        List<IOption> effectiveOptions,
        ICommand? command,
        Dictionary<IOption, object?> optionValues,
        Dictionary<IArgument, object?> argumentValues,
        List<ParseError> errors)
    {
        foreach (var opt in effectiveOptions)
        {
            if (opt.IsRequired && !optionValues.ContainsKey(opt))
                errors.Add(new ParseError($"Required option {opt.Name} was not supplied."));
        }
        if (command == null) return;
        foreach (var arg in command.Arguments)
        {
            if (arg.IsRequired && !argumentValues.ContainsKey(arg))
                errors.Add(new ParseError($"Required argument {arg.Name} was not supplied."));
        }
    }

    /// <summary>
    /// Populates <paramref name="optionValues"/> and <paramref name="argumentValues"/> with
    /// declared default values for any inputs that were not explicitly supplied.
    /// </summary>
    private static void FillDefaults(
        List<IOption> effectiveOptions,
        ICommand? command,
        Dictionary<IOption, object?> optionValues,
        Dictionary<IArgument, object?> argumentValues)
    {
        foreach (var opt in effectiveOptions)
        {
            if (!optionValues.ContainsKey(opt) && opt.HasDefault)
                optionValues[opt] = opt.DefaultValue;
        }
        if (command == null) return;
        foreach (var arg in command.Arguments)
        {
            if (!argumentValues.ContainsKey(arg) && arg.HasDefault)
                argumentValues[arg] = arg.DefaultValue;
        }
    }

    /// <summary>
    /// Searches <paramref name="options"/> from the end (highest priority first) for an option
    /// whose long name or short name equals <paramref name="raw"/>.
    /// </summary>
    private static IOption? FindOption(List<IOption> options, string raw)
    {
        for (var i = options.Count - 1; i >= 0; i--)
        {
            if (options[i].Name == raw || options[i].ShortName == raw)
                return options[i];
        }
        return null;
    }

    /// <summary>
    /// Appends each option from <paramref name="source"/> to <paramref name="target"/>,
    /// first removing any existing entry with a conflicting long or short name so that
    /// child options override parent options with the same name.
    /// </summary>
    private static void AddOptionsWithOverride(List<IOption> target, IReadOnlyList<IOption> source)
    {
        foreach (var option in source)
        {
            target.RemoveAll(o =>
                o.Name == option.Name ||
                (option.ShortName != null && o.ShortName != null && o.ShortName == option.ShortName));
            target.Add(option);
        }
    }

    /// <summary>
    /// Attempts to convert <paramref name="raw"/> to <paramref name="targetType"/> using
    /// <see cref="TypeDescriptor"/> with invariant culture.
    /// Supports <see cref="string"/>, numeric types, <see cref="bool"/>, and enums.
    /// </summary>
    private static bool TryConvert(string raw, Type targetType, out object? value)
    {
        if (targetType == typeof(string))
        {
            value = raw;
            return true;
        }
        try
        {
            var converter = TypeDescriptor.GetConverter(targetType);
            value = converter.ConvertFrom(null, CultureInfo.InvariantCulture, raw);
            return true;
        }
        catch
        {
            value = null;
            return false;
        }
    }
}
