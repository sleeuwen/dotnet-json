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

        protected FileOption OutputFile = new FileOption(new[] { "-o", "--output" }, "The output file (use '-' for STDOUT, defaults to <file>)") { Argument = { Name = "file", Arity = ArgumentArity.ZeroOrOne } };

        protected Option<bool> Compressed = new Option<bool>(new[] { "-c", "--compressed" }, "Write the output in compressed form (defaults to indented)");

        private InvocationContext? _context = null;

        protected CommandBase(string name, string? description = null)
            : base(name, description)
        {
            AddArgument(InputFile);
            AddOption(OutputFile);

            Handler = this;
        }

        public Task<int> InvokeAsync(InvocationContext context)
        {
            _context = context;

            return ExecuteAsync();
        }

        protected Stream GetInputStream()
        {
            var filename = _context?.ParseResult.ValueForArgument(InputFile) ?? throw new Exception("GetInputStream must be called from a command handler");

            return filename switch
            {
                "-" => Console.OpenStandardInput(),
                _ => File.OpenRead(filename),
            };
        }

        protected Stream GetOutputStream()
        {
            var filename = _context?.ParseResult.HasOption(OutputFile) ?? throw new Exception("GetOutputStream() must be called from a command handler")
                ? _context.ParseResult.ValueForOption(OutputFile)
                : _context.ParseResult.ValueForArgument(InputFile);

            return filename switch
            {
                "-" => Console.OpenStandardOutput(),
                _ => File.OpenWrite(filename),
            };
        }

        protected Formatting GetFormatting()
        {
            return _context?.ParseResult.HasOption(Compressed) ?? throw new Exception("GetFormatting() must be called from a command handler")
                ? Formatting.None
                : Formatting.Indented;
        }

        [return: MaybeNull]
        protected T GetParameterValue<T>(Argument<T> argument)
        {
            return (_context ?? throw new Exception("GetParameterValue() must be called from a command handler"))
                .ParseResult.ValueForArgument(argument);
        }

        protected List<T>? GetMultiParameterValue<T>(Argument<T> argument)
        {
            return (_context ?? throw new Exception("GetMultiParameterValue() must be called from a command handler"))
                .ParseResult.ValueForArgument<List<T>>(argument);
        }

        protected abstract Task<int> ExecuteAsync();
    }
}
