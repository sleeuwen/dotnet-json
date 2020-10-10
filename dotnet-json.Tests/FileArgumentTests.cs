using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.IO;
using System.Threading.Tasks;
using dotnet_json.Commands;
using FluentAssertions;
using Xunit;

namespace dotnet_json.Tests
{
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
                await File.WriteAllTextAsync(Path.Combine(tmpDir, filename), "{}");

                var (exitCode, console) = await RunCommand(filename);

                exitCode.Should().Be(0);
                console.Error.ToString().Should().BeEmpty();
                console.Out.ToString().Should().Contain($"Success {filename}");
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

            exitCode.Should().Be(0);
            console.Error.ToString().Should().BeEmpty();
            console.Out.ToString().Should().Contain("Success -");
        }

        [Fact]
        public async Task ThrowsWhenFileDoesNotExist()
        {
            var (exitCode, console) = await RunCommand("this-file-does-not-exist.json");

            exitCode.Should().NotBe(0);
            console.Error.ToString().Should().Contain("File does not exist: this-file-does-not-exist.json");
        }

        private async Task<(int ExitCode, IConsole Console)> RunCommand(string filename)
        {
            var file = new FileArgument("file");

            var command = new RootCommand();
            command.AddArgument(file);

            var console = new TestConsole();

            command.Handler = CommandHandler.Create((string file) =>
            {
                console.Out.Write("Success " + file);
                return 0;
            });

            var exitCode = await command.InvokeAsync(filename, console);
            return (exitCode, console);
        }
    }
}
