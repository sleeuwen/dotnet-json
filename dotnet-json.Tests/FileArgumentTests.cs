using System.CommandLine;
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

            var (exitCode, output, error) = await RunCommand(filename);

            Assert.Equal(0, exitCode);
            Assert.Empty(error);
            Assert.Contains("Success", output);
        }
        finally
        {
            Directory.Delete(tmpDir, true);
        }
    }

    [Fact]
    public async Task SucceedsWithStandardInputOutput()
    {
        var (exitCode, output, error) = await RunCommand("-");

        Assert.Equal(0, exitCode);
        Assert.Empty(error);
        Assert.Contains("Success", output);
    }

    [Fact]
    public async Task ThrowsWhenFileDoesNotExist()
    {
        var (exitCode, _, error) = await RunCommand("this-file-does-not-exist.json");

        Assert.NotEqual(0, exitCode);
        Assert.Contains("File does not exist: this-file-does-not-exist.json", error);
    }

    [Fact]
    public async Task DoesNotThrowOnNonExistingFileIfAllowNewFileIsTrue()
    {
        var (exitCode, output, error) = await RunCommand("this-file-does-not-exist.json", allowNewFile: true);

        Assert.Equal(0, exitCode);
        Assert.Empty(error);
        Assert.Contains("Success", output);
    }

    private static Task<(int ExitCode, string Out, string Error)> RunCommand(string filename, bool allowNewFile = false)
    {
        var file = new FileArgument("file");
        file.AllowNewFile = allowNewFile;

        var command = new RootCommand();
        command.Arguments.Add(file);
        command.SetAction((ParseResult parseResult) =>
        {
            parseResult.InvocationConfiguration.Output.Write("Success " + parseResult.GetValue(file));
        });

        return TestHelpers.RunCommand(command, filename);
    }
}
