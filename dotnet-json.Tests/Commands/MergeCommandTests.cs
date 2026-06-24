using dotnet_json.Commands;
using Xunit;

namespace dotnet_json.Tests.Commands;

public sealed class MergeCommandTests : IDisposable
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
        await File.WriteAllTextAsync(Path.Join(_tmpDir, "a.json"), """
                                                                   {
                                                                     // This file uses comments
                                                                     "b": {
                                                                       // To have more lines of JSON
                                                                       // then the resulting file
                                                                       "key": "value"
                                                                     }
                                                                     // So to test that it does not leave behind
                                                                     // data from the previous file and it still
                                                                     // is a valid JSON file after merge
                                                                   }
                                                                   """, TestContext.Current.CancellationToken);
        await File.WriteAllTextAsync(Path.Join(_tmpDir, "b.json"), """{ "a": 1 }""", TestContext.Current.CancellationToken);

        var (exitCode, _, _) = await RunCommand(
            Path.Join(_tmpDir, "a.json"),
            Path.Join(_tmpDir, "b.json"));

        Assert.Equal(0, exitCode);

        var content = await File.ReadAllTextAsync(Path.Join(_tmpDir, "a.json"), TestContext.Current.CancellationToken);
        Assert.Equal("""
                     {
                       "b": {
                         "key": "value"
                       },
                       "a": 1
                     }
                     """, content);
    }

    [Fact]
    public async Task CorrectlyMergesKeysWithColons()
    {
        await File.WriteAllTextAsync(Path.Join(_tmpDir, "a.json"), """
                                                                   {
                                                                       "Parent:Child": "value"
                                                                   }
                                                                   """, TestContext.Current.CancellationToken);

        await File.WriteAllTextAsync(Path.Join(_tmpDir, "b.json"), """
                                                                   {
                                                                       "Parent:Child": "other"
                                                                   }
                                                                   """, TestContext.Current.CancellationToken);

        var (exitCode, _, _) = await RunCommand(
            Path.Join(_tmpDir, "a.json"),
            Path.Join(_tmpDir, "b.json"));

        Assert.Equal(0, exitCode);

        var content = await File.ReadAllTextAsync(Path.Join(_tmpDir, "a.json"), TestContext.Current.CancellationToken);
        Assert.Equal("""
                     {
                       "Parent:Child": "other"
                     }
                     """, content);
    }

    [Fact]
    public async Task KeepsTheFormattingPerKey()
    {
        await File.WriteAllTextAsync(Path.Join(_tmpDir, "a.json"), """
                                                                   {
                                                                       "Parent:Child": "value"
                                                                   }
                                                                   """, TestContext.Current.CancellationToken);

        await File.WriteAllTextAsync(Path.Join(_tmpDir, "b.json"), """
                                                                   {
                                                                       "Parent": {
                                                                           "Child": "other"
                                                                       }
                                                                   }
                                                                   """, TestContext.Current.CancellationToken);

        var (exitCode, _, _) = await RunCommand(
            Path.Join(_tmpDir, "a.json"),
            Path.Join(_tmpDir, "b.json"));

        Assert.Equal(0, exitCode);

        var content = await File.ReadAllTextAsync(Path.Join(_tmpDir, "a.json"), TestContext.Current.CancellationToken);
        Assert.Equal("""
                     {
                       "Parent:Child": "other"
                     }
                     """, content);
    }

    [Fact]
    public async Task AddsKeyInObject()
    {
        await File.WriteAllTextAsync(Path.Join(_tmpDir, "a.json"), """
                                                                   {
                                                                       "Parent": {
                                                                           "Other": "value"
                                                                       },
                                                                       "Parent:Another": "value"
                                                                   }
                                                                   """, TestContext.Current.CancellationToken);

        await File.WriteAllTextAsync(Path.Join(_tmpDir, "b.json"), """
                                                                   {
                                                                       "Parent": {
                                                                           "Child": "other"
                                                                       }
                                                                   }
                                                                   """, TestContext.Current.CancellationToken);

        var (exitCode, _, _) = await RunCommand(
            Path.Join(_tmpDir, "a.json"),
            Path.Join(_tmpDir, "b.json"));

        Assert.Equal(0, exitCode);

        var content = await File.ReadAllTextAsync(Path.Join(_tmpDir, "a.json"), TestContext.Current.CancellationToken);
        Assert.Equal("""
                     {
                       "Parent": {
                         "Other": "value",
                         "Child": "other"
                       },
                       "Parent:Another": "value"
                     }
                     """, content);
    }

    [Fact]
    public async Task AddsKeyInMostSpecificObject()
    {
        await File.WriteAllTextAsync(Path.Join(_tmpDir, "a.json"), """
                                                                   {
                                                                       "Parent:Nested": {
                                                                           "Key": "value"
                                                                       },
                                                                       "Parent": {
                                                                           "Nested": {
                                                                               "Another": "value"
                                                                           }
                                                                       }
                                                                   }
                                                                   """, TestContext.Current.CancellationToken);

        await File.WriteAllTextAsync(Path.Join(_tmpDir, "b.json"), """
                                                                   {
                                                                       "Parent": {
                                                                           "Nested": {
                                                                               "Child": "other"
                                                                           },
                                                                           "Another:Child": "value"
                                                                       }
                                                                   }
                                                                   """, TestContext.Current.CancellationToken);

        var (exitCode, _, _) = await RunCommand(
            Path.Join(_tmpDir, "a.json"),
            Path.Join(_tmpDir, "b.json"));

        Assert.Equal(0, exitCode);

        var content = await File.ReadAllTextAsync(Path.Join(_tmpDir, "a.json"), TestContext.Current.CancellationToken);
        Assert.Equal("""
                     {
                       "Parent:Nested": {
                         "Key": "value",
                         "Child": "other"
                       },
                       "Parent": {
                         "Nested": {
                           "Another": "value"
                         },
                         "Another": {
                           "Child": "value"
                         }
                       }
                     }
                     """, content);
    }

    [Fact]
    public async Task MergeWithArraysWorkCorrectly()
    {
        await File.WriteAllTextAsync(Path.Join(_tmpDir, "a.json"), """
                                                                   {
                                                                     "Array": [
                                                                       "item 1",
                                                                       "item 2"
                                                                     ]
                                                                   }
                                                                   """, TestContext.Current.CancellationToken);

        await File.WriteAllTextAsync(Path.Join(_tmpDir, "b.json"), """
                                                                   {
                                                                     "Array": [
                                                                       "1 item",
                                                                       null,
                                                                       "3 item"
                                                                     ]
                                                                   }
                                                                   """, TestContext.Current.CancellationToken);

        var (exitCode, _, _) = await RunCommand(
            Path.Join(_tmpDir, "a.json"),
            Path.Join(_tmpDir, "b.json"));

        Assert.Equal(0, exitCode);

        var content = await File.ReadAllTextAsync(Path.Join(_tmpDir, "a.json"), TestContext.Current.CancellationToken);
        Assert.Equal("""
                     {
                       "Array": [
                         "1 item",
                         null,
                         "3 item"
                       ]
                     }
                     """, content);
    }

    [Fact]
    public async Task MergeWithNewArrayWorkCorrectly()
    {
        await File.WriteAllTextAsync(Path.Join(_tmpDir, "a.json"), """
                                                                   {
                                                                   }
                                                                   """, TestContext.Current.CancellationToken);

        await File.WriteAllTextAsync(Path.Join(_tmpDir, "b.json"), """
                                                                   {
                                                                     "Array": [
                                                                       "item 1",
                                                                       "item 2"
                                                                     ]
                                                                   }
                                                                   """, TestContext.Current.CancellationToken);

        var (exitCode, _, _) = await RunCommand(
            Path.Join(_tmpDir, "a.json"),
            Path.Join(_tmpDir, "b.json"));

        Assert.Equal(0, exitCode);

        var content = await File.ReadAllTextAsync(Path.Join(_tmpDir, "a.json"), TestContext.Current.CancellationToken);
        Assert.Equal("""
                     {
                       "Array": [
                         "item 1",
                         "item 2"
                       ]
                     }
                     """, content);
    }

    private static Task<(int exitCode, string Out, string Error)> RunCommand(params string[] args)
        => TestHelpers.RunCommand(new MergeCommand(), args);
}
