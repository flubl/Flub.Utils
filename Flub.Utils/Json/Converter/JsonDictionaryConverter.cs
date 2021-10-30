using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Flub.Utils.Json
{
    public class JsonDictionaryConverter : JsonConverterFactory
    {
        private static bool IsIDictionary(Type type) => type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>);
        private static IEnumerable<Type> GetIDictionaryGenericArguments(Type type) => (IsIDictionary(type) ? type : type.GetInterfaces().First(IsIDictionary)).GenericTypeArguments.Take(2);

        public override bool CanConvert(Type typeToConvert) => IsIDictionary(typeToConvert) || typeToConvert.GetInterfaces().Any(IsIDictionary);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
            (JsonConverter)Activator.CreateInstance(typeof(JsonDictionaryConverter<,,>).MakeGenericType(GetIDictionaryGenericArguments(typeToConvert).Prepend(typeToConvert).ToArray()));
    }

    public class JsonDictionaryConverter<TDictionary, TKey, TValue> : JsonConverter<TDictionary> where TDictionary : IDictionary<TKey, TValue>
    {
        public override TDictionary Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType is not JsonTokenType.StartObject)
                throw new JsonException();
            TDictionary items = typeToConvert.IsInterface ? (TDictionary)(object)new Dictionary<TKey, TValue>() : Activator.CreateInstance<TDictionary>();
            while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject)
            {
                if (reader.TokenType is not JsonTokenType.PropertyName)
                    throw new JsonException();
                string name = reader.GetString();
                reader.Read();
                TKey key = JsonSerializer.Deserialize<TKey>($"\"{name}\"", options);
                TValue value = JsonSerializer.Deserialize<TValue>(ref reader, options);
                if (!items.TryAdd(key, value))
                    throw new JsonException();
            }
            return items;
        }

        public override void Write(Utf8JsonWriter writer, TDictionary value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            foreach ((TKey k, TValue v) in value)
            {
                writer.WritePropertyName(JsonSerializer.Serialize(k, options).Trim('"'));
                JsonSerializer.Serialize(writer, v, options);
            }
            writer.WriteEndObject();
        }
    }
}
