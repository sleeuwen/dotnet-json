using System;
using System.CommandLine;
using System.Threading.Tasks;
using dotnet_json.Core;
using Newtonsoft.Json.Linq;

namespace dotnet_json.Commands
{
    public class GetCommand : CommandBase
    {
        private Argument<string> Key = new Argument<string>("key", "The key to get (use ':' to get a nested object and use index numbers to get array values eg. nested:key or nested:1:key)") { Arity = ArgumentArity.ExactlyOne };

        private Option<bool> Exact = new Option<bool>(new[] { "-e", "--exact" }, "only return exact value matches, this will return an error for references to nested objects/arrays.");

        public GetCommand()
            : base("get", "Read a value from a JSON file.", false)
        {
            AddArgument(Key);
            AddOption(Exact);
        }

        protected override async Task<int> ExecuteAsync()
        {
            var key = GetParameterValue(Key) ?? throw new ArgumentException("Missing argument <key>");

            JsonDocument document;

            await using (var inputStream = GetInputStream())
                document = JsonDocument.ReadFromStream(inputStream);

            var result = document[key];
            if (result == null)
            {
                Console.Error.WriteLine($"Key '{key}' does not exist in the json");
                return 1;
            }

            if (Context!.ParseResult.ValueForOption(Exact) && !(result is JValue))
            {
                Console.Error.WriteLine($"Value for key '{key}' is a complex object.");
                return 1;
            }

            Console.WriteLine(ToString(result));
            return 0;
        }

        private static string ToString(object obj) => obj switch
        {
            null => "null",
            JValue value when value.Value is null => "null",
            JValue value => ToString(value.Value!),
            bool b => b.ToString().ToLowerInvariant(),
            _ => obj.ToString() ?? "null",
        };
    }
}
