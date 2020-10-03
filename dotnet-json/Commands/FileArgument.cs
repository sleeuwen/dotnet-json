using System.CommandLine;
using System.IO;
using System.Linq;

namespace dotnet_json.Commands
{
    public class FileArgument : Argument<string>
    {
        public FileArgument()
        {
            AddValidator();
        }

        public FileArgument(string name, string? description = null)
            : base(name, description)
        {
            AddValidator();
        }

        private void AddValidator()
        {
            this.AddValidator(symbol =>
                symbol.Tokens
                    .Select(t => t.Value)
                    .Where(filePath => filePath != "-")
                    .Where(filePath => !File.Exists(filePath))
                    .Select(filePath => $"File does not exist: {filePath}")
                    .FirstOrDefault());
        }
    }
}
