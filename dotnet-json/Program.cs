using System.CommandLine;
using dotnet_json.Commands;

namespace dotnet_json;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var command = CreateRootCommand();
        return await command.Parse(args).InvokeAsync(new InvocationConfiguration(), CancellationToken.None);
    }

    internal static RootCommand CreateRootCommand()
    {
        var root = new RootCommand("JSON .NET Global Tool");

        root.Subcommands.Add(new MergeCommand());
        root.Subcommands.Add(new SetCommand());
        root.Subcommands.Add(new RemoveCommand());
        root.Subcommands.Add(new GetCommand());
        root.Subcommands.Add(new IndentCommand());

        return root;
    }
}
