using Xunit;

namespace dotnet_json.Tests;

public sealed class IntegrationTests : IDisposable
{
    private readonly string _tmpDir;

    public IntegrationTests()
    {
        _tmpDir = Path.Join(Path.GetTempPath(), Path.GetTempFileName());
        Directory.CreateDirectory(_tmpDir);
    }

    public void Dispose()
    {
        Directory.Delete(_tmpDir, true);
    }

    [Fact]
    public async Task Set()
    {
        await File.WriteAllTextAsync(Path.Join(_tmpDir, "set.json"), """{ "key": "value" }""", TestContext.Current.CancellationToken);

        var (exitCode, _, _) = await RunCommand([
            "set",
            Path.Join(_tmpDir, "set.json"),
            "path:to:0:key",
            "value"
        ]);

        Assert.Equal(0, exitCode);

        var content = await File.ReadAllTextAsync(Path.Join(_tmpDir, "set.json"), TestContext.Current.CancellationToken);
        Assert.Equal("""
                     {
                       "key": "value",
                       "path": {
                         "to": [
                           {
                             "key": "value"
                           }
                         ]
                       }
                     }
                     """, content);
    }

    [Fact]
    public async Task Remove()
    {
        await File.WriteAllTextAsync(Path.Join(_tmpDir, "remove.json"), """{ "key": "value", "path": { "to": [ { "key": "value" } ] } }""", TestContext.Current.CancellationToken);

        var (exitCode, _, _) = await RunCommand([
            "remove",
            Path.Join(_tmpDir, "remove.json"),
            "path:to:0:key"
        ]);

        Assert.Equal(0, exitCode);

        var content = await File.ReadAllTextAsync(Path.Join(_tmpDir, "remove.json"), TestContext.Current.CancellationToken);
        Assert.Equal("""
                     {
                       "key": "value",
                       "path": {
                         "to": [
                           {}
                         ]
                       }
                     }
                     """, content);
    }

    [Fact]
    public async Task Merge()
    {
        await File.WriteAllTextAsync(Path.Join(_tmpDir, "a.json"), "{}", TestContext.Current.CancellationToken);
        await File.WriteAllTextAsync(Path.Join(_tmpDir, "b.json"), """{ "b": { "key": "value" } }""", TestContext.Current.CancellationToken);
        await File.WriteAllTextAsync(Path.Join(_tmpDir, "c.json"), """{ "c": [ 1 ] }""", TestContext.Current.CancellationToken);

        var (exitCode, _, _) = await RunCommand([
            "merge",
            Path.Join(_tmpDir, "a.json"),
            Path.Join(_tmpDir, "b.json"),
            Path.Join(_tmpDir, "c.json"),
            "-o",
            Path.Join(_tmpDir, "d.json")
        ]);

        Assert.Equal(0, exitCode);

        var content = await File.ReadAllTextAsync(Path.Join(_tmpDir, "d.json"), TestContext.Current.CancellationToken);
        Assert.Equal("""
                     {
                       "b": {
                         "key": "value"
                       },
                       "c": [
                         1
                       ]
                     }
                     """, content);
    }

    [Fact]
    public async Task Get()
    {
        await File.WriteAllTextAsync(Path.Join(_tmpDir, "a.json"), """{"key": "value"}""", TestContext.Current.CancellationToken);

        var (exitCode, output, error) = await RunCommand([
            "get",
            Path.Join(_tmpDir, "a.json"),
            "key"
        ]);

        Assert.Equal(0, exitCode);
        Assert.Empty(error);
        Assert.Equal("value\n", output);
    }

    private static Task<(int ExitCode, string Out, string Error)> RunCommand(string[] args)
        => TestHelpers.RunCommand(Program.CreateRootCommand(), args);
}
