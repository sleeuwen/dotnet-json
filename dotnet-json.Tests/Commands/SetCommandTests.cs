using dotnet_json.Commands;
using Newtonsoft.Json.Linq;
using Xunit;

namespace dotnet_json.Tests.Commands;

public sealed class SetCommandTests : IDisposable
{
    private readonly string _tmpDir;

    public SetCommandTests()
    {
        _tmpDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tmpDir);
    }

    public void Dispose()
    {
        Directory.Delete(_tmpDir, true);
    }

    [Theory]
    [InlineData("""{"key": "value"}""", "key", "test")]
    [InlineData("""{"key": "value"}""", "key", "")]
    public async Task SavesNewValueToFile(string json, string key, string value)
    {
        await File.WriteAllTextAsync(Path.Join(_tmpDir, "test.json"), json, TestContext.Current.CancellationToken);

        var (exitCode, _, _) = await RunCommand(Path.Join(_tmpDir, "test.json"), key, value);

        Assert.Equal(0, exitCode);

        var contents = await File.ReadAllTextAsync(Path.Join(_tmpDir, "test.json"), TestContext.Current.CancellationToken);
        var result = JObject.Parse(contents)[key]!.ToString();

        Assert.Equal(value, result);
    }

    [Fact]
    public async Task Existing_DoesNotChangeFileIfKeyDoesNotExist()
    {
        var json = """{"key1":"value"}""";
        var filename = Path.Join(_tmpDir, "test.json");
        await File.WriteAllTextAsync(filename, json, TestContext.Current.CancellationToken);

        var (exitCode, _, _) = await RunCommand(filename, "key2", "newvalue", "--compressed", "--existing");

        Assert.Equal(0, exitCode);

        var contents = await File.ReadAllTextAsync(filename, TestContext.Current.CancellationToken);
        Assert.Equal(json, contents);
    }

    [Fact]
    public async Task Existing_UpdatesValueIfKeyDoesExist()
    {
        var json = """{"key1":"value","key2":"value"}""";
        var filename = Path.Join(_tmpDir, "test.json");
        await File.WriteAllTextAsync(filename, json, TestContext.Current.CancellationToken);

        var (exitCode, _, _) = await RunCommand(filename, "key2", "newvalue", "--compressed", "--existing");

        Assert.Equal(0, exitCode);

        var contents = await File.ReadAllTextAsync(filename, TestContext.Current.CancellationToken);
        Assert.Equal("""{"key1":"value","key2":"newvalue"}""", contents);
    }

    private static Task<(int exitCode, string Out, string Error)> RunCommand(params string[] arguments)
        => TestHelpers.RunCommand(new SetCommand(), arguments);
}
