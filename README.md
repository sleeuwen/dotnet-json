# dotnet-json

[![nuget](https://img.shields.io/nuget/v/dotnet-json)](https://www.nuget.org/packages/dotnet-json/)

dotnet-json is a command line tool for working with and manipulating JSON files for example in CI/CD pipelines.

dotnet-json allows you to do basic manipulation of JSON files like setting the value of a specific (nested) key, or deleting a key,
as well as merging two or more JSON files into one.

## Installation

dotnet-json can be installed as a .NET Core (global) tool or be downloaded directly from the [GitHub releases](https://github.com/sleeuwen/dotnet-json/releases).

To install dotnet-json as a global tool, run the following command:

```
dotnet tool install -g dotnet-json
```

Or, if you already have `dotnet-json` installed and want to update it to the latest version, run the following command:
```
dotnet tool update -g dotnet-json
```

## Usage

When you installed dotnet-json as a .NET Core global tool, you can run it as either `dotnet json` or `dotnet-json`.

dotnet-json has 5 sub-commands, `merge`, `set`, `remove`, `get` and `indent`.

### Merge

Merges two or more files into the first, writing it back to the first file or into a specified output file.

**Usage:**
```
dotnet json merge <input file> <merge files...> [-o|--output <output file>]
```

**Arguments:**

- _\<input file>_ The first JSON file used as base to merge the other files' contents into, also used as default output
- _\<merge files...>_ One or more JSON files that are merged into the first file

**Options:**
- _-o|--output file_ Write the merge result to a custom output file instead of using the input file 

### Set

Set a specific value in the JSON file. Use `:` as separator for nesting objects.

**Usage:**
```
dotnet json set <file> <key> <value> [-o|--output <output file>]
```

**Arguments:**
- _\<file>_ The file to read the JSON from and write the result to unless `-o` is given.
- _\<key>_ The key to update or create, use `:` to separate nested objects.
- _\<value>_ The value to set the key to

**Options:**
- _-o|--output file_ Write the result to a custom output file instead of using the input file

### remove

Removes a key/value pair or complex object from a JSON file.

**Usage:**
```
dotnet json remove <file> <key> [-o|--output <output file>]
```

**Arguments:**
- _\<file>_ The file to read the JSON from and write the result to unless `-o` is given.
- _\<key>_ The key to remove from the read JSON

**Options:**
- _-o|--output file_ Write the result to a custom output file instead of using the input file

### get

Reads the json file and returns the value for the given key.

**Usage:**
```
dotnet json get <file> <key> [-e|--exact]
```

**Arguments:**
- _\<file>_ The file to read the JSON from.
- _\<key>_ The key to output

**Options:**
- _-e|--exact_ only return an exact value, this will return an error if the key references an object or array.

### indent

Reads the json file and writes it out with indentation.

**Usage:**
```
dotnet json indent <file> [-o|--output <output file>]
```

**Arguments:**
- _\<file>_ The file to read the JSON from and write the result to unless `-o` is given.

**Options:**
- _-o|--output file_ Write the result to a custom output file instead of writing back to the input file
