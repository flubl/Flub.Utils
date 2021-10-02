using Moq;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;

namespace Flub.Utils.Json.Test
{
    [ExcludeFromCodeCoverage]
    public class JsonTypedConverterTest
    {
        const int BASE_TYPE = 0;
        const int SUB1_TYPE = 1;
        const int SUB2_TYPE = 2;
        const int SUB3_TYPE = 3;
        const int SUB4_TYPE = 4;
        const int SUB5_TYPE = 5;
        const int SUB6_TYPE = 5;
        const string BASE2_PROPERTY_NAME = "status";
        const string BASE2_TYPE = "0";
        const string SUB21_TYPE = "1";
        static readonly string SUB1_VALUE = Guid.NewGuid().ToString();
        static readonly Sub2.Inner SUB2_VALUE = new() { Text = Guid.NewGuid().ToString() };
        static readonly int[] SUB3_VALUES = Enumerable.Range(0, 3).ToArray();
        static readonly bool SUB4_VALUE1 = true;
        static readonly bool SUB4_VALUE2 = false;
        static readonly bool? SUB4_VALUE3 = null;
        static readonly JsonTypedConverter<Base, int> converter = new();
        static readonly JsonTypedConverter<Base2, string> converter2 = new();
        static readonly JsonSerializerOptions options = new();

        abstract class Base : IJsonTyped<int>
        {
            public int Type { get; } = BASE_TYPE;

            protected Base(int type)
            {
                Type = type;
            }
        }

        [JsonTyped(SUB1_TYPE)]
        class Sub1 : Base
        {
            public string Value { get; set; }

            public Sub1()
                : base(SUB1_TYPE)
            {

            }
        }

        [JsonTyped(SUB2_TYPE)]
        class Sub2 : Base
        {
            public Inner Value { get; set; }

            public Sub2()
                : base(SUB2_TYPE)
            {

            }

            public class Inner
            {
                public string Text { get; set; }
            }
        }

        [JsonTyped(SUB3_TYPE)]
        class Sub3 : Base
        {
            public int[] Values { get; set; }

            public Sub3()
                : base(SUB3_TYPE)
            {

            }
        }

        [JsonTyped(SUB4_TYPE)]
        class Sub4 : Base
        {
            public bool Value1 { get; set; }
            public bool Value2 { get; set; }
            public bool? Value3 { get; set; }

            public Sub4()
                : base(SUB4_TYPE)
            {

            }
        }

        [JsonTyped(SUB5_TYPE)]
        class Sub5: Base
        {
            public Sub5()
                : base(SUB5_TYPE)
            {

            }
        }

        [JsonTyped(SUB6_TYPE)]
        class Sub6 : Base
        {
            public Sub6()
                : base(SUB6_TYPE)
            {

            }
        }

        abstract class Base2 : IJsonTyped<string>
        {
            [JsonPropertyName(BASE2_PROPERTY_NAME)]
            public string Type { get; } = BASE2_TYPE;

            protected Base2(string type)
            {
                Type = type;
            }
        }
        
        [JsonTyped(SUB21_TYPE)]
        class Sub21 : Base2
        {
            public Sub21()
                : base(SUB21_TYPE)
            {

            }
        }

        static Utf8JsonReader GetReader(string json)
        {
            Utf8JsonReader result = new(Encoding.UTF8.GetBytes(json));
            result.Read();
            return result;
        }

        [Test]
        public void CanConvertTest()
        {
            JsonTypedConverter<IJsonTyped<int>, int> converterInterface = new();
            JsonTypedConverter<Base, int> converterClass = new();

            Assert.IsTrue(converterInterface.CanConvert(typeof(IJsonTyped<int>)));
            Assert.IsTrue(converterInterface.CanConvert(typeof(Base)));
            Assert.IsFalse(converterClass.CanConvert(typeof(IJsonTyped<int>)));
            Assert.IsTrue(converterClass.CanConvert(typeof(Base)));
            Assert.IsFalse(converterInterface.CanConvert(typeof(IJsonTyped<string>)));
            Assert.IsFalse(converterClass.CanConvert(typeof(IJsonTyped<string>)));
            Assert.IsFalse(converterInterface.CanConvert(typeof(int)));
            Assert.IsFalse(converterInterface.CanConvert(typeof(object)));
        }

        [Test]
        public void ReadSub1Test()
        {
            Utf8JsonReader reader = GetReader($"{{\"{nameof(Base.Type)}\":{SUB1_TYPE},\"{nameof(Sub1.Value)}\":\"{SUB1_VALUE}\"}}");

            Base result = converter.Read(ref reader, null, options);

            Assert.AreEqual(JsonTokenType.EndObject, reader.TokenType);
            Assert.AreEqual(typeof(Sub1), result.GetType());
            Assert.AreEqual(SUB1_TYPE, result.Type);
            Assert.AreEqual(SUB1_VALUE, ((Sub1)result).Value);
        }

        [Test]
        public void ReadSub2Test()
        {
            Utf8JsonReader reader = GetReader($"{{\"{nameof(Base.Type)}\":{SUB2_TYPE},\"{nameof(Sub2.Value)}\":{{\"{nameof(Sub2.Inner.Text)}\":\"{SUB2_VALUE.Text}\"}}}}");

            Base result = converter.Read(ref reader, null, options);

            Assert.AreEqual(JsonTokenType.EndObject, reader.TokenType);
            Assert.AreEqual(typeof(Sub2), result.GetType());
            Assert.AreEqual(SUB2_TYPE, result.Type);
            Assert.AreEqual(SUB2_VALUE.Text, ((Sub2)result).Value.Text);
        }

        [Test]
        public void ReadSub3Test()
        {
            Utf8JsonReader reader = GetReader($"{{\"{nameof(Base.Type)}\":{SUB3_TYPE},\"{nameof(Sub3.Values)}\":[{string.Join(",", SUB3_VALUES.Select(i => i.ToString()))}]}}");

            Base result = converter.Read(ref reader, null, options);

            Assert.AreEqual(JsonTokenType.EndObject, reader.TokenType);
            Assert.AreEqual(typeof(Sub3), result.GetType());
            Assert.AreEqual(SUB3_TYPE, result.Type);
            for (int i = 0; i < SUB3_VALUES.Length; i++)
                Assert.AreEqual(SUB3_VALUES[i], ((Sub3)result).Values[i]);
        }

        [Test]
        public void ReadSub4Test()
        {
            Utf8JsonReader reader = GetReader($"{{\"{nameof(Base.Type)}\":{SUB4_TYPE},"
                + $"\"{nameof(Sub4.Value1)}\":{SUB4_VALUE1.ToString().ToLower()},"
                + $"\"{nameof(Sub4.Value2)}\":{SUB4_VALUE2.ToString().ToLower()},"
                + $"\"{nameof(Sub4.Value3)}\":null"
                + $"}}");

            Base result = converter.Read(ref reader, null, options);

            Assert.AreEqual(JsonTokenType.EndObject, reader.TokenType);
            Assert.AreEqual(typeof(Sub4), result.GetType());
            Assert.AreEqual(SUB4_TYPE, result.Type);
            Assert.AreEqual(SUB4_VALUE1, ((Sub4)result).Value1);
            Assert.AreEqual(SUB4_VALUE2, ((Sub4)result).Value2);
            Assert.AreEqual(SUB4_VALUE3, ((Sub4)result).Value3);
        }

        [Test]
        public void ReadSub21Test()
        {
            Utf8JsonReader reader = GetReader($"{{\"{BASE2_PROPERTY_NAME}\":\"{SUB21_TYPE}\"}}");

            Base2 result = converter2.Read(ref reader, null, options);

            Assert.AreEqual(JsonTokenType.EndObject, reader.TokenType);
            Assert.AreEqual(typeof(Sub21), result.GetType());
            Assert.AreEqual(SUB21_TYPE, result.Type);
        }

        [Test]
        public void DeserializeTest()
        {
            JsonSerializerOptions options = new(JsonTypedConverterTest.options) { Converters = { new JsonTypedConverter<Base, int>() } };

            Base result = JsonSerializer.Deserialize<Base>($"{{\"{nameof(Base.Type)}\":{SUB1_TYPE},\"{nameof(Sub1.Value)}\":\"{SUB1_VALUE}\"}}", options);

            Assert.AreEqual(typeof(Sub1), result.GetType());
            Assert.AreEqual(SUB1_TYPE, result.Type);
            Assert.AreEqual(SUB1_VALUE, ((Sub1)result).Value);
        }

        [Test]
        public void ReadWrongTypeTest()
        {
            Assert.Throws<JsonException>(() =>
            {
                Utf8JsonReader reader = GetReader("123");
                converter.Read(ref reader, null, options);
            });
            Assert.Throws<JsonException>(() =>
            {
                Utf8JsonReader reader = GetReader("\"value\"");
                converter.Read(ref reader, null, options);
            });
            Assert.Throws<JsonException>(() =>
            {
                Utf8JsonReader reader = GetReader("[]");
                converter.Read(ref reader, null, options);
            });
            Assert.Throws<JsonException>(() =>
            {
                Utf8JsonReader reader = GetReader("null");
                converter.Read(ref reader, null, options);
            });
        }

        [Test]
        public void ReadNotFoundTest()
        {
            Assert.Throws<JsonException>(() =>
            {
                Utf8JsonReader reader = GetReader("{}");
                converter.Read(ref reader, null, options);
            });
            Assert.Throws<JsonException>(() =>
            {
                Utf8JsonReader reader = GetReader($"{{\"{nameof(Base.Type)}\":-1}}");
                converter.Read(ref reader, null, options);
            });
        }

        [Test]
        public void ReadMultipleFoundTest()
        {
            Assert.Throws<JsonException>(() =>
            {
                Utf8JsonReader reader = GetReader($"{{\"{nameof(Base.Type)}\":{SUB5_TYPE}}}");
                converter.Read(ref reader, null, options);
            });
            Assert.Throws<JsonException>(() =>
            {
                Utf8JsonReader reader = GetReader($"{{\"{nameof(Base.Type)}\":{SUB6_TYPE}}}");
                converter.Read(ref reader, null, options);
            });
        }
    }
}
