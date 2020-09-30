using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace dotnet_json.Commands
{
    public class MergeCommand : Command, ICommandHandler
    {
        private Argument<string> FileBase = new Argument<string>("file", "main file to merge other files into") { Arity = ArgumentArity.ExactlyOne };

        private Argument<string> Files = new Argument<string>("files", "json files to merge into the main file") { Arity = ArgumentArity.OneOrMore };

        public MergeCommand() : base("merge", "merge two or more json files into one")
        {
            AddArgument(FileBase);
            AddArgument(Files);

            Handler = this;
        }

        public async Task<int> InvokeAsync(InvocationContext context)
        {

            var mainFile = context.ParseResult.ValueForArgument(FileBase);
            var files = context.ParseResult.ValueForArgument<List<string>>(Files);

            await MergeFiles(mainFile, files);

            return 0;
        }

        internal async Task MergeFiles(string mainFile, IEnumerable<string> mergeFiles)
        {
            var content = await ReadFileAsync(mainFile);
            var json = JObject.Parse(content);

            foreach (var file in mergeFiles)
            {
                content = await ReadFileAsync(file);
                var mergeJson = JObject.Parse(content);

                Merge(json, mergeJson);
            }

            await (mainFile switch
            {
                "-" => Console.Out.WriteAsync(json.ToString(Formatting.Indented)),
                _ => File.WriteAllTextAsync(mainFile, json.ToString(Formatting.Indented), new UTF8Encoding(false)),
            });
        }

        internal Task<string> ReadFileAsync(string filename) => filename switch
        {
            "-" => Console.IsInputRedirected ? Console.In.ReadToEndAsync() : Task.FromResult("{}"),
            var file => System.IO.File.ReadAllTextAsync(file),
        };

        internal void Merge(JToken main, JToken merge)
        {
            if (merge is JObject && main is JObject)
                MergeObject((JObject)main, (JObject)merge);

            else if (merge is JArray && main is JArray)
                MergeArray((JArray)main, (JArray)merge);

            else if (merge is JValue && main is JValue)
                MergeValue((JValue)main, (JValue)merge);
        }

        internal void MergeObject(JObject main, JObject merge)
        {
            foreach (var kvp in merge)
            {
                if (!main.ContainsKey(kvp.Key) || main[kvp.Key].Type != kvp.Value.Type)
                    main[kvp.Key] = kvp.Value switch
                    {
                        JObject _ => new JObject(),
                        JArray _ => new JArray(),
                        JValue _ => new JValue((object?)null),
                    };

                Merge(main[kvp.Key], merge[kvp.Key]);
            }
        }

        internal void MergeArray(JArray main, JArray merge)
        {
            var i = 0;
            for (; i < merge.Count; i++)
            {
                if (i >= main.Count)
                    main.Add(new JValue("placeholder"));

                Merge(main[i], merge[i]);
            }

            for (; i < main.Count; i++)
                main.RemoveAt(i);
        }

        internal void MergeValue(JValue main, JValue merge)
        {
            main.Value = merge.Value;
        }
    }
}
