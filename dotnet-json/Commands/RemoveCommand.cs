using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using dotnet_json.Core;

namespace dotnet_json.Commands
{
    public class RemoveCommand : CommandBase, ICommandHandler
    {
        private Argument<string> Key = new Argument<string>("key", "The JSON key to remove") { Arity = ArgumentArity.ExactlyOne };

        public RemoveCommand()
            : base("remove", "Remove a value from the json")
        {
            AddArgument(Key);

            AddAlias("rm");

            Handler = this;
        }

        protected override async Task<int> ExecuteAsync()
        {
            var key = GetParameterValue(Key) ?? throw new ArgumentException("Missing argument <key>");

            JsonDocument document;

            await using (var inputStream = GetInputStream())
                document = JsonDocument.ReadFromStream(inputStream);

            document.Remove(key);

            await using (var outputStream = GetOutputStream())
                document.WriteToStream(outputStream, GetFormatting());

            return 0;
        }
    }
}
