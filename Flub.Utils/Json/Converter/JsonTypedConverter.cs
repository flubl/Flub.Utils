using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Flub.Utils.Json
{
    /// <summary>
    /// Supports converting several types by using a factory pattern to create a <see cref="JsonTypedConverter{TBase, TType}"/> converter.
    /// </summary>
    public sealed class JsonTypedConverter : JsonConverterFactory
    {
        private static bool IsIJsonTyped(Type type) => type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(IJsonTyped<>);
        private static Type GetIJsonTypedGenericArgument(Type type) => (IsIJsonTyped(type) ? type : type.GetInterfaces().First(IsIJsonTyped)).GenericTypeArguments.Single();

        public override bool CanConvert(Type typeToConvert) => IsIJsonTyped(typeToConvert) || typeToConvert.GetInterfaces().Any(IsIJsonTyped);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
            (JsonConverter)Activator.CreateInstance(typeof(JsonTypedConverter<,>).MakeGenericType(typeToConvert, GetIJsonTypedGenericArgument(typeToConvert)));
    }

    /// <summary>
    /// Converts an object with the specified base type to or from JSON.
    /// Determindes the type of the object from a property value when converting from JSON.
    /// Uses the GetType() method of the object to determind the type when converting to JSON.
    /// </summary>
    /// <typeparam name="TBase">The base type of object handled by the converter.</typeparam>
    /// <typeparam name="TType">The type of the property to get type from.</typeparam>
    public sealed class JsonTypedConverter<TBase, TType> : JsonConvertByGetTypeConverter<TBase> where TBase : IJsonTyped<TType>
    {
        private readonly static PropertyInfo Property = typeof(TBase).GetProperty(nameof(IJsonTyped<TType>.Type));
        private readonly static bool ValidPropertyType = Property?.PropertyType == typeof(TType);

        public override bool CanConvert(Type typeToConvert) =>
            ValidPropertyType && (typeToConvert?.IsAssignableTo(typeof(TBase)) ?? false);

        public override TBase Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException($"The JSON value could not be converted to {typeof(TBase)}.");
            string propertyName = Property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? Property.Name;
            int index = 0;
            bool valueFound = false;
            TType value = default;
            using MemoryStream stream = new();
            using Utf8JsonWriter writer = new(stream);
            writer.WriteStartObject();
            while (reader.Read() && !(reader.TokenType == JsonTokenType.EndObject && index <= 0))
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.PropertyName:
                        string name = reader.GetString();
                        writer.WritePropertyName(name);
                        if (!valueFound && name == propertyName && index == 0 && reader.Read())
                        {
                            value = JsonSerializer.Deserialize<TType>(ref reader, options);
                            valueFound = true;
                            JsonSerializer.Serialize(writer, value, options);
                        }
                        break;
                    case JsonTokenType.StartObject:
                        writer.WriteStartObject();
                        index++;
                        break;
                    case JsonTokenType.EndObject:
                        writer.WriteEndObject();
                        index--;
                        break;
                    case JsonTokenType.Number:
                        writer.WriteNumberValue(reader.GetInt64());
                        break;
                    case JsonTokenType.String:
                        writer.WriteStringValue(reader.GetString());
                        break;
                    case JsonTokenType.StartArray:
                        writer.WriteStartArray();
                        break;
                    case JsonTokenType.EndArray:
                        writer.WriteEndArray();
                        break;
                    case JsonTokenType.Null:
                        writer.WriteNullValue();
                        break;
                    case JsonTokenType.True:
                        writer.WriteBooleanValue(true);
                        break;
                    case JsonTokenType.False:
                        writer.WriteBooleanValue(false);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            writer.WriteEndObject();
            writer.Flush();
            if (!valueFound)
                throw new JsonException($"No value for '{propertyName}' found.");
            try
            {
                Type returnType = Assembly.GetAssembly(typeof(TBase)).GetTypes()
                    .Where(t => t.IsAssignableTo(typeof(TBase)))
                    .SingleOrDefault(t => t.GetCustomAttribute<JsonTypedAttribute>() is JsonTypedAttribute a && Equals(a.Value, value));
                if (returnType is null)
                    throw new JsonException($"No {typeof(TBase)} found with value '{value}'.");
                var subReader = new Utf8JsonReader(stream.ToArray());
                return (TBase)JsonSerializer.Deserialize(ref subReader, returnType, options?.GetWithoutConverter<JsonTypedConverter<TBase, TType>>());
            }
            catch (InvalidOperationException)
            {
                throw new JsonException($"More than one {typeof(TBase)} found for value '{value}'.");
            }
        }
    }
}
