using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace dotnet_json.Core
{
    internal sealed class JsonDocument
    {
        private static readonly Encoding UTF8EncodingWithoutBOM = new UTF8Encoding(false);
        private static readonly JsonSerializer Serializer = JsonSerializer.CreateDefault();

        internal JToken _json;

        internal JsonDocument(JToken json)
        {
            _json = json;
        }

        public static JsonDocument ReadFromStream(Stream stream, bool leaveOpen = false)
        {
            var json = Serializer.Deserialize(new JsonTextReader(new StreamReader(stream, leaveOpen))) ?? throw new Exception("Unable to read json");
            return new JsonDocument((JToken)json);
        }

        public void WriteToStream(Stream stream, Formatting formatting = Formatting.Indented)
        {
            using var sw = new StreamWriter(stream, UTF8EncodingWithoutBOM);
            sw.Write(_json.ToString(formatting));
        }

        public void Merge(JsonDocument document)
        {
            foreach (var (key, value) in AllValues(document._json))
                SetValue(key, ToValue(value));
        }

        public object? this[string key]
        {
            get => FindToken(key);
            set => SetValue(key, value);
        }

        public void Remove(string key)
        {
            var jValue = FindToken(key);

            if (jValue?.Parent is JProperty jProperty)
                jProperty.Remove();
            else
                jValue?.Remove();
        }

        internal static IEnumerable<KeyValuePair<string, JValue>> AllValues(JToken token, string prefix = "")
        {
            switch (token)
            {
                case JValue jValue:
                    yield return KeyValuePair.Create(prefix, jValue);
                    break;

                case JProperty jProperty:
                    foreach (var kv in AllValues(jProperty.Value, CreatePrefix(prefix, jProperty.Name)))
                        yield return kv;
                    break;

                case JArray jArray:
                    for (var i = 0; i < jArray.Count; i++)
                        foreach (var kv in AllValues(jArray[i], CreatePrefix(prefix, i.ToString())))
                            yield return kv;
                    break;

                case JObject jObject:
                    foreach (var (key, value) in jObject)
                    {
                        if (value == null)
                            continue;

                        foreach (var kv in AllValues(value, CreatePrefix(prefix, key)))
                            yield return kv;
                    }

                    break;

                default:
                    throw new ArgumentException("Unhandled token type: " + token.GetType());
            }
        }

        internal static object? ToValue(JValue value)
            => value.Value;

        internal static string CreatePrefix(string prefix, string value) => prefix switch
        {
            null => value,
            "" => value,
            _ => $"{prefix}:{value}",
        };

        internal JToken? FindToken(string key, bool createNew = false)
        {
            var subKeys = string.IsNullOrWhiteSpace(key) ? new string[0] : key.Split(':');

            var current = _json;
            for (var i = 0; i < subKeys.Length; i++)
            {
                var subkey = subKeys[i];
                var isLastKey = i == subKeys.Length - 1;

                if (current is JValue jValue && (!createNew || jValue.Value == null))
                    throw new ArgumentException("");

                if (int.TryParse(subkey, out _) && current is JObject && ((JObject)current).Count == 0 && current.Parent != null)
                {
                    if (current.Parent is JProperty jProperty)
                        jProperty.Value = current = new JArray();
                    else if (current.Parent is JArray parentArray)
                        parentArray[parentArray.IndexOf(current)] = current = new JArray();
                }

                if (current is JArray jArray)
                {
                    current = FindTokenInArray(jArray, subkey, createNew, isLastKey);
                    continue;
                }

                var jObject = (JObject)current!; // At this point current can only be a JObject.
                current = jObject[subkey];

                if (current == null && createNew)
                {
                    current = jObject[subkey] = isLastKey ? (JToken)new JValue((object?)null) : new JObject();
                }
            }

            return current; // If current is no JValue here, throw an exception
        }

        internal static JToken FindTokenInArray(JArray jArray, string subkey, bool createNew, bool isLastKey)
        {
            if (!int.TryParse(subkey, out var index))
                throw new Exception($"Cannot index into array at {GetPosition(jArray)} with index {subkey}.");

            if (createNew && index >= jArray.Count)
            {
                for (var j = jArray.Count; j < index; j++)
                    jArray.Add(new JValue((object?)null));
                jArray.Add(isLastKey ? (JToken)new JValue((object?)null) : new JObject());
            }

            if (index >= jArray.Count)
                throw new IndexOutOfRangeException($"Index {index} does not exist for array at {GetPosition(jArray)}");

            return jArray[index];
        }

        internal void SetValue(string key, object? value)
        {
            var jToken = FindToken(key, true);

            if (!(jToken is JValue jValue))
                throw new Exception("The key you're trying to set is already set to a complex type, refusing to overwrite it.");

            jValue.Value = value;
        }

        internal static string GetPosition(JToken token)
            => token.Path.Replace(".", ":").Replace("[", ".").Replace("]", "");
    }
}
