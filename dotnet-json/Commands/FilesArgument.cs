using System.CommandLine;

namespace dotnet_json.Commands;

public class FilesArgument : Argument<List<string>>
{
    public FilesArgument(string name)
        : base(name)
    {
        AddValidator();
    }

    public bool AllowNewFile { get; set; }

    private void AddValidator()
    {
        this.Validators.Add(symbol =>
        {
            var error = symbol.Tokens
                .Select(t => t.Value)
                .Where(_ => !AllowNewFile)
                .Where(filePath => filePath != "-")
                .Where(filePath => !File.Exists(filePath))
                .Select(filePath => $"File does not exist: {filePath}")
                .FirstOrDefault();

            if (error != null)
                symbol.AddError(error);
        });
    }
}
