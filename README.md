## dotnet-json

`dotnet-json` is a .NET Core 3.1 global tool for manipulation JSON files.

Install `dotnet-json` with `dotnet tool install -g dotnet-json`

### Commands

#### Merge

`dotnet-json` can merge one or more .json files into a main file:

`dotnet json merge <main> <merge>...`

The merge result is written to the `<main>` file.


#### Set

Use `dotnet json set <file.json> <key> <value>` to set a single value in a json file.
