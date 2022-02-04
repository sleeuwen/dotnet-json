using System.Collections.Generic;
using System.Linq;
using dotnet_json.Core;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace dotnet_json.Tests
{
    public class JsonDocumentTests
    {
        [Theory]
        [InlineData(null, "key", "key")]
        [InlineData("", "key", "key")]
        [InlineData("nested", "key", "nested:key")]
        public void CreatePrefix_CreatesCorrectPrefix(string current, string value, string expected)
        {
            var prefix = JsonDocument.CreatePrefix(current, value);
            prefix.Should().Be(expected);
        }

        [Theory]
        [MemberData(nameof(ToValueData))]
        public void ToValue_ReturnsCorrectValueObject(JValue jValue, object expected)
        {
            var value = JsonDocument.ToValue(jValue);
            value.Should().Be(expected);
        }

        [Fact]
        public void AllValues_ReturnsSingleValueForJValue()
        {
            var jValue = JValue.Parse("10");

            var allValues = JsonDocument.AllValues(jValue).ToList();

            allValues.Should().HaveCount(1);

            var kv = allValues.Single();
            kv.Key.Should().Be("");
            kv.Value.Value.Should().NotBeNull().And.Be(10);
        }

        [Fact]
        public void AllValues_ReturnsOneForEveryArrayIndex()
        {
            var jArray = JArray.Parse("[1, true, null, \"\"]");

            var allValues = JsonDocument.AllValues(jArray).ToList();

            allValues.Should().HaveCount(4)
                .And.ContainKeys("0", "1", "2", "3");
        }

        [Fact]
        public void AllValues_ReturnsOneForEveryObjectProperty()
        {
            var jObject = JObject.Parse(@"{ ""key"": ""value"", ""other"": 10, ""another"": null }");

            var allValues = JsonDocument.AllValues(jObject).ToList();

            allValues.Should().HaveCount(3)
                .And.ContainKeys("key", "other", "another");
        }

        [Fact]
        public void AllValues_ReturnsOneForEveryNestedKey()
        {
            var jObject = JObject.Parse(@"{ ""key"": ""value"", ""array"": [1, true, { ""another"": ""object"" }] }");

            var allValues = JsonDocument.AllValues(jObject);

            allValues.Should().HaveCount(4)
                .And.ContainKeys("key", "array:0", "array:1", "array:2:another");
        }

        [Theory]
        [MemberData(nameof(FindValueData))]
        public void FindValue_ReturnsCorrectValue(JToken root, string key, JValue expected)
        {
            var document = new JsonDocument(root);
            var actual = document.FindToken(key);

            actual.Should().BeOfType<JValue>()
                .Which.Should<JValue>().Be(expected);
        }

        [Fact]
        public void FindValue_ReturnsNullIfKeyDoesNotExist()
        {
            var document = new JsonDocument(JObject.Parse(@"{ ""key"": ""value"" }"));
            var actual = document.FindToken("notkey");

            actual.Should().BeNull();
        }

        [Fact]
        public void FindValue_ReturnsNullIfKeyDoesNotExist_ButSubkeyExists()
        {
            var document = new JsonDocument(JObject.Parse(@"{ ""key"": ""value"" }"));
            var actual = document.FindToken("key:nested");

            actual.Should().BeNull();
        }

        [Fact]
        public void SetValue_ModifiesOriginalDocument_Object()
        {
            var document = new JsonDocument(JObject.Parse(@"{ ""key"": ""value"", ""nested"": { ""another"": ""value"" } }"));

            document.SetValue("nested:another", "something else");

            document._json.Should().BeOfType<JObject>()
                .Which.Should().ContainKey("nested")
                .WhoseValue.Should().BeOfType<JObject>()
                .Which.Should().ContainKey("another")
                .WhoseValue.Should().BeOfType<JValue>()
                .Which.Value.Should().Be("something else");
        }

        [Fact]
        public void SetValue_ModifiesOriginalDocument_Array()
        {
            var document = new JsonDocument(JObject.Parse(@"{ ""key"": ""value"", ""nested"": { ""another"": ""value"" } }"));

            document.SetValue("nested:another", "something else");

            document._json.Should().BeOfType<JObject>()
                .Which.Should().ContainKey("nested")
                .WhoseValue.Should().BeOfType<JObject>()
                .Which.Should().ContainKey("another")
                .WhoseValue.Should().BeOfType<JValue>()
                .Which.Value.Should().Be("something else");
        }

        [Fact]
        public void SetValue_CreatesNestedStructure_Object()
        {
            var document = new JsonDocument(JObject.Parse(@"{ ""key"": ""value"", ""nested"": { ""another"": ""value"" } }"));

            document.SetValue("object:key", "value");

            document._json.Should().BeOfType<JObject>()
                .Which.Should().ContainKeys("key", "nested", "object");
            document._json["object"].Should().BeOfType<JObject>()
                .Which.Should().ContainKey("key");
            document._json["object"]["key"].Should().BeOfType<JValue>()
                .Which.Value.Should().Be("value");
        }

        [Fact]
        public void SetValue_CreatesNestedStructure_Array()
        {
            var document = new JsonDocument(JArray.Parse(@"[ true, [ false ] ]"));

            document.SetValue("2:0", "value");

            document._json.Should().BeOfType<JArray>()
                .Which.Should().HaveCount(3);

            document._json[2].Should().BeOfType<JArray>()
                .And.HaveCount(1);

            document._json[2][0].Should().BeOfType<JValue>()
                .Which.Value.Should().Be("value");;
        }

        [Fact]
        public void SetValue_ReplacesValue_WithArray()
        {
            var document = new JsonDocument(JObject.Parse(@"{ ""key"": ""value"" }"));

            document.SetValue("key:0", "array");

            document._json.Should().BeOfType<JObject>()
                .Which.Should().ContainKey("key");

            document._json["key"].Should().BeOfType<JArray>()
                .Which.Should().HaveCount(1);

            document._json["key"][0].Should().BeOfType<JValue>()
                .Which.Value.Should().Be("array");
        }

        [Fact]
        public void SetValue_ReplacesValue_WithObject()
        {
            var document = new JsonDocument(JObject.Parse(@"{ ""key"": ""value"" }"));

            document.SetValue("key:nested", "object");

            document._json.Should().BeOfType<JObject>()
                .Which.Should().ContainKey("key");

            document._json["key"].Should().BeOfType<JObject>()
                .Which.Should().ContainKey("nested");

            document._json["key"]["nested"].Should().BeOfType<JValue>()
                .Which.Value.Should().Be("object");
        }

        [Fact]
        public void Merge_MergesTwoDocuments()
        {
            var document1 = new JsonDocument(JObject.Parse(@"{ ""key1"": ""value1"" }"));
            var document2 = new JsonDocument(JObject.Parse(@"{ ""key2"": ""value2"" }"));

            document1.Merge(document2);

            document1._json.Should().BeOfType<JObject>()
                .Which.Should().ContainKeys("key1", "key1");

            document1._json["key1"].Should().BeOfType<JValue>()
                .Which.Value.Should().Be("value1");
            document1._json["key2"].Should().BeOfType<JValue>()
                .Which.Value.Should().Be("value2");
        }

        [Fact(Skip = "not yet implemented")]
        public void Merge_ReplacesWholeArray()
        {
            // TODO: Make sure Merge replaces an array instead of only update array indices.
        }

        [Fact]
        public void SetValue_CreatesNestedStructure_Multi()
        {
            var document = new JsonDocument(JObject.Parse(@"{ ""key"": ""value"", ""nested"": { ""another"": ""value"" } }"));

            document.SetValue("array:0:nested", "value");

            document._json.Should().BeOfType<JObject>()
                .Which.Should().ContainKeys("key", "nested", "array")
                .And.Subject["array"].Should().BeOfType<JArray>()
                .Which.Should().ContainSingle()
                .Which.Should().BeOfType<JObject>()
                .Which.Should().ContainKey("nested")
                .WhoseValue.Should().BeOfType<JValue>()
                .Which.Value.Should().Be("value");
        }

        [Fact]
        public void Remove_ModifiesOriginalDocument_Property()
        {
            var document = new JsonDocument(JObject.Parse(@"{ ""key"": ""value"", ""nested"": { ""another"": ""value"", ""extra"": ""not deleted"" } }"));

            document.Remove("nested:another");

            document._json.Should().BeOfType<JObject>()
                .Which.Should().ContainKey("nested")
                .WhoseValue.Should().BeOfType<JObject>()
                .Which.Should().NotContainKey("another")
                .And.ContainKey("extra");
        }

        [Fact]
        public void Remove_ModifiesOriginalDocument_Array()
        {
            var document = new JsonDocument(JObject.Parse(@"{ ""key"": ""value"", ""nested"": [ ""value"" ] }"));

            document.Remove("nested:0");

            document._json.Should().BeOfType<JObject>()
                .Which.Should().ContainKey("nested")
                .WhoseValue.Should().BeOfType<JArray>()
                .Which.Should().BeEmpty();
        }

        public static IEnumerable<object[]> ToValueData()
        {
            yield return new object[] { JValue.Parse("null"), null };
            yield return new object[] { JValue.Parse("true"), true };
            yield return new object[] { JValue.Parse("false"), false };
            yield return new object[] { JValue.Parse("3.14"), 3.14 };
            yield return new object[] { JValue.Parse("10"), 10 };
            yield return new object[] { JValue.Parse("\"\""), "" };
            yield return new object[] { JValue.Parse("\"stringValue\""), "stringValue" };
        }

        public static IEnumerable<object[]> FindValueData()
        {
            JToken json;

            json = JObject.Parse(@"{ ""key"": ""value"", ""another"": 10 }");
            yield return new object[] { json, "another", (JValue)(json["another"]) };

            json = JArray.Parse(@"[ 1, null, false ]");
            yield return new object[] { json, "1", (JValue)(json[1]) };

            json = JValue.Parse("true");
            yield return new object[] { json, "", (JValue)(json) };

            json = JObject.Parse(@"{ ""key"": ""value"", ""nested"": { ""another"": ""value2"" } }");
            yield return new object[] { json, "nested:another", (JValue)(json["nested"]["another"]) };

            json = JArray.Parse(@"[ 1, [ true ], false ]");
            yield return new object[] { json, "1:0", (JValue)(json[1][0]) };

            json = JObject.Parse(@"{ ""key"": ""value"", ""array"": [ 1, false, null ] }");
            yield return new object[] { json, "array:2", (JValue)(json["array"][2]) };

            json = JArray.Parse(@"[2, false, { ""key"": ""value"" }]");
            yield return new object[] { json, "2:key", (JValue)(json[2]["key"]) };

            json = JObject.Parse(@"{ ""key"": ""value"", ""array"": [ false, { ""key2"": ""value2"" }, null ] }");
            yield return new object[] { json, "array:1:key2", (JValue)(json["array"][1]["key2"]) };

            json = JObject.Parse(@"{ ""key"": ""value"", ""not-array"": { ""0"": true } }");
            yield return new object[] { json, "not-array:0", (JValue)(json["not-array"]["0"]) };
        }
    }
}
