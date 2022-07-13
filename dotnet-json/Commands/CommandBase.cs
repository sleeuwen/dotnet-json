using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace dotnet_json.Commands
{
    public abstract class CommandBase : Command, ICommandHandler
    {
        protected FileArgument InputFile = new FileArgument("file", "The JSON file (use '-' for STDIN)");

        protected FileOption OutputFile = new FileOption(new[] { "-o", "--output" }, "The output file (use '-' for STDOUT, defaults to <file>)") { AllowNewFile = true };

        protected Option<bool> Compressed = new Option<bool>(new[] { "-c", "--compressed" }, "Write the output in compressed form (defaults to indented)");

        protected InvocationContext? Context = null;

        protected CommandBase(string name, string? description = null, bool includeOutputOption = true)
            : base(name, description)
        {
            AddArgument(InputFile);

            if (includeOutputOption)
                AddOption(OutputFile);

            Handler = this;
        }

        public int Invoke(InvocationContext context)
        {
            return InvokeAsync(context).GetAwaiter().GetResult();
        }

        public Task<int> InvokeAsync(InvocationContext context)
        {
            Context = context;

            return ExecuteAsync();
        }

        protected Stream GetInputStream()
        {
            var filename = Context?.ParseResult.GetValueForArgument(InputFile) ?? throw new Exception("GetInputStream must be called from a command handler");

            return filename switch
            {
                "-" => Console.OpenStandardInput(),
                _ => File.OpenRead(filename),
            };
        }

        protected Stream GetOutputStream()
        {
            var filename = Context?.ParseResult.HasOption(OutputFile) ?? throw new Exception("GetOutputStream() must be called from a command handler")
                ? Context.ParseResult.GetValueForOption(OutputFile)
                : Context.ParseResult.GetValueForArgument(InputFile);

            return filename switch
            {
                "-" => Console.OpenStandardOutput(),
                _ => File.Create(filename),
            };
        }

        protected Formatting GetFormatting()
        {
            return Context?.ParseResult.HasOption(Compressed) ?? throw new Exception("GetFormatting() must be called from a command handler")
                ? Formatting.None
                : Formatting.Indented;
        }

        [return: MaybeNull]
        protected T GetParameterValue<T>(Argument<T> argument)
        {
            return (Context ?? throw new Exception("GetParameterValue() must be called from a command handler"))
                .ParseResult.GetValueForArgument(argument);
        }

        protected abstract Task<int> ExecuteAsync();
    }
}
