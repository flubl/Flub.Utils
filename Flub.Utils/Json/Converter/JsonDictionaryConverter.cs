using System.Text.Json;
using System.Text.Json.Serialization;

namespace Flub.Utils.Json
{
    /// <summary>
    /// Supports converting several types by using a factory pattern to create a <see cref="JsonDictionaryConverter{TDictionary, TKey, TValue}"/> converter.
    /// </summary>
    public class JsonDictionaryConverter : JsonConverterFactory
    {
        private static bool IsIDictionary(Type type) => type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>);
        private static IEnumerable<Type> GetIDictionaryGenericArguments(Type type) =>
            ((IsIDictionary(type) ? type : type.GetInterfaces().FirstOrDefault(IsIDictionary)) ?? throw new ArgumentException($"Type of '{nameof(type)}' is not an dictionary.", nameof(type))).GenericTypeArguments.Take(2);

        /// <inheritdoc/>
        public override bool CanConvert(Type typeToConvert) => IsIDictionary(typeToConvert) || typeToConvert.GetInterfaces().Any(IsIDictionary);

        /// <inheritdoc/>
        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
            (JsonConverter?)Activator.CreateInstance(typeof(JsonDictionaryConverter<,,>).MakeGenericType(GetIDictionaryGenericArguments(typeToConvert).Prepend(typeToConvert).ToArray()));
    }

    /// <summary>
    /// Converts an dictionary with a non-string key to or from JSON.
    /// </summary>
    /// <typeparam name="TDictionary">The type of dictionary.</typeparam>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    public class JsonDictionaryConverter<TDictionary, TKey, TValue> : JsonConverter<TDictionary> where TDictionary : IDictionary<TKey, TValue?> where TKey : notnull
    {
        /// <inheritdoc/>
        public override TDictionary? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType is not JsonTokenType.StartObject)
                throw new JsonException($"The JSON value could not be converted to {typeof(TDictionary)}.");
            TDictionary items = typeToConvert.IsInterface ? (TDictionary)(object)new Dictionary<TKey, TValue?>() : Activator.CreateInstance<TDictionary>();
            while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject)
            {
                string? name = reader.GetString() ?? throw new NullReferenceException("value of a key was null");
                reader.Read();
                TKey? key;
                try
                {
                    key = JsonSerializer.Deserialize<TKey>($"\"{name}\"", options);
                }
                catch (JsonException)
                {
                    key = JsonSerializer.Deserialize<TKey>(name, options);
                }
                if (key == null) throw new NullReferenceException("value of a key was null");
                TValue? value = JsonSerializer.Deserialize<TValue>(ref reader, options);
                items.Add(key, value);
            }
            return items;
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, TDictionary value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            foreach ((TKey k, TValue? v) in value)
            {
                writer.WritePropertyName(JsonSerializer.Serialize(k, options).Trim('"'));
                JsonSerializer.Serialize(writer, v, options);
            }
            writer.WriteEndObject();
        }
    }
}
