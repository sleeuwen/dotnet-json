using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.IO;
using System.Threading.Tasks;
using dotnet_json.Commands;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace dotnet_json.Tests.Commands
{
    public class SetCommandTests : IDisposable
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
        [InlineData(@"{""key"": ""value""}", "key", "test")]
        [InlineData(@"{""key"": ""value""}", "key", "")]
        public async Task SavesNewValueToFile(string json, string key, string value)
        {
            await File.WriteAllTextAsync(Path.Join(_tmpDir, "test.json"), json);

            var (exitCode, _) = await RunCommand(Path.Join(_tmpDir, "test.json"), key, value);

            exitCode.Should().Be(0);

            var contents = await File.ReadAllTextAsync(Path.Join(_tmpDir, "test.json"));
            var result = JObject.Parse(contents)[key].ToString();

            result.Should().Be(value);
        }

        [Fact]
        public async Task Existing_DoesNotChangeFileIfKeyDoesNotExist()
        {
            var json = @"{""key1"":""value""}";
            var filename = Path.Join(_tmpDir, "test.json");
            await File.WriteAllTextAsync(filename, json);

            var (exitCode, output) = await RunCommand(filename, "key2", "newvalue", "--compressed", "--existing");

            exitCode.Should().Be(0);

            var contents = await File.ReadAllTextAsync(filename);
            contents.Should().Be(json);
        }

        [Fact]
        public async Task Existing_UpdatesValueIfKeyDoesExist()
        {
            var json = @"{""key1"":""value"",""key2"":""value""}";
            var filename = Path.Join(_tmpDir, "test.json");
            await File.WriteAllTextAsync(filename, json);

            var (exitCode, output) = await RunCommand(filename, "key2", "newvalue", "--compressed", "--existing");

            exitCode.Should().Be(0);

            var contents = await File.ReadAllTextAsync(filename);
            contents.Should().Be(@"{""key1"":""value"",""key2"":""newvalue""}");
        }

        private async Task<(int exitCode, IConsole console)> RunCommand(params string[] arguments)
        {
            var command = new SetCommand();

            var console = new TestConsole();

            var exitCode = await command.InvokeAsync(arguments, console);

            return (exitCode, console);
        }
    }
}
