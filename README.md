# SergeiM.Cli

An object-oriented .NET 8 library for building command-line applications.
No static classes. No magic strings. Interfaces all the way down.

## Features

- **Tree-based command model** — commands and branches form a typed tree
- **Strongly-typed options and arguments** — `Option<T>` / `Argument<T>` with
  built-in type conversion
- **Inherited options** — options declared on a branch are available to all
  its subcommands
- **Auto help** — `--help` / `-h` is injected automatically by `Application`
- **Decorators over inheritance** — extend behaviour by wrapping `ICommand`
  / `IBranch`
- **Exit codes** — `0` success · `1` unhandled exception · `2` parse error

## Installation

```bash
dotnet add package SergeiM.Cli
```

## Quick start

```csharp
using SergeiM.Cli;
using SergeiM.Cli.Abstractions;
using SergeiM.Cli.Arguments;
using SergeiM.Cli.Options;

var nameOpt  = new Option<string>("--name", "-n", "Your name", isRequired: true);
var countArg = new Argument<int>("<count>", "Number of greetings", defaultValue: 1);

var greetCmd = new GreetCommand(nameOpt, countArg);
return await new Application(greetCmd).RunAsync(args);

// ---

sealed class GreetCommand(Option<string> nameOpt, Argument<int> countArg) : ICommand
{
    public string Name => "greet";
    public string Description => "Print a greeting.";
    public IReadOnlyList<IOption> Options => [nameOpt];
    public IReadOnlyList<IArgument> Arguments => [countArg];

    public Task<int> ExecuteAsync(ICommandContext ctx, CancellationToken ct = default)
    {
        var name  = ctx.GetOption(nameOpt)!;
        var count = ctx.GetArgument(countArg);
        for (var i = 0; i < count; i++)
            Console.WriteLine($"Hello, {name}!");
        return Task.FromResult(0);
    }
}
```

```text
$ myapp greet --name Alice 3
Hello, Alice!
Hello, Alice!
Hello, Alice!

$ myapp greet --help
Print a greeting.

USAGE:
  greet [options] <count>

ARGUMENTS:
  <count>  Number of greetings

OPTIONS:
  --name, -n  Your name
  --help, -h  Show help and exit.
```

## Core concepts

### ICommand

A leaf node that performs work. Implement `ExecuteAsync` and return an integer
exit code. For synchronous logic, extend `SyncCommand` instead — it only
requires an `Execute` method.

```csharp
sealed class DeployCommand : ICommand
{
    private static readonly Option<string> _env =
        new("--env", "-e", "Target environment", isRequired: true);

    public string Name => "deploy";
    public string Description => "Deploy the application.";
    public IReadOnlyList<IOption> Options => [_env];
    public IReadOnlyList<IArgument> Arguments => [];

    public async Task<int> ExecuteAsync(ICommandContext ctx, CancellationToken ct = default)
    {
        var env = ctx.GetOption(_env)!;
        Console.WriteLine($"Deploying to {env}…");
        return 0;
    }
}
```

### SyncCommand

Base class for commands without async I/O. Override `Execute` and return an
exit code — no `Task.FromResult` boilerplate.

```csharp
sealed class VersionCommand : SyncCommand
{
    public override string Name => "version";
    public override string Description => "Print version.";
    public override IReadOnlyList<IOption> Options => [];

    public override int Execute(ICommandContext ctx)
    {
        Console.WriteLine("1.0.0");
        return 0;
    }
}
```

`Arguments` defaults to `[]` and can be overridden when needed.

### Branch

Groups related subcommands. Use the built-in `Branch` class for declarative
trees, or implement `IBranch` for custom behaviour.

```csharp
var root = new Branch("myapp", "My CLI tool", [
    new Branch("remote", "Manage remotes", [
        new AddRemoteCommand(),
        new RemoveRemoteCommand(),
    ]),
    new DeployCommand(),
]);

return await new Application(root).RunAsync(args);
```

### Options

`Option<T>` supports `string`, `int`, `double`, `bool`, and any `enum` out of
the box.

```csharp
// Long name only, optional
var verbose = new Option<bool>("--verbose", "Enable verbose output");

// Long + short alias, required
var output = new Option<string>("--output", "-o", "Output path", isRequired: true);

// Optional with explicit default
var retries = new Option<int>("--retries", "Retry count", isRequired: false, defaultValue: 3);

// Required, but falls back to an environment variable via factory callback
var url = new Option<string>("--url", "API URL",
    defaultFactory: () => Environment.GetEnvironmentVariable("API_URL"),
    isRequired: true);
```

When the option is not supplied on the command line, values are resolved
with this cascade: **explicit arg → static default → factory() →
required check**. If the factory returns `null`, the value is treated as
missing.
Bool options are **flags** — supply the name alone to set them to `true`:

```text
myapp build --verbose
```

Options are matched by **name**, not by reference. Two `Option<string>`
instances with the same `Name` resolve to the same parsed value, so you can
declare a descriptor once and reuse it across commands:

```csharp
static readonly Option<string> Url = new("--url", "API URL");

class ExportCommand : ICommand
{
    public IReadOnlyList<IOption> Options => [Url];
    // ctx.GetOption(Url) works regardless of where Url was declared
}
```

### Arguments

`Argument<T>` captures positional values in declaration order.

```csharp
// Required positional argument
var source = new Argument<string>("<source>", "Source path");

// Optional positional argument with default
var dest = new Argument<string>("<dest>", "Destination path", defaultValue: ".");
```

### ICommandContext

Inside `ExecuteAsync`, use `ICommandContext` for type-safe access to parsed
values.

```csharp
var name  = ctx.GetOption(nameOpt);       // T? — null when not supplied and no default
var file  = ctx.GetArgument(fileArg);     // T?
var extra = ctx.RemainingArgs;            // string[] — tokens after --
var token = ctx.CancellationToken;
```

### Inherited options

Options declared on a branch are available to all subcommands:

```csharp
var verbose = new Option<bool>("--verbose", "Enable verbose output");

var root = new Branch("myapp", "My CLI", [verbose], [
    new BuildCommand(),   // can read --verbose
    new DeployCommand(),  // can read --verbose
]);
```

### Application

`Application` wraps the root node, injects `--help`, and handles exit codes.

```csharp
// Minimal — uses ConsoleHelpRenderer, writes errors to Console.Error
new Application(root)

// Custom renderer
new Application(root, new MyHelpRenderer())

// Custom renderer + redirect errors (useful in tests)
new Application(root, new ConsoleHelpRenderer(), myTextWriter)
```

`RunAsync` returns:

| Code | Meaning                                                 |
| ---- | ------------------------------------------------------- |
| `0`  | Success                                                 |
| `1`  | Unhandled exception thrown by a command                 |
| `2`  | Parse error (unknown option, missing required value, …) |

## Project structure

```text
src/
  SergeiM.Cli/
    Abstractions/       INode, ICommand, IBranch, IOption<T>, IArgument<T>,
                        ICommandContext, IHelpRenderer, IParser, IApplication,
                        ParseResult, ParseError
    Options/            Option<T>
    Arguments/          Argument<T>
    Parsing/            Token, Tokenizer, Parser
    Branch.cs
    CommandContext.cs
    ConsoleHelpRenderer.cs
    WithHelp.cs
    Application.cs
  SergeiM.Cli.Tests/
```

## Conventional Commits

This project follows [Conventional Commits](https://www.conventionalcommits.org/)
to automate versioning and changelog generation via
[release-please](https://github.com/googleapis/release-please).

| Type       | Purpose                              | Bump    |
| ---------- | ------------------------------------ | ------- |
| `feat`     | New feature                          | minor   |
| `fix`      | Bug fix                              | patch   |
| `docs`     | Documentation only changes           | —       |
| `style`    | Code style (formatting, whitespace)  | —       |
| `refactor` | Code refactoring                     | —       |
| `test`     | Adding or updating tests             | —       |
| `chore`    | Maintenance (CI, deps, etc.)         | —       |

Breaking changes are signaled with `!` after the type (`feat!:`)
or a `BREAKING CHANGE:` footer — triggers a major bump.

```text
feat(#4): support factory callback as default value for options
fix(#3): don't show <subcommand> in usage for branches with no subcommands
chore: update CI dependencies
```

## License

See [LICENSE.txt](LICENSE.txt).
