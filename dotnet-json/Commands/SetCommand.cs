using System;
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
        private Argument<string> File = new Argument<string>("file", "The JSON file  (use '-' to read from STDIN and write to STDOUT)") { Arity = ArgumentArity.ExactlyOne };

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
            var value = context.ParseResult.ValueForArgument(Value);

            var content = file switch
            {
                "-" => await Console.In.ReadToEndAsync(),
                _ => await System.IO.File.ReadAllTextAsync(file),
            };
            var json = JObject.Parse(content);

            SetValue(json, key, value);

            await (file switch
            {
                "-" => Console.Out.WriteAsync(json.ToString(Formatting.Indented)),
                _ => System.IO.File.WriteAllTextAsync(file, json.ToString(Formatting.Indented), new UTF8Encoding(false)),
            });
            return 0;
        }

        internal void SetValue(JToken json, string key, string value)
        {
            var idx = key.IndexOf(':');

            var isIndex = int.TryParse(idx < 0 ? key : key.Substring(0, idx), out var index) && index >= 0;

            if (idx < 0)
            {
                if (json is JArray jArray && isIndex)
                    jArray.Insert(index, ToJValue(value));
                else if (json is JObject)
                    json[key] = ToJValue(value);
                else
                    throw new Exception($"Cannot set value of '{key}' as the expected types are not the same (Expected {(isIndex ? "array" : "object")}, got {GetType(json)}).");

                return;
            }

            var subkey = key.Substring(0, idx);
            var nextKey = key.Substring(idx + 1);

            JToken? subjson = null;
            if (isIndex && json is JArray jsonArray)
                subjson = jsonArray.Count > index ? jsonArray[index] : null;
            else if (json is JObject jsonObject)
                subjson = jsonObject[subkey];
            else
                throw new Exception($"Cannot set value of '{key}' as the expected types are not the same (Expected {(isIndex ? "array" : "object")}, got {GetType(json)}).");

            if (subjson == null)
            {
                var nextIndex = nextKey.IndexOf(':');
                var nextIsIndex = int.TryParse(nextKey.Substring(0, nextIndex < 0 ? nextKey.Length : nextIndex), out _);

                if (json is JArray jArray)
                    jArray.Insert(index, subjson = nextIsIndex ? (JToken)new JArray() : new JObject());
                else
                    json[subkey] = subjson = nextIsIndex ? (JToken)new JArray() : new JObject();
            }

            SetValue(subjson, nextKey, value);
        }

        private static JValue ToJValue(string value)
            => value switch
            {
                var n when n.ToLowerInvariant() == "null" => new JValue((object?)null),
                var b when bool.TryParse(b, out var boolean) => new JValue(boolean),
                var i when long.TryParse(i, out var digit) => new JValue(digit),
                var d when decimal.TryParse(d, out var number) => new JValue(number),
                var str => new JValue(str),
            };

        private static string GetType(JToken token) => token switch
        {
            JObject _ => "object",
            JArray _ => "array",
            JValue _ => "value",
        };
    }
}
