# dotnet-json

.NET Core 3.1 global tool for manipulating JSON files.

## Installation

`dotnet tool install -g dotnet-json`

## Usage

```
dotnet-json:
  JSON .NET Global Tool

Usage:
  dotnet-json [options] [command]

Options:
  --version         Show version information
  -?, -h, --help    Show help and usage information

Commands:
  merge <file> <files>        merge two or more json files into one
  set <file> <key> <value>    set a value in a json file
```

### Commands

#### Merge

Merges two or more files into the first, writing it back to the first file or into a specified output file.

```
merge:
  merge two or more json files into one

Usage:
  dotnet-json merge [options] <file> <files>...

Arguments:
  <file>     The name of the first file (use '-' to read from STDIN)
  <files>    The names of the files to merge with the first file.

Options:
  -o, --output <file>    The filename to write the merge result into, leave out to write back into the first file (use '-' to write to STDOUT).
  -?, -h, --help         Show help and usage information
```

#### Set

Updates the json file to set a value for a key. Use `:` as separator for nesting objects.

```
set:
  set a value in a json file

Usage:
  dotnet-json set [options] <file> <key> <value>

Arguments:
  <file>     The JSON file (use '-' to read from STDIN and write to STDOUT)
  <key>      The key to set (use ':' to set nested object and use index numbers to set array values eg. nested:key or nested:1:key)
  <value>    The value to set

Options:
  -?, -h, --help    Show help and usage information
```
