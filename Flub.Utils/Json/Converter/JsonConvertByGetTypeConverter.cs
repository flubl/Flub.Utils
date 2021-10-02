using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Flub.Utils.Json
{
    /// <summary>
    /// Converts an object or value with the specified base type to or from JSON.
    /// Uses the GetType() method of the object or value to determind the type.
    /// </summary>
    /// <typeparam name="TBase">The base type of object or value handled by the converter.</typeparam>
    public class JsonConvertByGetTypeConverter<TBase> : JsonConverter<TBase>
    {
        public override bool CanConvert(Type typeToConvert) =>
            typeToConvert.IsAssignableTo(typeof(TBase));

        public override TBase Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            throw new NotSupportedException();

        public override void Write(Utf8JsonWriter writer, TBase value, JsonSerializerOptions options) =>
            JsonSerializer.Serialize(writer, value, value.GetType(), options.GetWithoutConverter<JsonConvertByGetTypeConverter<TBase>>());
    }
}
