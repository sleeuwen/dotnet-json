using System.Threading.Tasks;
using dotnet_json.Core;

namespace dotnet_json.Commands
{
    public class IndentCommand : CommandBase
    {
        public IndentCommand() : base("indent", "read a json file and write it out with correct indentation")
        {
        }

        protected override async Task<int> ExecuteAsync()
        {
            JsonDocument document;

            await using (var inputStream = GetInputStream())
                document = JsonDocument.ReadFromStream(inputStream);

            await using (var outputStream = GetOutputStream())
                document.WriteToStream(outputStream);

            return 0;
        }
    }
}
