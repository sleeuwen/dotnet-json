using System.CommandLine;
using dotnet_json.Core;

namespace dotnet_json.Commands;

public class MergeCommand : CommandBase
{
    private FilesArgument Files = new("files")
    {
        Description = "The names of the files to merge with the first file.",
        Arity = ArgumentArity.OneOrMore,
    };

    public MergeCommand() : base("merge", "merge two or more json files into one")
    {
        Arguments.Add(Files);
        Options.Add(Compressed);
    }

    protected override async Task<int> ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var files = parseResult.GetValue(Files) ?? throw new ArgumentException("Missing argument <files>");

        JsonDocument document;

        await using (var inputStream = GetInputStream(parseResult))
            document = JsonDocument.ReadFromStream(inputStream);

        foreach (var file in files)
        {
            await using var stream = GetStream(file);
            var mergeDocument = JsonDocument.ReadFromStream(stream);
            document.Merge(mergeDocument);
        }

        await using (var outputStream = GetOutputStream(parseResult))
            document.WriteToStream(outputStream, GetFormatting(parseResult, Compressed));

        return 0;
    }

    private static Stream GetStream(string filename) => filename switch
    {
        "-" => Console.OpenStandardInput(),
        _ => File.OpenRead(filename),
    };
}
