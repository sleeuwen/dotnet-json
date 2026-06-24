using System.CommandLine;
using dotnet_json.Core;
using Newtonsoft.Json.Linq;

namespace dotnet_json.Commands;

public class GetCommand : CommandBase
{
    private Argument<string> Key = new("key")
    {
        Description = "The key to get (use ':' to get a nested object and use index numbers to get array values eg. nested:key or nested:1:key)",
        Arity = ArgumentArity.ExactlyOne,
    };

    private Option<bool> Exact = new("--exact", "-e")
    {
        Description = "only return exact value matches, this will return an error for references to nested objects/arrays."
    };

    public GetCommand()
        : base("get", "Read a value from a JSON file.", false)
    {
        Arguments.Add(Key);
        Options.Add(Exact);
    }

    protected override async Task<int> ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var key = parseResult.GetValue(Key) ?? throw new ArgumentException("Missing argument <key>");

        JsonDocument document;

        await using (var inputStream = GetInputStream(parseResult))
            document = JsonDocument.ReadFromStream(inputStream);

        var result = document[key];
        if (result == null)
        {
            parseResult.InvocationConfiguration.Error.WriteLine($"Key '{key}' does not exist in the json");
            return 1;
        }

        if (parseResult.GetValue(Exact) && !(result is JValue))
        {
            parseResult.InvocationConfiguration.Error.WriteLine($"Value for key '{key}' is a complex object.");
            return 1;
        }

        parseResult.InvocationConfiguration.Output.WriteLine(ToString(result));
        return 0;
    }

    private static string ToString(object obj) => obj switch
    {
        null => "null",
        JValue value when value.Value is null => "null",
        JValue value => ToString(value.Value!),
        bool b => b.ToString().ToLowerInvariant(),
        _ => obj.ToString() ?? "",
    };
}
