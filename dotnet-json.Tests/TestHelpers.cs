using System.CommandLine;

namespace dotnet_json.Tests;

internal static class TestHelpers
{
    internal static async Task<(int ExitCode, string Out, string Error)> RunCommand(
        Command command, params string[] args)
    {
        var outWriter = new StringWriter();
        var errWriter = new StringWriter();
        var config = new InvocationConfiguration { Output = outWriter, Error = errWriter };
        var exitCode = await command.Parse(args).InvokeAsync(config, CancellationToken.None);
        return (exitCode, outWriter.ToString(), errWriter.ToString());
    }
}

