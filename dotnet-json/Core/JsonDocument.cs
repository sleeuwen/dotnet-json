using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace dotnet_json.Core
{
    internal sealed class JsonDocument
    {
        private static readonly Encoding Utf8EncodingWithoutBom = new UTF8Encoding(false);
        private static readonly JsonSerializer Serializer = JsonSerializer.CreateDefault();

        internal readonly JToken _json;

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
            using var sw = new StreamWriter(stream, Utf8EncodingWithoutBom);
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

        internal static IEnumerable<KeyValuePair<string, JValue>> AllValues(JToken token)
        {
            return AllTokens(token)
                .Where(kv => kv.Value is JValue)
                .Select(kv => KeyValuePair.Create(kv.Key, (kv.Value as JValue)!));
        }

        internal static IEnumerable<KeyValuePair<string, JToken>> AllTokens(JToken token, string prefix = "")
        {
            yield return KeyValuePair.Create(prefix, token);

            switch (token)
            {
                case JValue:
                    break;

                case JProperty jProperty:
                    foreach (var kv in AllTokens(jProperty.Value, CreatePrefix(prefix, jProperty.Name)))
                        yield return kv;
                    break;

                case JArray jArray:
                    for (var i = 0; i < jArray.Count; i++)
                    {
                        foreach (var kv in AllTokens(jArray[i], CreatePrefix(prefix, i.ToString())))
                            yield return kv;
                    }

                    break;

                case JObject jObject:
                    foreach (var (key, value) in jObject)
                    {
                        if (value == null)
                            continue;

                        foreach (var kv in AllTokens(value, CreatePrefix(prefix, key)))
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
            var parentKey = "";
            var bestParent = (JToken?)(_json as JObject) ?? (_json as JArray);

            foreach (var kv in AllTokens(_json))
            {
                if (kv.Key == key)
                    return kv.Value;

                if (key.StartsWith(kv.Key) && kv.Value is JObject jObject && kv.Key.Length > parentKey.Length)
                {
                    parentKey = kv.Key;
                    bestParent = jObject;
                }
            }

            if (bestParent == null || !createNew)
                return null;

            var restKeys = key.Substring(parentKey.Length).TrimStart(':').Split(':');
            for (var i = 0; i < restKeys.Length - 1; i++)
            {
                JToken newValue;

                if (restKeys.Length > i + 1 && int.TryParse(restKeys[i + 1], out _))
                    newValue = new JArray();
                else
                    newValue = new JObject();

                if (bestParent is JArray jArray)
                {
                    if (!int.TryParse(restKeys[i], out var idx))
                        throw new Exception($"Cannot index into array with key '{restKeys[^1]}'");

                    while (jArray.Count <= idx)
                        jArray.Add(new JValue((object?)null));
                    jArray[idx] = newValue;
                }
                else
                {
                    bestParent[restKeys[i]] = newValue;
                }

                bestParent = newValue;
            }

            var value = (JToken)new JValue((object?)null);

            if (bestParent is JArray array)
            {
                if (!int.TryParse(restKeys[^1], out var idx))
                    throw new Exception($"Cannot index into array with key '{restKeys[^1]}'");

                while (array.Count <= idx)
                    array.Add(new JValue((object?)null));
                array[idx] = value;
                value = array[idx];
            }
            else
            {
                bestParent[restKeys[^1]] = value;
            }

            return value;
        }

        internal void SetValue(string key, object? value)
        {
            var jToken = FindToken(key, true);

            if (!(jToken is JValue jValue))
                throw new Exception("The key you're trying to set is already set to a complex type, refusing to overwrite it.");

            jValue.Value = value;
        }
    }
}
