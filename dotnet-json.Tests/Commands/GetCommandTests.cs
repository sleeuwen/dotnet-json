using System.CommandLine;
using System.CommandLine.IO;
using dotnet_json.Commands;
using Xunit;

namespace dotnet_json.Tests.Commands;

public sealed class GetCommandTests : IDisposable
{
    private readonly string _tmpDir;

    public GetCommandTests()
    {
        _tmpDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tmpDir);
    }

    public void Dispose()
    {
        Directory.Delete(_tmpDir, true);
    }

    [Theory]
    [InlineData("""{"key": "value"}""", "key", "value")]
    [InlineData("""{"key": 3.14}""", "key", "3.14")]
    [InlineData("""{"key": null}""", "key", "null")]
    [InlineData("""{"key": true}""", "key", "true")]
    public async Task ReturnsCorrectValue(string json, string key, string expected)
    {
        await File.WriteAllTextAsync(Path.Join(_tmpDir, "a.json"), json, TestContext.Current.CancellationToken);

        var (exitCode, console) = await RunCommand(Path.Join(_tmpDir, "a.json"), key);

        Assert.Equal(0, exitCode);
        Assert.Empty(console.Error.ToString() ?? "");
        Assert.Equal($"{expected}\n", console.Out.ToString());
    }

    [Fact]
    public async Task ReturnsErrorWhenKeyDoesntExist()
    {
        await File.WriteAllTextAsync(Path.Join(_tmpDir, "a.json"), """{"key": "value"}""", TestContext.Current.CancellationToken);

        var (exitCode, console) = await RunCommand(Path.Join(_tmpDir, "a.json"), "value");

        Assert.Equal(1, exitCode);
        Assert.Contains("Key 'value' does not exist in the json", console.Error.ToString());
    }

    [Fact]
    public async Task ReturnsJsonValueWhenKeyIsComplexObject()
    {
        await File.WriteAllTextAsync(Path.Join(_tmpDir, "a.json"), """{"nested": {"key": "value"}}""", TestContext.Current.CancellationToken);

        var (exitCode, console) = await RunCommand(Path.Join(_tmpDir, "a.json"), "nested");

        Assert.Equal(0, exitCode);
        Assert.Empty(console.Error.ToString() ?? "");
        Assert.Equal("""
                     {
                       "key": "value"
                     }

                     """, console.Out.ToString());
    }

    [Fact]
    public async Task ReturnsErrorWhenKeyIsComplexObjectWithExact()
    {
        await File.WriteAllTextAsync(Path.Join(_tmpDir, "a.json"), """{"nested": {"key": "value"}}""", TestContext.Current.CancellationToken);

        var (exitCode, console) = await RunCommand(Path.Join(_tmpDir, "a.json"), "nested", "-e");

        Assert.Equal(1, exitCode);
        Assert.Contains("x", console.Error.ToString());
        Assert.Empty(console.Out.ToString() ?? "");
    }

    private static async Task<(int exitCode, IConsole console)> RunCommand(params string[] arguments)
    {
        var command = new GetCommand();

        var console = new TestConsole();

        var exitCode = await command.InvokeAsync(arguments, console);

        return (exitCode, console);
    }
}
