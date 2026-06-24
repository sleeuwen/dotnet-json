using System.CommandLine;
using System.CommandLine.IO;
using dotnet_json.Commands;
using Xunit;

namespace dotnet_json.Tests;

public class FileArgumentTests
{
    [Fact]
    public async Task SucceedsWhenFileExists()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tmpDir);

        try
        {
            var filename = Path.Combine(tmpDir, "file.json");
            await File.WriteAllTextAsync(Path.Combine(tmpDir, filename), "{}", TestContext.Current.CancellationToken);

            var (exitCode, console) = await RunCommand(filename);

            Assert.Equal(0, exitCode);
            Assert.Empty(console.Error.ToString() ?? "");
            Assert.Contains("Success", console.Out.ToString());
        }
        finally
        {
            Directory.Delete(tmpDir, true);
        }
    }

    [Fact]
    public async Task SucceedsWithStandardInputOutput()
    {
        var (exitCode, console) = await RunCommand("-");

        Assert.Equal(0, exitCode);
        Assert.Empty(console.Error.ToString() ?? "");
        Assert.Contains("Success", console.Out.ToString());
    }

    [Fact]
    public async Task ThrowsWhenFileDoesNotExist()
    {
        var (exitCode, console) = await RunCommand("this-file-does-not-exist.json");

        Assert.NotEqual(0, exitCode);
        Assert.Contains("File does not exist: this-file-does-not-exist.json", console.Error.ToString());
    }

    [Fact]
    public async Task DoesNotThrowOnNonExistingFileIfAllowNewFileIsTrue()
    {
        var (exitCode, console) = await RunCommand("this-file-does-not-exist.json", allowNewFile: true);

        Assert.Equal(0, exitCode);
        Assert.Empty(console.Error.ToString() ?? "");
        Assert.Contains("Success", console.Out.ToString());
    }

    private static async Task<(int ExitCode, IConsole Console)> RunCommand(string filename, bool allowNewFile = false)
    {
        var file = new FileArgument("file");
        file.AllowNewFile = allowNewFile;

        var command = new RootCommand();
        command.AddArgument(file);

        var console = new TestConsole();

        command.SetHandler((string file) =>
        {
            console.Out.Write("Success " + file);
        }, file);

        var exitCode = await command.InvokeAsync(filename, console);
        return (exitCode, console);
    }
}
