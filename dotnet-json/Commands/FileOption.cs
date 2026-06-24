using System.CommandLine;

namespace dotnet_json.Commands;

public class FileOption : Option<string>
{
    public FileOption(string name, params string[] aliases)
        : base(name, aliases)
    {
        Arity = ArgumentArity.ExactlyOne;
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
