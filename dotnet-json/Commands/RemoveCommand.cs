using System.CommandLine;
using dotnet_json.Core;

namespace dotnet_json.Commands;

public class RemoveCommand : CommandBase
{
    private Argument<string> Key = new("key")
    {
        Description = "The JSON key to remove",
        Arity = ArgumentArity.ExactlyOne
    };

    public RemoveCommand()
        : base("remove", "Remove a value from the json")
    {
        Key.Validators.Add(symbol =>
        {
            if (symbol.Tokens.Count == 1 && string.IsNullOrEmpty(symbol.Tokens[0].Value))
                symbol.AddError("Removing an empty key is not allowed.");
        });
        Arguments.Add(Key);

        Aliases.Add("rm");

        Options.Add(Compressed);
    }

    protected override async Task<int> ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var key = parseResult.GetValue(Key) ?? throw new ArgumentException("Missing argument <key>");

        JsonDocument document;

        await using (var inputStream = GetInputStream(parseResult))
            document = JsonDocument.ReadFromStream(inputStream);

        document.Remove(key);

        await using (var outputStream = GetOutputStream(parseResult))
            document.WriteToStream(outputStream, GetFormatting(parseResult, Compressed));

        return 0;
    }
}
