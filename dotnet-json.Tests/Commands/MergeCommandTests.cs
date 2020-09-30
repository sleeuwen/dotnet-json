using System.Collections.Generic;
using dotnet_json.Commands;
using Newtonsoft.Json.Linq;
using Xunit;

namespace dotnet_json.Tests.Commands
{
    public class MergeCommandTests
    {
        private MergeCommand _command;

        public MergeCommandTests()
        {
            _command = new MergeCommand();
        }

        [Theory]
        [MemberData(nameof(ValuesMatrix))]
        public void MergeValue_MergesTwoValueObjects(object main, object merge)
        {
            var mainValue = new JValue(main);
            var mergeValue = new JValue(merge);

            _command.MergeValue(mainValue, mergeValue);

            Assert.Equal(mainValue.Value, merge);
        }

        public static IEnumerable<object[]> ValuesMatrix()
        {
            var values = new object[] { true, false, 1, 2, "test", "tset", null, null };

            foreach (var value1 in values)
            foreach (var value2 in values)
                if (value1 != value2)
                    yield return new[] { value1, value2 };
        }
    }
}
