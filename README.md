# dotnet-json

[![Nuget](https://img.shields.io/nuget/v/dotnet-json)](https://www.nuget.org/packages/dotnet-json/)

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
  remove, rm <file> <key>     Remove a value from the json
  get <file> <key>            Read a value from a JSON file.
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
  <file>     The JSON file (use '-' for STDIN)
  <files>    The names of the files to merge with the first file.

Options:
  -o, --output <file>    The output file (use '-' for STDOUT, defaults to <file>)
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
  <file>     The JSON file (use '-' for STDIN)
  <key>      The key to set (use ':' to set nested object and use index numbers to set array values eg. nested:key or nested:1:key)
  <value>    The value to set

Options:
  -o, --output <file>    The output file (use '-' for STDOUT, defaults to <file>)
  -?, -h, --help         Show help and usage information
```

#### remove

Updates the json file to remove a key from the file. Use `:` as separator for nested objects.

```
remove:
  Remove a value from the json

Usage:
  dotnet-json remove [options] <file> <key>

Arguments:
  <file>    The JSON file (use '-' for STDIN)
  <key>     The JSON key to remove

Options:
  -o, --output <file>    The output file (use '-' for STDOUT, defaults to <file>)
  -?, -h, --help         Show help and usage information
```

#### get

Reads the json file and returns the value for the given key.

```
get:
  Read a value from a JSON file.

Usage:
  dotnet-json get [options] <file> <key>

Arguments:
  <file>    The JSON file (use '-' for STDIN)
  <key>     The key to get (use ':' to get a nested object and use index numbers to get array values eg. nested:key or nested:1:key)

Options:
  -e, --exact       only return exact value matches, this will return an error for references to nested objects/arrays.
  -?, -h, --help    Show help and usage information
```
