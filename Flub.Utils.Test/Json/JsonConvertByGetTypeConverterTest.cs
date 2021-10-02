using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Text.Json;

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

        [Test]
        public void CanConvertTest()
        {
            Assert.IsTrue(new JsonConvertByGetTypeConverter<int>().CanConvert(typeof(int)));
            Assert.IsFalse(new JsonConvertByGetTypeConverter<string>().CanConvert(typeof(int)));
            Assert.IsTrue(new JsonConvertByGetTypeConverter<Base>().CanConvert(typeof(Base)));
            Assert.IsFalse(new JsonConvertByGetTypeConverter<Sub>().CanConvert(typeof(Base)));
            Assert.IsTrue(new JsonConvertByGetTypeConverter<Base>().CanConvert(typeof(Sub)));
        }

        [Test]
        public void ReadThrowsNotSupportedExceptionTest()
        {
            Assert.Throws<NotSupportedException>(() =>
            {
                Utf8JsonReader reader = new();
                new JsonConvertByGetTypeConverter<int>().Read(ref reader, null, null);
            });
        }

        [Test]
        public void WriteTest()
        {
            string Convert<T>(JsonConvertByGetTypeConverter<T> converter, T value, JsonSerializerOptions options)
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
    }
}
