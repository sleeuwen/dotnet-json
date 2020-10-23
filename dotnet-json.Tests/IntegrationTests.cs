using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace dotnet_json.Tests
{
    public class IntegrationTests : IDisposable
    {
        private string _tmpDir;

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
            await File.WriteAllTextAsync(Path.Join(_tmpDir, "set.json"), @"{ ""key"": ""value"" }");

            var (exitCode, console) = await RunCommand(new[]
            {
                "set",
                Path.Join(_tmpDir, "set.json"),
                "path:to:0:key",
                "value",
            });

            exitCode.Should().Be(0);

            var content = await File.ReadAllTextAsync(Path.Join(_tmpDir, "set.json"));
            content.Should().Be(@"{
  ""key"": ""value"",
  ""path"": {
    ""to"": [
      {
        ""key"": ""value""
      }
    ]
  }
}");
        }

        [Fact]
        public async Task Remove()
        {
            await File.WriteAllTextAsync(Path.Join(_tmpDir, "remove.json"), @"{ ""key"": ""value"", ""path"": { ""to"": [ { ""key"": ""value"" } ] } }");

            var (exitCode, console) = await RunCommand(new[]
            {
                "remove",
                Path.Join(_tmpDir, "remove.json"),
                "path:to:0:key",
            });

            exitCode.Should().Be(0);

            var content = await File.ReadAllTextAsync(Path.Join(_tmpDir, "remove.json"));
            content.Should().Be(@"{
  ""key"": ""value"",
  ""path"": {
    ""to"": [
      {}
    ]
  }
}");
        }

        [Fact]
        public async Task Merge()
        {
            await File.WriteAllTextAsync(Path.Join(_tmpDir, "a.json"), "{}");
            await File.WriteAllTextAsync(Path.Join(_tmpDir, "b.json"), @"{ ""b"": { ""key"": ""value"" } }");
            await File.WriteAllTextAsync(Path.Join(_tmpDir, "c.json"), @"{ ""c"": [ 1 ] }");

            var (exitCode, console) = await RunCommand(new[]
            {
                "merge",
                Path.Join(_tmpDir, "a.json"),
                Path.Join(_tmpDir, "b.json"),
                Path.Join(_tmpDir, "c.json"),
                "-o",
                Path.Join(_tmpDir, "d.json"),
            });

            exitCode.Should().Be(0);

            var content = await File.ReadAllTextAsync(Path.Join(_tmpDir, "d.json"));
            content.Should().Be(@"{
  ""b"": {
    ""key"": ""value""
  },
  ""c"": [
    1
  ]
}");
        }

        [Fact]
        public async Task Get()
        {
            await File.WriteAllTextAsync(Path.Join(_tmpDir, "a.json"), @"{""key"": ""value""}");

            var (exitCode, console) = await RunCommand(new[]
            {
                "get",
                Path.Join(_tmpDir, "a.json"),
                "key",
            });

            exitCode.Should().Be(0);

            console.Error.ToString().Should().BeEmpty();
            console.Out.ToString().Should().Be("value\n");
        }

        private async Task<(int ExitCode, IConsole Console)> RunCommand(string[] args)
        {
            var rootCommand = Program.CreateRootCommand();

            var console = new TestConsole();

            var exitCode = await rootCommand.InvokeAsync(args, console);

            return (exitCode, console);
        }
    }
}
