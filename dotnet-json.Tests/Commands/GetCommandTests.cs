using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.IO;
using System.Threading.Tasks;
using dotnet_json.Commands;
using FluentAssertions;
using Xunit;

namespace dotnet_json.Tests.Commands
{
    public class GetCommandTests : IDisposable
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
        [InlineData(@"{""key"": ""value""}", "key", "value")]
        [InlineData(@"{""key"": 3.14}", "key", "3.14")]
        [InlineData(@"{""key"": null}", "key", "null")]
        [InlineData(@"{""key"": true}", "key", "true")]
        public async Task ReturnsCorrectValue(string json, string key, string expected)
        {
            await File.WriteAllTextAsync(Path.Join(_tmpDir, "a.json"), json);

            var (exitCode, console) = await RunCommand(Path.Join(_tmpDir, "a.json"), key);

            exitCode.Should().Be(0);
            console.Error.ToString().Should().BeEmpty();
            console.Out.ToString().Should().Be($"{expected}\n");
        }

        [Fact]
        public async Task ReturnsErrorWhenKeyDoesntExist()
        {
            await File.WriteAllTextAsync(Path.Join(_tmpDir, "a.json"), @"{""key"": ""value""}");

            var (exitCode, console) = await RunCommand(Path.Join(_tmpDir, "a.json"), "value");

            exitCode.Should().Be(1);
            console.Error.ToString().Should().Contain("Key 'value' does not exist in the json");
        }

        [Fact]
        public async Task ReturnsJsonValueWhenKeyIsComplexObject()
        {
            await File.WriteAllTextAsync(Path.Join(_tmpDir, "a.json"), @"{""nested"": {""key"": ""value""}}");

            var (exitCode, console) = await RunCommand(Path.Join(_tmpDir, "a.json"), "nested");

            exitCode.Should().Be(0);
            console.Error.ToString().Should().BeEmpty();
            console.Out.ToString().Should().Be("{\n  \"key\": \"value\"\n}\n");
        }

        [Fact]
        public async Task ReturnsErrorWhenKeyIsComplexObjectWithExact()
        {
            await File.WriteAllTextAsync(Path.Join(_tmpDir, "a.json"), @"{""nested"": {""key"": ""value""}}");

            var (exitCode, console) = await RunCommand(Path.Join(_tmpDir, "a.json"), "nested", "-e");

            exitCode.Should().Be(1);
            console.Error.ToString().Should().Contain("x");
            console.Out.ToString().Should().BeEmpty();
        }

        private async Task<(int exitCode, IConsole console)> RunCommand(params string[] arguments)
        {
            var command = new GetCommand();

            var console = new TestConsole();

            var exitCode = await command.InvokeAsync(arguments, console);

            return (exitCode, console);
        }
    }
}
