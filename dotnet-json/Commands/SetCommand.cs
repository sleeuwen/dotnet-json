using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace dotnet_json.Commands
{
    public class SetCommand : Command, ICommandHandler
    {
        private Argument<FileInfo> File = new Argument<FileInfo>("file", "The JSON file") { Arity = ArgumentArity.ExactlyOne }
            .ExistingOnly();

        private Argument<string> Key = new Argument<string>("key", "The key to set (use ':' to set nested object and use index numbers to set array values eg. nested:key or nested:1:key)") { Arity = ArgumentArity.ExactlyOne };

        private Argument<string> Value = new Argument<string>("value", "The value to set") { Arity = ArgumentArity.ExactlyOne };

        public SetCommand() : base("set", "set a value in a json file")
        {
            AddArgument(File);
            AddArgument(Key);
            AddArgument(Value);

            Handler = this;
        }

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            var file = context.ParseResult.ValueForArgument(File);
            var key = context.ParseResult.ValueForArgument(Key);
            object value = context.ParseResult.ValueForArgument(Value);

            var content = await System.IO.File.ReadAllTextAsync(file.FullName);
            var json = JObject.Parse(content);

            if (int.TryParse((string)value, out var intValue))
                value = intValue;

            SetValue(json, key, value);

            await System.IO.File.WriteAllTextAsync(file.FullName, json.ToString(Formatting.Indented), new UTF8Encoding(false));
            return 0;
        }

        internal void SetValue(JToken json, string key, object value)
        {
            var idx = key.IndexOf(':');


            if (idx < 0)
            {
                json[key] = new JValue(value);
                return;
            }

            var subkey = key.Substring(0, idx);
            var nextKey = key.Substring(idx + 1);
            var subjson = json[subkey];

            if (subjson == null)
            {
                json[subkey] = subjson = new JObject();
            }

            SetValue(subjson, nextKey, value);
        }
    }
}
