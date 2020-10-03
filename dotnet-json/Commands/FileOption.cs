using System;
using System.CommandLine;

namespace dotnet_json.Commands
{
    public class FileOption : Option<string>
    {
        private bool _initialized = false;

        public FileOption(string alias, string? description = null)
            : base(alias, description)
        {
            base.Argument = new FileArgument();
            _initialized = true;
        }

        public FileOption(string[] aliases, string? description = null)
            : base(aliases, description)
        {
            base.Argument = new FileArgument();
            _initialized = true;
        }

        public override Argument Argument
        {
            set
            {
                if (_initialized && !(value is FileArgument))
                    throw new ArgumentException($"{nameof(Argument)} must be of type {typeof(FileArgument)} but was {value?.GetType().ToString() ?? "null"}");

                base.Argument = value;
            }
        }
    }
}
