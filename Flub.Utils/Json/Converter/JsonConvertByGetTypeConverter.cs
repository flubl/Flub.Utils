﻿using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Flub.Utils.Json
{
    /// <summary>
    /// Supports converting several types by using a factory pattern to create a <see cref="JsonConvertByGetTypeConverter{TBase}"/> converter.
    /// </summary>
    public sealed class JsonConvertByGetTypeConverter : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) => true;

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
            (JsonConverter)Activator.CreateInstance(typeof(JsonConvertByGetTypeConverter<>).MakeGenericType(typeToConvert));
    }

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
