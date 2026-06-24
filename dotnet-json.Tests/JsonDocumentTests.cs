using dotnet_json.Core;
using Newtonsoft.Json.Linq;
using Xunit;

namespace dotnet_json.Tests;

public class JsonDocumentTests
{
    [Theory]
    [InlineData(null, "key", "key")]
    [InlineData("", "key", "key")]
    [InlineData("nested", "key", "nested:key")]
    public void CreatePrefix_CreatesCorrectPrefix(string? current, string value, string expected)
    {
        var prefix = JsonDocument.CreatePrefix(current, value);
        Assert.Equal(expected, prefix);
    }

    [Theory]
    [MemberData(nameof(ToValueData))]
    public void ToValue_ReturnsCorrectValueObject(JValue jValue, object? expected)
    {
        var value = JsonDocument.ToValue(jValue);
        Assert.Equal(expected, value);
    }

    [Fact]
    public void AllValues_ReturnsSingleValueForJValue()
    {
        var jValue = JToken.Parse("10");

        var allValues = JsonDocument.AllValues(jValue).ToList();

        var kv = Assert.Single(allValues);
        Assert.Equal("", kv.Key);
        Assert.Equal(10L, kv.Value.Value);
    }

    [Fact]
    public void AllValues_ReturnsOneForEveryArrayIndex()
    {
        var jArray = JArray.Parse("[1, true, null, \"\"]");

        var allValues = JsonDocument.AllValues(jArray).ToList();

        Assert.Equal(4, allValues.Count);
        Assert.Contains(allValues, kv => kv.Key == "0");
        Assert.Contains(allValues, kv => kv.Key == "1");
        Assert.Contains(allValues, kv => kv.Key == "2");
        Assert.Contains(allValues, kv => kv.Key == "3");
    }

    [Fact]
    public void AllValues_ReturnsOneForEveryObjectProperty()
    {
        var jObject = JObject.Parse("""{ "key": "value", "other": 10, "another": null }""");

        var allValues = JsonDocument.AllValues(jObject).ToList();

        Assert.Equal(3, allValues.Count);
        Assert.Contains(allValues, kv => kv.Key == "key");
        Assert.Contains(allValues, kv => kv.Key == "other");
        Assert.Contains(allValues, kv => kv.Key == "another");
    }

    [Fact]
    public void AllValues_ReturnsOneForEveryNestedKey()
    {
        var jObject = JObject.Parse("""{ "key": "value", "array": [1, true, { "another": "object" }] }""");

        var allValues = JsonDocument.AllValues(jObject).ToList();

        Assert.Equal(4, allValues.Count);
        Assert.Contains(allValues, kv => kv.Key == "key");
        Assert.Contains(allValues, kv => kv.Key == "array:0");
        Assert.Contains(allValues, kv => kv.Key == "array:1");
        Assert.Contains(allValues, kv => kv.Key == "array:2:another");
    }

    [Theory]
    [MemberData(nameof(FindValueData))]
    public void FindValue_ReturnsCorrectValue(JToken root, string key, JValue expected)
    {
        var document = new JsonDocument(root);
        var actual = document.FindToken(key);

        var jVal = Assert.IsType<JValue>(actual);
        Assert.Equal(expected, jVal);
    }

    [Fact]
    public void FindValue_ReturnsNullIfKeyDoesNotExist()
    {
        var document = new JsonDocument(JObject.Parse("""{ "key": "value" }"""));
        var actual = document.FindToken("notkey");

        Assert.Null(actual);
    }

    [Fact]
    public void FindValue_ReturnsNullIfKeyDoesNotExist_ButSubkeyExists()
    {
        var document = new JsonDocument(JObject.Parse("""{ "key": "value" }"""));
        var actual = document.FindToken("key:nested");

        Assert.Null(actual);
    }

    [Fact]
    public void SetValue_ModifiesOriginalDocument_Object()
    {
        var document = new JsonDocument(JObject.Parse("""{ "key": "value", "nested": { "another": "value" } }"""));

        document.SetValue("nested:another", "something else");

        var jObj = Assert.IsType<JObject>(document._json);
        Assert.True(jObj.ContainsKey("nested"));
        var nestedObj = Assert.IsType<JObject>(jObj["nested"]);
        Assert.True(nestedObj.ContainsKey("another"));
        var anotherVal = Assert.IsType<JValue>(nestedObj["another"]);
        Assert.Equal("something else", anotherVal.Value);
    }

    [Fact]
    public void SetValue_ModifiesOriginalDocument_Array()
    {
        var document = new JsonDocument(JObject.Parse("""{ "key": "value", "nested": { "another": "value" } }"""));

        document.SetValue("nested:another", "something else");

        var jObj = Assert.IsType<JObject>(document._json);
        Assert.True(jObj.ContainsKey("nested"));
        var nestedObj = Assert.IsType<JObject>(jObj["nested"]);
        Assert.True(nestedObj.ContainsKey("another"));
        var anotherVal = Assert.IsType<JValue>(nestedObj["another"]);
        Assert.Equal("something else", anotherVal.Value);
    }

    [Fact]
    public void SetValue_CreatesNestedStructure_Object()
    {
        var document = new JsonDocument(JObject.Parse("""{ "key": "value", "nested": { "another": "value" } }"""));

        document.SetValue("object:key", "value");

        var jObj = Assert.IsType<JObject>(document._json);
        Assert.True(jObj.ContainsKey("key"));
        Assert.True(jObj.ContainsKey("nested"));
        Assert.True(jObj.ContainsKey("object"));
        var objectVal = Assert.IsType<JObject>(document._json["object"]);
        Assert.True(objectVal.ContainsKey("key"));
        var keyVal = Assert.IsType<JValue>(document._json["object"]!["key"]);
        Assert.Equal("value", keyVal.Value);
    }

    [Fact]
    public void SetValue_CreatesNestedStructure_Array()
    {
        var document = new JsonDocument(JArray.Parse(@"[ true, [ false ] ]"));

        document.SetValue("2:0", "value");

        var jArr = Assert.IsType<JArray>(document._json);
        Assert.Equal(3, jArr.Count);
        var inner = Assert.IsType<JArray>(document._json[2]);
        Assert.Single(inner);
        var val = Assert.IsType<JValue>(document._json[2]![0]);
        Assert.Equal("value", val.Value);
    }

    [Fact]
    public void SetValue_ReplacesValue_WithArray()
    {
        var document = new JsonDocument(JObject.Parse("""{ "key": "value" }"""));

        document.SetValue("key:0", "array");

        var jObj = Assert.IsType<JObject>(document._json);
        Assert.True(jObj.ContainsKey("key"));
        var keyArr = Assert.IsType<JArray>(document._json["key"]);
        Assert.Single(keyArr);
        var arrVal = Assert.IsType<JValue>(document._json["key"]![0]);
        Assert.Equal("array", arrVal.Value);
    }

    [Fact]
    public void SetValue_ReplacesValue_WithObject()
    {
        var document = new JsonDocument(JObject.Parse("""{ "key": "value" }"""));

        document.SetValue("key:nested", "object");

        var jObj = Assert.IsType<JObject>(document._json);
        Assert.True(jObj.ContainsKey("key"));
        var keyObj = Assert.IsType<JObject>(document._json["key"]);
        Assert.True(keyObj.ContainsKey("nested"));
        var nestedVal = Assert.IsType<JValue>(document._json["key"]!["nested"]);
        Assert.Equal("object", nestedVal.Value);
    }

    [Fact]
    public void SetValue_ReplacesValue_WithEmptyString()
    {
        var document = new JsonDocument(JObject.Parse("""{ "key": "value" }"""));

        document.SetValue("key", "");

        var jObj = Assert.IsType<JObject>(document._json);
        Assert.True(jObj.ContainsKey("key"));
        var value = Assert.IsType<JValue>(document._json["key"]);
        Assert.Equal(JTokenType.String, value.Type);
        Assert.Equal("", value.Value);
    }

    [Fact]
    public void Merge_MergesTwoDocuments()
    {
        var document1 = new JsonDocument(JObject.Parse("""{ "key1": "value1" }"""));
        var document2 = new JsonDocument(JObject.Parse("""{ "key2": "value2" }"""));

        document1.Merge(document2);

        var jObj = Assert.IsType<JObject>(document1._json);
        Assert.True(jObj.ContainsKey("key1"));
        Assert.True(jObj.ContainsKey("key2"));
        var val1 = Assert.IsType<JValue>(document1._json["key1"]);
        Assert.Equal("value1", val1.Value);
        var val2 = Assert.IsType<JValue>(document1._json["key2"]);
        Assert.Equal("value2", val2.Value);
    }

    [Fact(Skip = "not yet implemented")]
    public void Merge_ReplacesWholeArray()
    {
        // TODO: Make sure Merge replaces an array instead of only update array indices.
    }

    [Fact]
    public void SetValue_CreatesNestedStructure_Multi()
    {
        var document = new JsonDocument(JObject.Parse("""{ "key": "value", "nested": { "another": "value" } }"""));

        document.SetValue("array:0:nested", "value");

        var jObj = Assert.IsType<JObject>(document._json);
        Assert.True(jObj.ContainsKey("key"));
        Assert.True(jObj.ContainsKey("nested"));
        Assert.True(jObj.ContainsKey("array"));
        var arr = Assert.IsType<JArray>(document._json["array"]);
        var singleItem = Assert.Single(arr);
        var innerObj = Assert.IsType<JObject>(singleItem);
        Assert.True(innerObj.ContainsKey("nested"));
        var nestedVal = Assert.IsType<JValue>(innerObj["nested"]);
        Assert.Equal("value", nestedVal.Value);
    }

    [Fact]
    public void Remove_ModifiesOriginalDocument_Property()
    {
        var document = new JsonDocument(JObject.Parse("""{ "key": "value", "nested": { "another": "value", "extra": "not deleted" } }"""));

        document.Remove("nested:another");

        var jObj = Assert.IsType<JObject>(document._json);
        Assert.True(jObj.ContainsKey("nested"));
        var nestedObj = Assert.IsType<JObject>(jObj["nested"]);
        Assert.False(nestedObj.ContainsKey("another"));
        Assert.True(nestedObj.ContainsKey("extra"));
    }

    [Fact]
    public void Remove_ModifiesOriginalDocument_Array()
    {
        var document = new JsonDocument(JObject.Parse("""{ "key": "value", "nested": [ "value" ] }"""));

        document.Remove("nested:0");

        var jObj = Assert.IsType<JObject>(document._json);
        Assert.True(jObj.ContainsKey("nested"));
        var nested = Assert.IsType<JArray>(jObj["nested"]);
        Assert.Empty(nested);
    }

    public static IEnumerable<object?[]> ToValueData()
    {
        yield return [JToken.Parse("null"), null];
        yield return [JToken.Parse("true"), true];
        yield return [JToken.Parse("false"), false];
        yield return [JToken.Parse("3.14"), 3.14];
        yield return [JToken.Parse("10"), 10L];
        yield return [JToken.Parse("\"\""), ""];
        yield return [JToken.Parse("\"stringValue\""), "stringValue"];
    }

    public static IEnumerable<object[]> FindValueData()
    {
        JToken json;

        json = JObject.Parse("""{ "key": "value", "another": 10 }""");
        yield return [json, "another", (JValue)json["another"]!];

        json = JArray.Parse(@"[ 1, null, false ]");
        yield return [json, "1", (JValue)json[1]!];

        json = JToken.Parse("true");
        yield return [json, "", (JValue)json];

        json = JObject.Parse("""{ "key": "value", "nested": { "another": "value2" } }""");
        yield return [json, "nested:another", (JValue)json["nested"]!["another"]!];

        json = JArray.Parse(@"[ 1, [ true ], false ]");
        yield return [json, "1:0", (JValue)json[1]![0]!];

        json = JObject.Parse("""{ "key": "value", "array": [ 1, false, null ] }""");
        yield return [json, "array:2", (JValue)json["array"]![2]!];

        json = JArray.Parse("""[2, false, { "key": "value" }]""");
        yield return [json, "2:key", (JValue)json[2]!["key"]!];

        json = JObject.Parse("""{ "key": "value", "array": [ false, { "key2": "value2" }, null ] }""");
        yield return [json, "array:1:key2", (JValue)json["array"]![1]!["key2"]!];

        json = JObject.Parse("""{ "key": "value", "not-array": { "0": true } }""");
        yield return [json, "not-array:0", (JValue)json["not-array"]!["0"]!];
    }
}
