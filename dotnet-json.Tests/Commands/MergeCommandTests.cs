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
    public class MergeCommandTests : IDisposable
    {
        private readonly string _tmpDir;

        public MergeCommandTests()
        {
            _tmpDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(_tmpDir);
        }

        public void Dispose()
        {
            Directory.Delete(_tmpDir, true);
        }

        [Fact]
        public async Task DoesNotLeaveTraceOfPreviousJsonInFile()
        {
            await File.WriteAllTextAsync(Path.Join(_tmpDir, "a.json"), @"{
  // This file uses comments
  ""b"": {
    // To have more lines of JSON
    // then the resulting file
    ""key"": ""value""
  }
  // So to test that it does not leave behind
  // data from the previous file and it still
  // is a valid JSON file after merge
}");
            await File.WriteAllTextAsync(Path.Join(_tmpDir, "b.json"), @"{ ""a"": 1 }");

            var (exitCode, console) = await RunCommand(
                Path.Join(_tmpDir, "a.json"),
                Path.Join(_tmpDir, "b.json"));

            exitCode.Should().Be(0);

            var content = await File.ReadAllTextAsync(Path.Join(_tmpDir, "a.json"));
            content.Should().Be(@"{
  ""b"": {
    ""key"": ""value""
  },
  ""a"": 1
}");
        }

        [Fact]
        public async Task CorrectlyMergesKeysWithColons()
        {
            await File.WriteAllTextAsync(Path.Join(_tmpDir, "a.json"), @"{
    ""Parent:Child"": ""value""
}");

            await File.WriteAllTextAsync(Path.Join(_tmpDir, "b.json"), @"{
    ""Parent:Child"": ""other""
}");

            var (exitCode, console) = await RunCommand(
                Path.Join(_tmpDir, "a.json"),
                Path.Join(_tmpDir, "b.json"));

            exitCode.Should().Be(0);

            var content = await File.ReadAllTextAsync(Path.Join(_tmpDir, "a.json"));
            content.Should().Be(@"{
  ""Parent:Child"": ""other""
}");
        }

        [Fact]
        public async Task KeepsTheFormattingPerKey()
        {
            await File.WriteAllTextAsync(Path.Join(_tmpDir, "a.json"), @"{
    ""Parent:Child"": ""value""
}");

            await File.WriteAllTextAsync(Path.Join(_tmpDir, "b.json"), @"{
    ""Parent"": {
        ""Child"": ""other""
    }
}");

            var (exitCode, console) = await RunCommand(
                Path.Join(_tmpDir, "a.json"),
                Path.Join(_tmpDir, "b.json"));

            exitCode.Should().Be(0);

            var content = await File.ReadAllTextAsync(Path.Join(_tmpDir, "a.json"));
            content.Should().Be(@"{
  ""Parent:Child"": ""other""
}");
        }

        [Fact]
        public async Task AddsKeyInObject()
        {
            await File.WriteAllTextAsync(Path.Join(_tmpDir, "a.json"), @"{
    ""Parent"": {
        ""Other"": ""value""
    },
    ""Parent:Another"": ""value""
}");

            await File.WriteAllTextAsync(Path.Join(_tmpDir, "b.json"), @"{
    ""Parent"": {
        ""Child"": ""other""
    }
}");

            var (exitCode, console) = await RunCommand(
                Path.Join(_tmpDir, "a.json"),
                Path.Join(_tmpDir, "b.json"));

            exitCode.Should().Be(0);

            var content = await File.ReadAllTextAsync(Path.Join(_tmpDir, "a.json"));
            content.Should().Be(@"{
  ""Parent"": {
    ""Other"": ""value"",
    ""Child"": ""other""
  },
  ""Parent:Another"": ""value""
}");
        }

        [Fact]
        public async Task AddsKeyInMostSpecificObject()
        {
            await File.WriteAllTextAsync(Path.Join(_tmpDir, "a.json"), @"{
    ""Parent:Nested"": {
        ""Key"": ""value""
    },
    ""Parent"": {
        ""Nested"": {
            ""Another"": ""value""
        }
    }
}");

            await File.WriteAllTextAsync(Path.Join(_tmpDir, "b.json"), @"{
    ""Parent"": {
        ""Nested"": {
            ""Child"": ""other""
        },
        ""Another:Child"": ""value""
    }
}");

            var (exitCode, console) = await RunCommand(
                Path.Join(_tmpDir, "a.json"),
                Path.Join(_tmpDir, "b.json"));

            exitCode.Should().Be(0);

            var content = await File.ReadAllTextAsync(Path.Join(_tmpDir, "a.json"));
            content.Should().Be(@"{
  ""Parent:Nested"": {
    ""Key"": ""value"",
    ""Child"": ""other""
  },
  ""Parent"": {
    ""Nested"": {
      ""Another"": ""value""
    },
    ""Another"": {
      ""Child"": ""value""
    }
  }
}");
        }

        [Fact]
        public async Task MergeWithArraysWorkCorrectly()
        {
            await File.WriteAllTextAsync(Path.Join(_tmpDir, "a.json"), @"{
  ""Array"": [
    ""item 1"",
    ""item 2""
  ]
}");

            await File.WriteAllTextAsync(Path.Join(_tmpDir, "b.json"), @"{
  ""Array"": [
    ""1 item"",
    null,
    ""3 item""
  ]
}");

            var (exitCode, console) = await RunCommand(
                Path.Join(_tmpDir, "a.json"),
                Path.Join(_tmpDir, "b.json"));

            exitCode.Should().Be(0);

            var content = await File.ReadAllTextAsync(Path.Join(_tmpDir, "a.json"));
            content.Should().Be(@"{
  ""Array"": [
    ""1 item"",
    null,
    ""3 item""
  ]
}");
        }

        [Fact]
        public async Task MergeWithNewArrayWorkCorrectly()
        {
            await File.WriteAllTextAsync(Path.Join(_tmpDir, "a.json"), @"{
}");

            await File.WriteAllTextAsync(Path.Join(_tmpDir, "b.json"), @"{
  ""Array"": [
    ""item 1"",
    ""item 2""
  ]
}");

            var (exitCode, console) = await RunCommand(
                Path.Join(_tmpDir, "a.json"),
                Path.Join(_tmpDir, "b.json"));

            exitCode.Should().Be(0);

            var content = await File.ReadAllTextAsync(Path.Join(_tmpDir, "a.json"));
            content.Should().Be(@"{
  ""Array"": [
    ""item 1"",
    ""item 2""
  ]
}");
        }

        private async Task<(int exitCode, IConsole console)> RunCommand(params string[] args)
        {
            var command = new MergeCommand();

            var console = new TestConsole();

            var exitCode = await command.InvokeAsync(args, console);

            return (exitCode, console);
        }
    }
}
