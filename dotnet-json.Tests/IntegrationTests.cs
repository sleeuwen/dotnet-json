using System;
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

            await Program.Main(new[]
            {
                "set",
                Path.Join(_tmpDir, "set.json"),
                "path:to:0:key",
                "value",
            });

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

            await Program.Main(new[]
            {
                "remove",
                Path.Join(_tmpDir, "remove.json"),
                "path:to:0:key",
            });

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

            await Program.Main(new[]
            {
                "merge",
                Path.Join(_tmpDir, "a.json"),
                Path.Join(_tmpDir, "b.json"),
                Path.Join(_tmpDir, "c.json"),
            });

            var content = await File.ReadAllTextAsync(Path.Join(_tmpDir, "a.json"));
            content.Should().Be(@"{
  ""b"": {
    ""key"": ""value""
  },
  ""c"": [
    1
  ]
}");
        }
    }
}
