using System.Linq;
using System.Threading.Tasks;
using dotnet_json.Commands;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace dotnet_json.Tests.Commands
{
    public class SetCommandTests
    {
        private SetCommand _command;

        public SetCommandTests()
        {
            _command = new SetCommand();
        }

        [Fact]
        public async Task Set_Nested()
        {
            var main = new JObject();

            _command.SetValue(main, "Array:0:Nested:0:Key", "Value");

            main.Should().ContainKey("Array")
                .WhichValue.Should().BeOfType<JArray>()
                .Which.Should().HaveCount(1)
                .And.Subject.First().Should().BeOfType<JObject>()
                .Which.Should().ContainKey("Nested")
                .WhichValue.Should().BeOfType<JArray>()
                .Which.Should().HaveCount(1)
                .And.Subject.First().Should().BeOfType<JObject>()
                .Which.Should().ContainKey("Key")
                .WhichValue.Should().BeOfType<JValue>()
                .Which.Value.Should().Be("Value");
        }

        [Fact]
        public async Task Set_Array()
        {
            var main = new JObject();

            _command.SetValue(main, "Array:0", "value");

            main.Should().ContainKey("Array")
                .WhichValue.Should().BeOfType<JArray>()
                .Which.Should().HaveCount(1)
                .And.Subject.First().Should().BeOfType<JValue>()
                .Which.Value.Should().Be("value");
        }

        [Fact]
        public async Task Set_Object()
        {
            var main = new JObject();

            _command.SetValue(main, "Object:Key", "value");

            main.Should().ContainKey("Object")
                .WhichValue.Should().BeOfType<JObject>()
                .Which.Should().ContainKey("Key")
                .WhichValue.Should().BeOfType<JValue>()
                .Which.Value.Should().Be("value");
        }

        [Fact]
        public async Task Set_NewValue()
        {
            var main = new JObject();

            _command.SetValue(main, "doesn't exist yet", "something");

            main.Should().ContainKey("doesn't exist yet")
                .WhichValue.Should().BeOfType<JValue>()
                .Which.Value.Should().Be("something");
        }

        [Fact]
        public async Task Set_ExistingValue()
        {
            var main = new JObject();

            _command.SetValue(main, "value", "something something");

            main.Should().ContainKey("value")
                .WhichValue.Should().BeOfType<JValue>()
                .Which.Value.Should().Be("something something");
        }

        [Fact]
        public async Task Set_string()
        {
            var main = new JObject();

            _command.SetValue(main, "value", "stringValue");

            main.Should().ContainKey("value")
                .WhichValue.Should().BeOfType<JValue>()
                .Which.Value.Should().Be("stringValue");
        }

        [Fact]
        public async Task Set_long()
        {
            var main = new JObject();

            _command.SetValue(main, "value", "2");

            main.Should().ContainKey("value")
                .WhichValue.Should().BeOfType<JValue>()
                .Which.Value.Should().Be(2L);
        }

        [Fact]
        public async Task Set_decimal()
        {
            var main = new JObject();

            _command.SetValue(main, "value", "3.14");

            main.Should().ContainKey("value")
                .WhichValue.Should().BeOfType<JValue>()
                .Which.Value.Should().Be(3.14M);
        }

        [Fact]
        public async Task Set_boolean()
        {
            var main = new JObject();

            _command.SetValue(main, "value", "true");

            main.Should().ContainKey("value")
                .WhichValue.Should().BeOfType<JValue>()
                .Which.Value.Should().Be(true);
        }

        [Fact]
        public async Task Set_null()
        {
            var main = new JObject();

            _command.SetValue(main, "value", "null");

            main.Should().ContainKey("value")
                .WhichValue.Should().BeOfType<JValue>()
                .Which.Value.Should().BeNull();
        }
    }
}
