using System.CommandLine;
using dotnet_json.Core;

namespace dotnet_json.Commands;

public class SetCommand : CommandBase
{
    private Argument<string> Key = new("key")
    {
        Description = "The key to set (use ':' to set nested object and use index numbers to set array values eg. nested:key or nested:1:key)",
        Arity = ArgumentArity.ExactlyOne
    };

    private Argument<string> Value = new("value")
    {
        Description = "The value to set",
        Arity = ArgumentArity.ExactlyOne
    };

    private Option<bool> Existing = new("--existing", "-e")
    {
        Description = "Only set the value if the key already exists in the json file, otherwise do nothing"
    };

    public SetCommand() : base("set", "set a value in a json file")
    {
        Arguments.Add(Key);
        Arguments.Add(Value);
        Options.Add(Existing);
        Options.Add(Compressed);
    }

    protected override async Task<int> ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var key = parseResult.GetValue(Key) ?? throw new ArgumentException("Missing argument <key>");
        var value = parseResult.GetValue(Value) ?? throw new ArgumentException("Missing argument <value>");
        var existing = parseResult.GetValue(Existing);

        JsonDocument document;

        await using (var inputStream = GetInputStream(parseResult))
            document = JsonDocument.ReadFromStream(inputStream);

        if (existing && document[key] == null)
            return 0;

        document[key] = value;

        await using (var outputStream = GetOutputStream(parseResult))
            document.WriteToStream(outputStream, GetFormatting(parseResult, Compressed));

        return 0;
    }
}
