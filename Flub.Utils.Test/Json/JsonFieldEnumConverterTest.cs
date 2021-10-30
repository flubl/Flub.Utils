using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Flub.Utils.Json.Test
{
    [ExcludeFromCodeCoverage]
    public class JsonFieldEnumConverterTest
    {
        private const string EMPTY_JSON_STRING = "\"\"";

        [Test]
        public void ConverterFactoryCanConvertTest()
        {
            Assert.IsTrue(new JsonFieldEnumConverter().CanConvert(typeof(TestEnumWithoutConverter)));
            Assert.IsFalse(new JsonFieldEnumConverter().CanConvert(typeof(int)));
            Assert.IsFalse(new JsonFieldEnumConverter().CanConvert(typeof(string)));
            Assert.IsFalse(new JsonFieldEnumConverter().CanConvert(typeof(Enum)));
            Assert.IsFalse(new JsonFieldEnumConverter().CanConvert(typeof(TestClass)));
        }

        [Test]
        public void ConverterFactoryCreateConverter()
        {
            Assert.AreEqual(typeof(JsonFieldEnumConverter<TestEnumWithoutConverter>), new JsonFieldEnumConverter().CreateConverter(typeof(TestEnumWithoutConverter), null).GetType());
            Assert.Throws<ArgumentException>(() => new JsonFieldEnumConverter().CreateConverter(typeof(int), null));
            Assert.Throws<ArgumentException>(() => new JsonFieldEnumConverter().CreateConverter(typeof(string), null));
            Assert.Throws<ArgumentException>(() => new JsonFieldEnumConverter().CreateConverter(typeof(Enum), null));
            Assert.Throws<ArgumentException>(() => new JsonFieldEnumConverter().CreateConverter(typeof(TestClass), null));
        }

        [Test]
        public void ConverterInOptionsTest()
        {
            var options = new JsonSerializerOptions() { Converters = { new JsonFieldEnumConverter<TestEnumWithoutConverter>() } };

            var value = TestEnumWithoutConverter.Other;
            var valueJson = "\"other\"";

            var valueResult = JsonSerializer.Deserialize<TestEnumWithoutConverter>(valueJson, options);
            var valueJsonResult = JsonSerializer.Serialize(value, options);

            Assert.AreEqual(value, valueResult);
            Assert.AreEqual(valueJson, valueJsonResult);
        }

        [Test]
        public void ConverterFactoryInOptionsTest()
        {
            var options = new JsonSerializerOptions() { Converters = { new JsonFieldEnumConverter() } };

            var value = TestEnumWithoutConverter.Other;
            var valueJson = "\"other\"";

            var valueResult = JsonSerializer.Deserialize<TestEnumWithoutConverter>(valueJson, options);
            var valueJsonResult = JsonSerializer.Serialize(value, options);

            Assert.AreEqual(value, valueResult);
            Assert.AreEqual(valueJson, valueJsonResult);
        }

        [Test]
        public void ConverterInPropertyAttributeTest()
        {
            var value = new TestClass { Value = TestEnumWithoutConverter.Other };
            var valueJson = "{\"Value\":\"other\"}";

            var valueResult = JsonSerializer.Deserialize<TestClass>(valueJson);
            var valueJsonResult = JsonSerializer.Serialize(value);

            Assert.AreEqual(value.Value, valueResult.Value);
            Assert.AreEqual(valueJson, valueJsonResult);
        }

        [Test]
        public void ConverterFactoryInPropertyAttributeTest()
        {
            var value = new TestClassFactory { Value = TestEnumWithoutConverter.Other };
            var valueJson = "{\"Value\":\"other\"}";

            var valueResult = JsonSerializer.Deserialize<TestClassFactory>(valueJson);
            var valueJsonResult = JsonSerializer.Serialize(value);

            Assert.AreEqual(value.Value, valueResult.Value);
            Assert.AreEqual(valueJson, valueJsonResult);
        }

        [Test]
        public void ConverterInEnumAttributeTest()
        {
            var value = TestEnumWithConverter.Other;
            var valueJson = "\"other\"";

            var valueResult = JsonSerializer.Deserialize<TestEnumWithConverter>(valueJson);
            var valueJsonResult = JsonSerializer.Serialize(value);

            Assert.AreEqual(value, valueResult);
            Assert.AreEqual(valueJson, valueJsonResult);
        }

        [Test]
        public void ConverterFactoryInEnumAttributeTest()
        {
            var value = TestEnumWithConverterFactory.Other;
            var valueJson = "\"other\"";

            var valueResult = JsonSerializer.Deserialize<TestEnumWithConverterFactory>(valueJson);
            var valueJsonResult = JsonSerializer.Serialize(value);

            Assert.AreEqual(value, valueResult);
            Assert.AreEqual(valueJson, valueJsonResult);
        }

        [Test]
        public void ConverterInEnumAttributeWrongTest()
        {
            Assert.Throws<InvalidOperationException>(() => JsonSerializer.Serialize<TestEnumWithWrongConverter>(default));
            Assert.Throws<InvalidOperationException>(() => JsonSerializer.Deserialize<TestEnumWithWrongConverter>(string.Empty));
        }

        [Test]
        public void DuplicationThrowsExceptionValueTest()
        {
            static void AssertDuplicationThrowsException<T>() where T : Enum
            {
                Assert.Throws<InvalidOperationException>(() => JsonSerializer.Serialize<T>(default));
                Assert.Throws<InvalidOperationException>(() => JsonSerializer.Deserialize<T>(EMPTY_JSON_STRING));
            }

            AssertDuplicationThrowsException<TestEnumAttributeDuplicates>();
            AssertDuplicationThrowsException<TestEnumValueDuplicates>();
        }

        [Test]
        public void ValueSwitchTest()
        {
            var value1 = TestEnumNoDuplicates.Value1;
            var value1Json = "\"Value2\"";
            var value2 = TestEnumNoDuplicates.Value2;
            var value2Json = "\"Value1\"";

            var value1Result = JsonSerializer.Deserialize<TestEnumNoDuplicates>(value1Json);
            var value1JsonResult = JsonSerializer.Serialize(value1);
            var value2Result = JsonSerializer.Deserialize<TestEnumNoDuplicates>(value2Json);
            var value2JsonResult = JsonSerializer.Serialize(value2);

            Assert.AreEqual(value1, value1Result);
            Assert.AreEqual(value1Json, value1JsonResult);
            Assert.AreEqual(value2, value2Result);
            Assert.AreEqual(value2Json, value2JsonResult);
        }

        [Test]
        public void CorrectValueTest()
        {
            var valueIgnore = TestEnumWithIgnore.Ignore;
            var valueIgnoreJson = "\"Ignore\"";
            var valueNoChange = TestEnumWithIgnore.NoChange;
            var valueNoChangeJson = "\"NoChange\"";
            var valueChange = TestEnumWithIgnore.Change;
            var valueChangeJson = "\"new\"";
            var valueChangeNotFoundJson = "\"Change\"";

            var valueNullable = (TestEnumWithIgnore?)TestEnumWithIgnore.NoChange;
            var valueNullableJson = "\"NoChange\"";
            var valueNullableNull = (TestEnumWithIgnore?)null;
            var valueNullableNullJson = "null";

            Assert.Throws<JsonException>(() => JsonSerializer.Serialize(valueIgnore));
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<TestEnumWithIgnore>(valueIgnoreJson));
            var valueNoChangeJsonResult = JsonSerializer.Serialize(valueNoChange);
            var valueNoChangeResult = JsonSerializer.Deserialize<TestEnumWithIgnore>(valueNoChangeJson);
            var valueChangeJsonResult = JsonSerializer.Serialize(valueChange);
            var valueChangeResult = JsonSerializer.Deserialize<TestEnumWithIgnore>(valueChangeJson);
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<TestEnumWithIgnore>(valueChangeNotFoundJson));

            var valueNullableJsonResult = JsonSerializer.Serialize(valueNullable);
            var valueNullableResult = JsonSerializer.Deserialize<TestEnumWithIgnore?>(valueNullableJson);
            var valueNullableNullJsonResult = JsonSerializer.Serialize(valueNullableNull);
            var valueNullableNullResult = JsonSerializer.Deserialize<TestEnumWithIgnore?>(valueNullableNullJson);

            Assert.AreEqual(valueNoChange, valueNoChangeResult);
            Assert.AreEqual(valueNoChangeJson, valueNoChangeJsonResult);
            Assert.AreEqual(valueChange, valueChangeResult);
            Assert.AreEqual(valueChangeJson, valueChangeJsonResult);

            Assert.AreEqual(valueNullable, valueNullableResult);
            Assert.AreEqual(valueNullableJson, valueNullableJsonResult);
            Assert.AreEqual(valueNullableNull, valueNullableNullResult);
            Assert.AreEqual(valueNullableNullJson, valueNullableNullJsonResult);
        }

        [Test]
        public void EnumWithFlagsTest()
        {
            var value1 = TestEnumFlags.Value1;
            var value1Json = "\"value_1\"";
            var value3 = TestEnumFlags.Value1 | TestEnumFlags.Value2;
            var value3Json = "\"value_1,value_2\"";
            var value6 = TestEnumFlags.Value6;
            var value6Json = "\"value_6\"";
            var value6SplittedJson = "\"value_2,value_4\"";

            var value1JsonResult = JsonSerializer.Serialize(value1);
            var value1Result = JsonSerializer.Deserialize<TestEnumFlags>(value1Json);
            var value3JsonResult = JsonSerializer.Serialize(value3);
            var value3Result = JsonSerializer.Deserialize<TestEnumFlags>(value3Json);
            var value6JsonResult = JsonSerializer.Serialize(value6);
            var value6Result = JsonSerializer.Deserialize<TestEnumFlags>(value6Json);
            var value6SplittedResult = JsonSerializer.Deserialize<TestEnumFlags>(value6SplittedJson);

            Assert.AreEqual(value1, value1Result);
            Assert.AreEqual(value1Json, value1JsonResult);
            Assert.AreEqual(value3, value3Result);
            Assert.AreEqual(value3Json, value3JsonResult);
            Assert.AreEqual(value6, value6Result);
            Assert.AreEqual(value6Json, value6JsonResult);
            Assert.AreEqual(value6, value6SplittedResult);
        }

        [Test]
        public void FlagsSeperatorChangeTest()
        {
            var seperator = "|";
            var converter = new JsonFieldEnumConverter<TestEnumFlags>();

            Assert.AreEqual(",", converter.FlagsSeperator);

            converter.FlagsSeperator = seperator;
            Assert.AreEqual(seperator, converter.FlagsSeperator);

            Assert.Throws<ArgumentException>(() => converter.FlagsSeperator = null);
            Assert.Throws<ArgumentException>(() => converter.FlagsSeperator = string.Empty);
        }

        [Test]
        public void EnumWithFlagsChangedSeperatorTest()
        {
            var options = new JsonSerializerOptions { Converters = { new JsonFieldEnumConverter<TestEnumFlags>() { FlagsSeperator = "|" } } };

            var value = TestEnumFlags.Value1 | TestEnumFlags.Value2;
            var valueJson = "\"value_1|value_2\"";

            var valueJsonResult = JsonSerializer.Serialize(value, options);
            var valueResult = JsonSerializer.Deserialize<TestEnumFlags>(valueJson, options);

            Assert.AreEqual(value, valueResult);
            Assert.AreEqual(valueJson, valueJsonResult);
        }

        #region Enums

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

        [JsonConverter(typeof(JsonFieldEnumConverter))]
        enum TestEnumWithConverterFactory
        {
            [JsonFieldValue("none")]
            None,
            [JsonFieldValue("other")]
            Other
        }

        [JsonConverter(typeof(JsonFieldEnumConverter<TestEnumWithoutConverter>))]
        enum TestEnumWithWrongConverter
        {
            [JsonFieldValue("none")]
            None
        }

        class TestClass
        {
            [JsonConverter(typeof(JsonFieldEnumConverter<TestEnumWithoutConverter>))]
            public TestEnumWithoutConverter Value { get; set; }
        }

        class TestClassFactory
        {
            [JsonConverter(typeof(JsonFieldEnumConverter))]
            public TestEnumWithoutConverter Value { get; set; }
        }

        [JsonConverter(typeof(JsonFieldEnumConverter<TestEnumAttributeDuplicates>))]
        enum TestEnumAttributeDuplicates
        {
            None,
            [JsonFieldValue("value")]
            Value1,
            [JsonFieldValue("value")]
            Value2
        }

        [JsonConverter(typeof(JsonFieldEnumConverter<TestEnumValueDuplicates>))]
        enum TestEnumValueDuplicates
        {
            None,
            Value1,
            [JsonFieldValue("Value1")]
            Value2
        }

        [JsonConverter(typeof(JsonFieldEnumConverter<TestEnumNoDuplicates>))]
        enum TestEnumNoDuplicates
        {
            None,
            [JsonFieldValue("Value2")]
            Value1,
            [JsonFieldValue("Value1")]
            Value2
        }

        [JsonConverter(typeof(JsonFieldEnumConverter<TestEnumWithIgnore>))]
        enum TestEnumWithIgnore
        {
            [JsonIgnore]
            Ignore,
            NoChange,
            [JsonFieldValue("new")]
            Change
        }

        [JsonConverter(typeof(JsonFieldEnumConverter<TestEnumFlags>))]
        [Flags]
        enum TestEnumFlags
        {
            None = 0,
            [JsonFieldValue("value_1")]
            Value1 = 1,
            [JsonFieldValue("value_2")]
            Value2 = 2,
            [JsonFieldValue("value_4")]
            Value4 = 4,
            [JsonFieldValue("value_6")]
            Value6 = Value2 | Value4
        }

        #endregion
    }
}