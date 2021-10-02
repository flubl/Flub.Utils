using Moq;
using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Flub.Utils.Json.Test
{
    [ExcludeFromCodeCoverage]
    public class JsonSerializerOptionsExtensionTest
    {
        class ExampleIntJsonConverter : JsonConverter<int>
        {
            public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }

            public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }
        }

        class ExampleStringJsonConverter : JsonConverter<string>
        {
            public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }

            public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }
        }

        static readonly JsonSerializerOptions options = new()
        {
            AllowTrailingCommas = true,
            Converters =
                {
                    new JsonStringEnumConverter(),
                    new ExampleIntJsonConverter(),
                    new ExampleStringJsonConverter()
                },
            DefaultBufferSize = 1,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            DictionaryKeyPolicy = Mock.Of<JsonNamingPolicy>(),
            Encoder = Mock.Of<JavaScriptEncoder>(),
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
            IncludeFields = true,
            MaxDepth = 1,
            NumberHandling = JsonNumberHandling.WriteAsString,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = Mock.Of<JsonNamingPolicy>(),
            ReadCommentHandling = JsonCommentHandling.Skip,
            ReferenceHandler = Mock.Of<ReferenceHandler>(),
            WriteIndented = true
        };

        static void CompareOptions(JsonSerializerOptions expected, JsonSerializerOptions actual)
        {
            Assert.AreEqual(expected.AllowTrailingCommas, actual.AllowTrailingCommas);
            Assert.AreEqual(expected.DefaultBufferSize, actual.DefaultBufferSize);
            Assert.AreEqual(expected.DefaultIgnoreCondition, actual.DefaultIgnoreCondition);
            Assert.AreEqual(expected.DictionaryKeyPolicy, actual.DictionaryKeyPolicy);
            Assert.AreEqual(expected.Encoder, actual.Encoder);
            Assert.AreEqual(expected.IgnoreNullValues, actual.IgnoreNullValues);
            Assert.AreEqual(expected.IgnoreReadOnlyFields, actual.IgnoreReadOnlyFields);
            Assert.AreEqual(expected.IgnoreReadOnlyProperties, actual.IgnoreReadOnlyProperties);
            Assert.AreEqual(expected.IncludeFields, actual.IncludeFields);
            Assert.AreEqual(expected.MaxDepth, actual.MaxDepth);
            Assert.AreEqual(expected.NumberHandling, actual.NumberHandling);
            Assert.AreEqual(expected.PropertyNameCaseInsensitive, actual.PropertyNameCaseInsensitive);
            Assert.AreEqual(expected.PropertyNamingPolicy, actual.PropertyNamingPolicy);
            Assert.AreEqual(expected.ReadCommentHandling, actual.ReadCommentHandling);
            Assert.AreEqual(expected.ReferenceHandler, actual.ReferenceHandler);
            Assert.AreEqual(expected.WriteIndented, actual.WriteIndented);
        }

        [Test]
        public void GetWithoutConvertersEmptyTest()
        {
            JsonSerializerOptions result = JsonSerializerOptionsExtension.GetWithoutConverters(options);
            CompareOptions(options, result);
            Assert.DoesNotThrow(() => options.Converters.GroupJoin(result.Converters, e => e, a => a, (key, values) => values.Single()).ToArray());
        }

        [Test]
        public void GetWithoutConvertersRemovesAllTest()
        {
            JsonSerializerOptions result = JsonSerializerOptionsExtension.GetWithoutConverters(options,
                typeof(JsonStringEnumConverter), 
                typeof(ExampleIntJsonConverter),
                typeof(JsonConverter<string>),
                typeof(JsonConverter<bool>));
            CompareOptions(options, result);
            Assert.IsEmpty(result.Converters);
        }

        [Test]
        public void GetWithoutConverterFoundTest()
        {
            JsonSerializerOptions result = JsonSerializerOptionsExtension.GetWithoutConverter<JsonStringEnumConverter>(options);
            CompareOptions(options, result);
            Assert.IsFalse(result.Converters.Any(c => c.GetType() == typeof(JsonStringEnumConverter)));
        }

        [Test]
        public void GetWithoutConverterNotFoundTest()
        {
            JsonSerializerOptions result = JsonSerializerOptionsExtension.GetWithoutConverter<JsonConverter<bool>>(options);
            CompareOptions(options, result);
            Assert.DoesNotThrow(() => options.Converters.GroupJoin(result.Converters, e => e, a => a, (key, values) => values.Single()).ToArray());
        }

        [Test]
        public void GetWithoutConvertersThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => JsonSerializerOptionsExtension.GetWithoutConverters(null));
            Assert.Throws<ArgumentNullException>(() => JsonSerializerOptionsExtension.GetWithoutConverters(options, null));
            Assert.Throws<ArgumentNullException>(() => JsonSerializerOptionsExtension.GetWithoutConverter<JsonStringEnumConverter>(null));
        }
    }
}
