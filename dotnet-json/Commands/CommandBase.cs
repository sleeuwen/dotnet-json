using System.CommandLine;
using Newtonsoft.Json;

namespace dotnet_json.Commands;

public abstract class CommandBase : Command
{
    protected FileArgument InputFile = new("file")
    {
        Description = "The JSON file (use '-' for STDIN)",
    };

    protected FileOption OutputFile = new("--output", "-o")
    {
        Description = "The output file (use '-' for STDOUT, defaults to <file>)",
        AllowNewFile = true
    };

    protected Option<bool> Compressed = new("--compressed", "-c")
    {
        Description = "Write the output in compressed form (defaults to indented)"
    };

    protected CommandBase(string name, string? description = null, bool includeOutputOption = true)
        : base(name, description)
    {
        Arguments.Add(InputFile);

        if (includeOutputOption)
            Options.Add(OutputFile);

        this.SetAction(ExecuteAsync);
    }

    protected Stream GetInputStream(ParseResult parseResult)
    {
        var filename = parseResult.GetValue(InputFile) ?? throw new Exception("GetInputStream must be called from a command handler");

        return filename switch
        {
            "-" => Console.OpenStandardInput(),
            _ => File.OpenRead(filename),
        };
    }

    protected Stream GetOutputStream(ParseResult parseResult)
    {
        var filename = parseResult.GetResult(OutputFile) != null
            ? parseResult.GetValue(OutputFile)
            : parseResult.GetValue(InputFile);

        return filename switch
        {
            "-" or null => Console.OpenStandardOutput(),
            _ => File.Create(filename),
        };
    }

    protected static Formatting GetFormatting(ParseResult parseResult, Option<bool> compressed)
    {
        return parseResult.GetValue(compressed) ? Formatting.None : Formatting.Indented;
    }

    protected abstract Task<int> ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken);
}
