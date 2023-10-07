using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Flub.Utils.Json.Test
{
    [ExcludeFromCodeCoverage]
    public class JsonDictionaryConverterTest
    {
        enum TestEnumWithoutConverter
        {
            [JsonFieldValue("none")]
            None,
            [JsonFieldValue("other")]
            Other
        }

        [JsonConverter(typeof(JsonFieldEnumConverter<TestEnumWithConverter>))]
        enum TestEnumWithConverter
        {
            [JsonFieldValue("none")]
            None,
            [JsonFieldValue("other")]
            Other
        }

        class TestClass
        {
            [JsonConverter(typeof(JsonDictionaryConverter<Dictionary<TestEnumWithConverter, string?>, TestEnumWithConverter, string>))]
            public Dictionary<TestEnumWithConverter, string> Value { get; set; } = new();
        }

        class TestClassFactory
        {
            [JsonConverter(typeof(JsonDictionaryConverter))]
            public Dictionary<TestEnumWithConverter, string> Value { get; set; } = new();
        }

        class TestClassWithoutConverter
        {
            public string Value { get; set; }
        }

        [JsonConverter(typeof(TestClassConverter))]
        class TestClassWithConverter
        {
            public string Value { get; set; }
        }

        class TestClassConverter : JsonConverter<TestClassWithConverter>
        {
            public override TestClassWithConverter Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return new TestClassWithConverter() { Value = reader.GetString() };
            }

            public override void Write(Utf8JsonWriter writer, TestClassWithConverter value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.Value);
            }
        }

        [Test]
        public void ConverterFactoryCanConvertTest()
        {
            Assert.IsTrue(new JsonDictionaryConverter().CanConvert(typeof(Dictionary<string, string>)));
            Assert.IsTrue(new JsonDictionaryConverter().CanConvert(typeof(Dictionary<int, string>)));
            Assert.IsTrue(new JsonDictionaryConverter().CanConvert(typeof(Dictionary<TestClass, string>)));
            Assert.IsTrue(new JsonDictionaryConverter().CanConvert(typeof(Dictionary<TestEnumWithConverter, string>)));
            Assert.IsFalse(new JsonDictionaryConverter().CanConvert(typeof(int)));
            Assert.IsFalse(new JsonDictionaryConverter().CanConvert(typeof(string)));
            Assert.IsFalse(new JsonDictionaryConverter().CanConvert(typeof(Enum)));
            Assert.IsFalse(new JsonDictionaryConverter().CanConvert(typeof(TestClass)));
            Assert.IsFalse(new JsonDictionaryConverter().CanConvert(typeof(TestEnumWithConverter)));
        }


        [Test]
        public void ConverterFactoryCreateConverter()
        {
            Assert.AreEqual(typeof(JsonDictionaryConverter<Dictionary<string, string>, string, string>), new JsonDictionaryConverter().CreateConverter(typeof(Dictionary<string, string>), null).GetType());
            Assert.AreEqual(typeof(JsonDictionaryConverter<Dictionary<int, string>, int, string>), new JsonDictionaryConverter().CreateConverter(typeof(Dictionary<int, string>), null).GetType());
            Assert.AreEqual(typeof(JsonDictionaryConverter<Dictionary<TestClass, string>, TestClass, string>), new JsonDictionaryConverter().CreateConverter(typeof(Dictionary<TestClass, string>), null).GetType());
            Assert.AreEqual(typeof(JsonDictionaryConverter<Dictionary<TestEnumWithConverter, string>, TestEnumWithConverter, string>), new JsonDictionaryConverter().CreateConverter(typeof(Dictionary<TestEnumWithConverter, string>), null).GetType());
            Assert.Throws<ArgumentException>(() => new JsonDictionaryConverter().CreateConverter(typeof(int), null));
            Assert.Throws<ArgumentException>(() => new JsonDictionaryConverter().CreateConverter(typeof(string), null));
            Assert.Throws<ArgumentException>(() => new JsonDictionaryConverter().CreateConverter(typeof(Enum), null));
            Assert.Throws<ArgumentException>(() => new JsonDictionaryConverter().CreateConverter(typeof(TestClass), null));
            Assert.Throws<ArgumentException>(() => new JsonDictionaryConverter().CreateConverter(typeof(TestEnumWithConverter), null));
        }

        [Test]
        public void ConverterInOptionsTest()
        {
            var options = new JsonSerializerOptions() { Converters = { new JsonDictionaryConverter<Dictionary<TestEnumWithConverter, string>, TestEnumWithConverter, string>() } };

            var value = new Dictionary<TestEnumWithConverter, string>() { { TestEnumWithConverter.Other, "value" } };
            var valueJson = "{\"other\":\"value\"}";

            var valueResult = JsonSerializer.Deserialize<Dictionary<TestEnumWithConverter, string>>(valueJson, options);
            var valueJsonResult = JsonSerializer.Serialize(value, options);

            Assert.AreEqual(value, valueResult);
            Assert.AreEqual(valueJson, valueJsonResult);
        }

        [Test]
        public void ConverterFactoryInOptionsTest()
        {
            var options = new JsonSerializerOptions() { Converters = { new JsonDictionaryConverter() } };

            var value = new Dictionary<TestEnumWithConverter, string>() { { TestEnumWithConverter.Other, "value" } };
            var valueJson = "{\"other\":\"value\"}";

            var valueResult = JsonSerializer.Deserialize<Dictionary<TestEnumWithConverter, string>>(valueJson, options);
            var valueJsonResult = JsonSerializer.Serialize(value, options);

            Assert.AreEqual(value, valueResult);
            Assert.AreEqual(valueJson, valueJsonResult);
        }

        [Test]
        public void ConverterInPropertyAttributeTest()
        {
            var value = new TestClass { Value = new() { { TestEnumWithConverter.Other, "value" } } };
            var valueJson = "{\"Value\":{\"other\":\"value\"}}";

            var valueResult = JsonSerializer.Deserialize<TestClass>(valueJson);
            var valueJsonResult = JsonSerializer.Serialize(value);

            Assert.AreEqual(value.Value, valueResult.Value);
            Assert.AreEqual(valueJson, valueJsonResult);
        }

        [Test]
        public void ConverterFactoryInPropertyAttributeTest()
        {
            var value = new TestClassFactory { Value = new() { { TestEnumWithConverter.Other, "value" } } };
            var valueJson = "{\"Value\":{\"other\":\"value\"}}";

            var valueResult = JsonSerializer.Deserialize<TestClassFactory>(valueJson);
            var valueJsonResult = JsonSerializer.Serialize(value);

            Assert.AreEqual(value.Value, valueResult.Value);
            Assert.AreEqual(valueJson, valueJsonResult);
        }

        [Test]
        public void NoConverterForKeyTypeTest()
        {
            var options = new JsonSerializerOptions() { Converters = { new JsonDictionaryConverter() } };

            var value = new Dictionary<TestEnumWithoutConverter, string>() { { TestEnumWithoutConverter.Other, "value" } };
            var valueJson = "{\"1\":\"value\"}";

            var valueResult = JsonSerializer.Deserialize<Dictionary<TestEnumWithoutConverter, string>>(valueJson, options);
            var valueJsonResult = JsonSerializer.Serialize(value, options);

            Assert.AreEqual(value, valueResult);
            Assert.AreEqual(valueJson, valueJsonResult);
        }

        [Test]
        public void StringAsKeyTest()
        {
            var options = new JsonSerializerOptions() { Converters = { new JsonDictionaryConverter() } };

            var value = new Dictionary<string, string>() { { "key", "value" } };
            var valueJson = "{\"key\":\"value\"}";

            var valueResult = JsonSerializer.Deserialize<Dictionary<string, string>>(valueJson, options);
            var valueJsonResult = JsonSerializer.Serialize(value, options);

            Assert.AreEqual(value, valueResult);
            Assert.AreEqual(valueJson, valueJsonResult);
        }

        [Test]
        public void IntAsKeyTest()
        {
            var options = new JsonSerializerOptions() { Converters = { new JsonDictionaryConverter() } };

            var value = new Dictionary<int, string>() { { 1, "value" } };
            var valueJson = "{\"1\":\"value\"}";

            var valueResult = JsonSerializer.Deserialize<Dictionary<int, string>>(valueJson, options);
            var valueJsonResult = JsonSerializer.Serialize(value, options);

            Assert.AreEqual(value, valueResult);
            Assert.AreEqual(valueJson, valueJsonResult);
        }

        [Test]
        public void ClassWithoutConverterAsKeyTest()
        {
            var options = new JsonSerializerOptions() { Converters = { new JsonDictionaryConverter() } };

            var value = new Dictionary<TestClassWithoutConverter, string>() { { new() { Value = "key" }, "value" } };
            var valueJson = "{\"{\\u0022Value\\u0022:\\u0022key\\u0022}\":\"value\"}";

            var valueResult = JsonSerializer.Deserialize<Dictionary<TestClassWithoutConverter, string>>(valueJson, options);
            var valueJsonResult = JsonSerializer.Serialize(value, options);

            Assert.AreEqual(value.Keys.Single().Value, valueResult.Keys.Single().Value);
            Assert.AreEqual(value.Values.Single(), valueResult.Values.Single());
            Assert.AreEqual(valueJson, valueJsonResult);
        }

        [Test]
        public void ClassWithConverterAsKeyTest()
        {
            var options = new JsonSerializerOptions() { Converters = { new JsonDictionaryConverter() } };

            var value = new Dictionary<TestClassWithConverter, string>() { { new() { Value = "key" }, "value" } };
            var valueJson = "{\"key\":\"value\"}";

            var valueResult = JsonSerializer.Deserialize<Dictionary<TestClassWithConverter, string>>(valueJson, options);
            var valueJsonResult = JsonSerializer.Serialize(value, options);

            Assert.AreEqual(value.Keys.Single().Value, valueResult.Keys.Single().Value);
            Assert.AreEqual(value.Values.Single(), valueResult.Values.Single());
            Assert.AreEqual(valueJson, valueJsonResult);
        }

        [Test]
        public void EnumWithStringConverterAsKeyTest()
        {
            var options = new JsonSerializerOptions() { Converters = { new JsonDictionaryConverter(), new JsonStringEnumConverter() } };

            var value = new Dictionary<TestEnumWithoutConverter, string>() { { TestEnumWithoutConverter.Other, "value" } };
            var valueJson = "{\"Other\":\"value\"}";

            var valueResult = JsonSerializer.Deserialize<Dictionary<TestEnumWithoutConverter, string>>(valueJson, options);
            var valueJsonResult = JsonSerializer.Serialize(value, options);

            Assert.AreEqual(value, valueResult);
            Assert.AreEqual(valueJson, valueJsonResult);
        }

        [Test]
        public void EnumAsKeyAndClassAsValueTest()
        {
            var options = new JsonSerializerOptions() { Converters = { new JsonDictionaryConverter() } };

            var value = new Dictionary<TestEnumWithConverter, TestClassWithoutConverter>() { { TestEnumWithConverter.Other, new() { Value = "value" } } };
            var valueJson = "{\"other\":{\"Value\":\"value\"}}";

            var valueResult = JsonSerializer.Deserialize<Dictionary<TestEnumWithConverter, TestClassWithoutConverter>>(valueJson, options);
            var valueJsonResult = JsonSerializer.Serialize(value, options);

            Assert.AreEqual(value.Keys.Single(), valueResult.Keys.Single());
            Assert.AreEqual(value.Values.Single().Value, valueResult.Values.Single().Value);
            Assert.AreEqual(valueJson, valueJsonResult);
        }

        [Test]
        public void MultipleEntriesTest()
        {
            var options = new JsonSerializerOptions() { Converters = { new JsonDictionaryConverter() } };

            var value = new Dictionary<int, string>() { { 1, "one" }, { 2, "two" }, { 3, "three" } };
            var valueJson = "{\"1\":\"one\",\"2\":\"two\",\"3\":\"three\"}";

            var valueResult = JsonSerializer.Deserialize<Dictionary<int, string>>(valueJson, options);
            var valueJsonResult = JsonSerializer.Serialize(value, options);

            Assert.AreEqual(value, valueResult);
            Assert.AreEqual(valueJson, valueJsonResult);
        }

        [Test]
        public void IDictionaryInterfaceTest()
        {
            var options = new JsonSerializerOptions() { Converters = { new JsonDictionaryConverter() } };

            var value = new Dictionary<int, string>() { { 1, "value" } };
            var valueJson = "{\"1\":\"value\"}";

            var valueResult = JsonSerializer.Deserialize<IDictionary<int, string>>(valueJson, options);
            var valueJsonResult = JsonSerializer.Serialize(value, options);

            Assert.AreEqual(value, valueResult);
            Assert.AreEqual(typeof(Dictionary<int, string>), valueResult.GetType());
            Assert.AreEqual(valueJson, valueJsonResult);
        }

        [Test]
        public void SortedDictionaryTest()
        {
            var options = new JsonSerializerOptions() { Converters = { new JsonDictionaryConverter() } };

            var value = new SortedDictionary<int, string>() { { 1, "value" } };
            var valueJson = "{\"1\":\"value\"}";

            var valueResult = JsonSerializer.Deserialize<SortedDictionary<int, string>>(valueJson, options);
            var valueJsonResult = JsonSerializer.Serialize(value, options);

            Assert.AreEqual(value, valueResult);
            Assert.AreEqual(typeof(SortedDictionary<int, string>), valueResult.GetType());
            Assert.AreEqual(valueJson, valueJsonResult);
        }
    }
}
