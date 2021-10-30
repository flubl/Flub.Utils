using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Flub.Utils.Json
{
    /// <summary>
    /// Supports converting several types by using a factory pattern to create a <see cref="JsonFieldEnumConverter{TBase}"/> converter.
    /// </summary>
    public sealed class JsonFieldEnumConverter : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) => typeToConvert.IsEnum;

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
            (JsonConverter)Activator.CreateInstance(typeof(JsonFieldEnumConverter<>).MakeGenericType(typeToConvert));
    }

    /// <summary>
    /// Converts enumeration values to and from strings.
    /// Values can be altered by <see cref="JsonFieldValueAttribute"/> attribute.
    /// </summary>
    /// <typeparam name="T">The type of Enum to convert.</typeparam>
    public sealed class JsonFieldEnumConverter<T> : JsonConverter<T> where T : struct, Enum
    {
        private const string DEFAULT_FLAGS_SEPERATOR = ",";

        private static bool EnumHasFlags => typeof(T).GetCustomAttribute<FlagsAttribute>() is not null;

        private readonly static IReadOnlyDictionary<T, string> values = typeof(T).GetFields()
            .Where(f => f.FieldType == typeof(T) && f.GetCustomAttribute<JsonIgnoreAttribute>() is null)
            .ToDictionary(
                f => (T)f.GetRawConstantValue(),
                f => f.GetCustomAttribute<JsonFieldValueAttribute>() is JsonFieldValueAttribute a ? a.Value : f.Name)
            .ToImmutableDictionary();

        private string flagsSeperator = DEFAULT_FLAGS_SEPERATOR;

        /// <summary>
        /// Gets or sets a <see cref="string"/> value that is used to combine the values of enums with the <see cref="FlagsAttribute"/> attribute.
        /// The default is <see langword="&quot;,&quot;" />.
        /// </summary>
        public string FlagsSeperator
        {
            get => flagsSeperator;
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("The value can't be null or empty.", nameof(value));
                flagsSeperator = value;
            }
        }

        private static void EnsureNoDuplicateValues()
        {
            if (values.FirstOrDefault(i => values.Any(j => !Equals(i.Key, j.Key) && Equals(i.Value, j.Value))) is KeyValuePair<T, string> result
                && result.Value is not null)
                throw new InvalidOperationException($"The JSON field value for '{typeof(T)}.{result.Key}' collides with another field.");
        }

        private static T Convert(string value)
        {
            if (values.SingleOrDefault(i => i.Value == value) is KeyValuePair<T, string> result && result.Value is not null)
                return result.Key;
            throw new JsonException($"The JSON value '{value}' could not be converted to {typeof(T)}");
        }

        private static string Convert(T value)
        {
            if (values.TryGetValue(value, out string result))
                return result;
            throw new JsonException($"The value '{value}' could not be converted to JSON.");
        }

        /// <summary>
        /// Reads and converts the JSON to type T.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        /// <returns>The converted value.</returns>
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            EnsureNoDuplicateValues();
            string value = reader.GetString();
            return EnumHasFlags ?
                (T)Enum.ToObject(typeToConvert, value.Split(FlagsSeperator).Select(i => System.Convert.ToInt64(Convert(i))).Aggregate((v, i) => v + i)) :
                Convert(value);
        }

        /// <summary>
        /// Writes a specified value as JSON.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="value">The value to convert to JSON.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            EnsureNoDuplicateValues();
            writer.WriteStringValue(EnumHasFlags ?
                Enum.GetValues<T>().SingleOrDefault(i => Equals(i, value)) is T t && !Equals(t, (T)default) ? Convert(t) :
                string.Join(FlagsSeperator, Enum.GetValues<T>().Where(i => value.HasFlag(i) && !Equals(i, (T)default)).Select(i => Convert(i))) :
                Convert(value));
        }
    }
}
