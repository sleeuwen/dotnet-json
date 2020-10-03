using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using dotnet_json.Core;

namespace dotnet_json.Commands
{
    public class MergeCommand : CommandBase, ICommandHandler
    {
        private FileArgument Files = new FileArgument("files", "The names of the files to merge with the first file.") { Arity = ArgumentArity.OneOrMore };

        public MergeCommand() : base("merge", "merge two or more json files into one")
        {
            AddArgument(Files);

            Handler = this;
        }

        protected override async Task<int> ExecuteAsync()
        {
            var files = GetMultiParameterValue(Files) ?? throw new ArgumentException("Missing argument <files>");

            JsonDocument document;

            await using (var inputStream = GetInputStream())
                document = JsonDocument.ReadFromStream(inputStream);

            foreach (var file in files)
            {
                await using (var stream = GetStream(file))
                {
                    var mergeDocument = JsonDocument.ReadFromStream(stream);
                    document.Merge(mergeDocument);
                }
            }

            await using (var outputStream = GetOutputStream())
                document.WriteToStream(outputStream, GetFormatting());

            return 0;
        }

        private Stream GetStream(string filename) => filename switch
        {
            "-" => Console.OpenStandardInput(),
            _ => File.OpenRead(filename),
        };
    }
}
