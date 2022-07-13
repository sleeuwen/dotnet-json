using System.CommandLine;
using System.IO;
using System.Linq;

namespace dotnet_json.Commands
{
    public class FileOption : Option<string>
    {
        public FileOption(string alias, string? description = null)
            : base(alias, description)
        {
            base.Arity = ArgumentArity.ExactlyOne;
            this.AddValidator();
        }

        public FileOption(string[] aliases, string? description = null)
            : base(aliases, description)
        {
            base.Arity = ArgumentArity.ExactlyOne;
            this.AddValidator();
        }

        public bool AllowNewFile { get; set; }

        private void AddValidator()
        {
            this.AddValidator(symbol =>
            {
                symbol.ErrorMessage ??= symbol.Tokens
                    .Select(t => t.Value)
                    .Where(_ => !AllowNewFile) // Need to check AllowNewFile at this point because AddValidator() is called from constructor
                    .Where(filePath => filePath != "-")
                    .Where(filePath => !File.Exists(filePath))
                    .Select(filePath => $"File does not exist: {filePath}")
                    .FirstOrDefault();
            });
        }
    }
}
