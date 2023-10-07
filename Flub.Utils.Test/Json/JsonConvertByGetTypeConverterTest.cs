using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Flub.Utils.Json.Test
{
    [ExcludeFromCodeCoverage]
    public class JsonConvertByGetTypeConverterTest
    {
        static readonly string VALUE = Guid.NewGuid().ToString();
        static readonly string NAME = Guid.NewGuid().ToString();

        class Base
        {
            public string Value { get; } = VALUE;
        }

        class Sub : Base
        {
            public string Name { get; } = NAME;
        }

        class SubWithPropertyAttribute
        {
            [JsonConverter(typeof(JsonConvertByGetTypeConverter))]
            public Sub Sub { get; set; } = new();
        }

        class SubWithPropertyAttributeBase
        {
            [JsonConverter(typeof(JsonConvertByGetTypeConverter<Base>))]
            public Sub Sub { get; set; } = new();
        }

        class SubWithNullablePropertyAndPropertyAttribute
        {
            [JsonConverter(typeof(JsonConvertByGetTypeConverter))]
            public Sub? Sub { get; set; } = null;
        }

        class SubWithNullablePropertyAndPropertyAttributeBase
        {
            [JsonConverter(typeof(JsonConvertByGetTypeConverter<Base>))]
            public Sub? Sub { get; set; } = null;
        }

        [JsonConverter(typeof(JsonConvertByGetTypeConverter))]
        class SubWithClassAttribute
        {
            public string Value { get; } = VALUE;
        }

        [Test]
        public void CanConvertTest()
        {
            Assert.IsTrue(new JsonConvertByGetTypeConverter().CanConvert(typeof(int)));
            Assert.IsTrue(new JsonConvertByGetTypeConverter().CanConvert(typeof(string)));
            Assert.IsTrue(new JsonConvertByGetTypeConverter().CanConvert(typeof(Base)));
            Assert.IsTrue(new JsonConvertByGetTypeConverter().CanConvert(typeof(Sub)));

            Assert.IsTrue(new JsonConvertByGetTypeConverter<int>().CanConvert(typeof(int)));
            Assert.IsFalse(new JsonConvertByGetTypeConverter<string>().CanConvert(typeof(int)));
            Assert.IsTrue(new JsonConvertByGetTypeConverter<Base>().CanConvert(typeof(Base)));
            Assert.IsFalse(new JsonConvertByGetTypeConverter<Sub>().CanConvert(typeof(Base)));
            Assert.IsTrue(new JsonConvertByGetTypeConverter<Base>().CanConvert(typeof(Sub)));
        }

        [Test]
        public void GetConverterTest()
        {
            Assert.AreEqual(typeof(JsonConvertByGetTypeConverter<int>), new JsonConvertByGetTypeConverter().CreateConverter(typeof(int), JsonSerializerOptions.Default)?.GetType());
            Assert.AreEqual(typeof(JsonConvertByGetTypeConverter<string>), new JsonConvertByGetTypeConverter().CreateConverter(typeof(string), JsonSerializerOptions.Default)?.GetType());
            Assert.AreEqual(typeof(JsonConvertByGetTypeConverter<Base>), new JsonConvertByGetTypeConverter().CreateConverter(typeof(Base), JsonSerializerOptions.Default)?.GetType());
            Assert.AreEqual(typeof(JsonConvertByGetTypeConverter<Sub>), new JsonConvertByGetTypeConverter().CreateConverter(typeof(Sub), JsonSerializerOptions.Default)?.GetType());
        }

        [Test]
        public void ReadThrowsNotSupportedExceptionTest()
        {
            Assert.Throws<NotSupportedException>(() =>
            {
                Utf8JsonReader reader = new();
                new JsonConvertByGetTypeConverter<int>().Read(ref reader, typeof(string), JsonSerializerOptions.Default);
            });
            string json = "{\"Sub\":{}}";
            Assert.Throws<NotSupportedException>(() => JsonSerializer.Deserialize<SubWithPropertyAttribute>(json));
            Assert.Throws<NotSupportedException>(() => JsonSerializer.Deserialize<SubWithPropertyAttributeBase>(json));
        }

        [Test]
        public void WriteThrowsArgumentNullExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                using MemoryStream stream = new();
                using Utf8JsonWriter writer = new(stream);
#pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
                new JsonConvertByGetTypeConverter<string?>().Write(writer, null, JsonSerializerOptions.Default);
#pragma warning restore CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
            });
            Assert.DoesNotThrow(() => JsonSerializer.Serialize<SubWithClassAttribute?>(null));
            Assert.DoesNotThrow(() => JsonSerializer.Serialize<SubWithNullablePropertyAndPropertyAttribute>(new()));
            Assert.DoesNotThrow(() => JsonSerializer.Serialize<SubWithNullablePropertyAndPropertyAttributeBase>(new()));
        }

        [Test]
        public void WriteTest()
        {
            static string Convert<T>(JsonConvertByGetTypeConverter<T> converter, T value, JsonSerializerOptions options)
            {
                using MemoryStream stream = new();
                using Utf8JsonWriter writer = new(stream);
                converter.Write(writer, value, options);
                writer.Flush();
                return Encoding.UTF8.GetString(stream.ToArray());
            }

            JsonConvertByGetTypeConverter<Base> converter = new();
            JsonSerializerOptions options = new();

            string expectedBaseJson = $"{{\"{nameof(Base.Value)}\":\"{VALUE}\"}}";
            string expectedSubJson = $"{{\"{nameof(Sub.Name)}\":\"{NAME}\",\"{nameof(Base.Value)}\":\"{VALUE}\"}}";

            Base value1 = new();
            Sub value2 = new();
            Base value3 = new Sub();

            string result1 = Convert(converter, value1, options);
            string result2 = Convert(converter, value2, options);
            string result3 = Convert(converter, value3, options);

            Assert.AreEqual(expectedBaseJson, result1);
            Assert.AreEqual(expectedSubJson, result2);
            Assert.AreEqual(expectedSubJson, result3);
        }

        [Test]
        public void WriteWithAttributeTest()
        {
            string expectedSubWithPropertyAttributeJson = $"{{\"{nameof(SubWithPropertyAttribute.Sub)}\":{{"
                + $"\"{nameof(SubWithPropertyAttribute.Sub.Name)}\":\"{NAME}\",\"{nameof(SubWithPropertyAttribute.Sub.Value)}\":\"{VALUE}\"}}}}";
            string expectedSubWithPropertyAttributeBaseJson = $"{{\"{nameof(SubWithPropertyAttributeBase.Sub)}\":{{"
                + $"\"{nameof(SubWithPropertyAttributeBase.Sub.Name)}\":\"{NAME}\",\"{nameof(SubWithPropertyAttributeBase.Sub.Value)}\":\"{VALUE}\"}}}}";

            string actualSubWithPropertyAttributeJson = JsonSerializer.Serialize(new SubWithPropertyAttribute() { Sub = new() });
            string actualSubWithPropertyAttributeBaseJson = JsonSerializer.Serialize(new SubWithPropertyAttributeBase() { Sub = new() });

            Assert.AreEqual(expectedSubWithPropertyAttributeJson, actualSubWithPropertyAttributeJson);
            Assert.AreEqual(expectedSubWithPropertyAttributeBaseJson, actualSubWithPropertyAttributeBaseJson);
        }
    }
}
