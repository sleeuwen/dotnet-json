using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using dotnet_json.Core;

namespace dotnet_json.Commands
{
    public class SetCommand : CommandBase, ICommandHandler
    {
        private Argument<string> Key = new Argument<string>("key", "The key to set (use ':' to set nested object and use index numbers to set array values eg. nested:key or nested:1:key)") { Arity = ArgumentArity.ExactlyOne };

        private Argument<string> Value = new Argument<string>("value", "The value to set") { Arity = ArgumentArity.ExactlyOne };

        private Option<bool> Existing = new Option<bool>(new[] { "-e", "--existing" }, "Only set the value if the key already exists in the json file, otherwise do nothing");

        public SetCommand() : base("set", "set a value in a json file")
        {
            AddArgument(Key);
            AddArgument(Value);
            AddOption(Existing);
            AddOption(Compressed);

            Handler = this;
        }

        protected override async Task<int> ExecuteAsync()
        {
            var key = GetParameterValue(Key) ?? throw new ArgumentException("Missing argument <key>");
            var value = GetParameterValue(Value) ?? throw new ArgumentException("Missing argument <value>");
            var existing = Context!.ParseResult.GetValueForOption(Existing);

            JsonDocument document;

            await using (var inputStream = GetInputStream())
                document = JsonDocument.ReadFromStream(inputStream);

            if (existing && document[key] == null)
                return 0;

            document[key] = value;

            await using (var outputStream = GetOutputStream())
                document.WriteToStream(outputStream, GetFormatting());

            return 0;
        }
    }
}
