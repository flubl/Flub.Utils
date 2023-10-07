using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

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
            public int Type { get; }

            protected Base(int type) { Type = type; }
        }

        [JsonTyped(SUB1_TYPE)]
        class Sub1 : Base
        {
            public string? Value { get; set; }

            public Sub1() : base(SUB1_TYPE) { }
        }

        [JsonTyped(SUB2_TYPE)]
        class Sub2 : Base
        {
            public Inner? Value { get; set; }

            public Sub2() : base(SUB2_TYPE) { }

            public class Inner
            {
                public string? Text { get; set; }
            }
        }

        [JsonTyped(SUB3_TYPE)]
        class Sub3 : Base
        {
            public int[]? Values { get; set; }

            public Sub3() : base(SUB3_TYPE) { }
        }

        [JsonTyped(SUB4_TYPE)]
        class Sub4 : Base
        {
            public bool? Value1 { get; set; }
            public bool? Value2 { get; set; }
            public bool? Value3 { get; set; }

            public Sub4() : base(SUB4_TYPE) { }
        }

        [JsonTyped(SUB5_TYPE)]
        class Sub5 : Base
        {
            public Sub5() : base(SUB5_TYPE) { }
        }

        [JsonTyped(SUB6_TYPE)]
        class Sub6 : Base
        {
            public Sub6() : base(SUB6_TYPE) { }
        }

        abstract class Base2 : IJsonTyped<string>
        {
            [JsonPropertyName(BASE2_PROPERTY_NAME)]
            public string Type { get; } = BASE2_TYPE;

            protected Base2(string type) { Type = type; }
        }

        [JsonTyped(SUB21_TYPE)]
        class Sub21 : Base2
        {
            public Sub21() : base(SUB21_TYPE) { }
        }

        abstract class BaseEmpty
        {

        }

        interface IBase : IJsonTyped<int>
        {

        }

        abstract class BaseWithIBase : IBase
        {
            public int Type { get; } = BASE_TYPE;

            protected BaseWithIBase(int type)
            {
                Type = type;
            }
        }

        class ClassWithPropertyAttributeFactory
        {
            [JsonConverter(typeof(JsonTypedConverter))]
            public Base? Value { get; set; }
        }

        class ClassWithWrongPropertyAttributeFactory
        {
            [JsonConverter(typeof(JsonTypedConverter))]
            public int? Value { get; set; }
        }

        class ClassWithPropertyAttribute
        {
            [JsonConverter(typeof(JsonTypedConverter<Base, int>))]
            public Base? Value { get; set; }
        }

        class ClassWithWrongPropertyAttribute
        {
            [JsonConverter(typeof(JsonTypedConverter<Base, int>))]
            public int? Value { get; set; }
        }

        class ClassWithWrongGenericPropertyAttribute
        {
            [JsonConverter(typeof(JsonTypedConverter<Base2, string>))]
            public Base? Value { get; set; }
        }

        [JsonConverter(typeof(JsonTypedConverter))]
        abstract class BaseWithAttributeFactory : IJsonTyped<int>
        {
            public int Type { get; } = BASE_TYPE;

            protected BaseWithAttributeFactory(int type) { Type = type; }
        }

        [JsonTyped(SUB1_TYPE)]
        class Sub1WithAttributeFactory : BaseWithAttributeFactory
        {
            public string? Value { get; set; }

            public Sub1WithAttributeFactory() : base(SUB1_TYPE) { }
        }

        [JsonTyped(SUB2_TYPE)]
        class Sub2WithAttributeFactory : BaseWithAttributeFactory
        {
            public int? Value { get; set; }

            public Sub2WithAttributeFactory() : base(SUB2_TYPE) { }
        }

        [JsonConverter(typeof(JsonTypedConverter<BaseWithAttribute, int>))]
        abstract class BaseWithAttribute : IJsonTyped<int>
        {
            public int Type { get; } = BASE_TYPE;

            protected BaseWithAttribute(int type) { Type = type; }
        }

        [JsonTyped(SUB1_TYPE)]
        class Sub1WithAttribute : BaseWithAttribute
        {
            public string? Value { get; set; }

            public Sub1WithAttribute() : base(SUB1_TYPE) { }
        }

        [JsonTyped(SUB2_TYPE)]
        class Sub2WithAttribute : BaseWithAttribute
        {
            public int? Value { get; set; }

            public Sub2WithAttribute() : base(SUB2_TYPE) { }
        }

        static Utf8JsonReader GetReader(string json)
        {
            Utf8JsonReader result = new(Encoding.UTF8.GetBytes(json));
            result.Read();
            return result;
        }

        [Test]
        public void ConverterFactoryCanConvertTest()
        {
            Assert.IsTrue(new JsonTypedConverter().CanConvert(typeof(Base)));
            Assert.IsTrue(new JsonTypedConverter().CanConvert(typeof(IJsonTyped<int>)));
            Assert.IsTrue(new JsonTypedConverter().CanConvert(typeof(Sub1)));
            Assert.IsTrue(new JsonTypedConverter().CanConvert(typeof(Base2)));
            Assert.IsTrue(new JsonTypedConverter().CanConvert(typeof(IJsonTyped<string>)));
            Assert.IsTrue(new JsonTypedConverter().CanConvert(typeof(Sub21)));
            Assert.IsTrue(new JsonTypedConverter().CanConvert(typeof(IBase)));
            Assert.IsTrue(new JsonTypedConverter().CanConvert(typeof(BaseWithIBase)));
            Assert.IsFalse(new JsonTypedConverter().CanConvert(typeof(int)));
            Assert.IsFalse(new JsonTypedConverter().CanConvert(typeof(string)));
            Assert.IsFalse(new JsonTypedConverter().CanConvert(typeof(BaseEmpty)));
        }

        [Test]
        public void ConverterFactoryCreateConverterTest()
        {
            Assert.AreEqual(typeof(JsonTypedConverter<Base, int>), new JsonTypedConverter().CreateConverter(typeof(Base), JsonSerializerOptions.Default)?.GetType());
            Assert.AreEqual(typeof(JsonTypedConverter<IJsonTyped<int>, int>), new JsonTypedConverter().CreateConverter(typeof(IJsonTyped<int>), JsonSerializerOptions.Default)?.GetType());
            Assert.AreEqual(typeof(JsonTypedConverter<Sub1, int>), new JsonTypedConverter().CreateConverter(typeof(Sub1), JsonSerializerOptions.Default)?.GetType());
            Assert.AreEqual(typeof(JsonTypedConverter<Base2, string>), new JsonTypedConverter().CreateConverter(typeof(Base2), JsonSerializerOptions.Default)?.GetType());
            Assert.AreEqual(typeof(JsonTypedConverter<IJsonTyped<string>, string>), new JsonTypedConverter().CreateConverter(typeof(IJsonTyped<string>), JsonSerializerOptions.Default)?.GetType());
            Assert.AreEqual(typeof(JsonTypedConverter<Sub21, string>), new JsonTypedConverter().CreateConverter(typeof(Sub21), JsonSerializerOptions.Default)?.GetType());
            Assert.AreEqual(typeof(JsonTypedConverter<IBase, int>), new JsonTypedConverter().CreateConverter(typeof(IBase), JsonSerializerOptions.Default)?.GetType());
            Assert.AreEqual(typeof(JsonTypedConverter<BaseWithIBase, int>), new JsonTypedConverter().CreateConverter(typeof(BaseWithIBase), JsonSerializerOptions.Default)?.GetType());
            Assert.Throws<InvalidOperationException>(() => new JsonTypedConverter().CreateConverter(typeof(int), JsonSerializerOptions.Default));
            Assert.Throws<InvalidOperationException>(() => new JsonTypedConverter().CreateConverter(typeof(string), JsonSerializerOptions.Default));
            Assert.Throws<InvalidOperationException>(() => new JsonTypedConverter().CreateConverter(typeof(BaseEmpty), JsonSerializerOptions.Default));
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

            Base result = converter.Read(ref reader, typeof(Base), options);

            Assert.AreEqual(JsonTokenType.EndObject, reader.TokenType);
            Assert.AreEqual(typeof(Sub1), result.GetType());
            Assert.AreEqual(SUB1_TYPE, result.Type);
            Assert.AreEqual(SUB1_VALUE, ((Sub1)result).Value);
        }

        [Test]
        public void ReadSub2Test()
        {
            Utf8JsonReader reader = GetReader($"{{\"{nameof(Base.Type)}\":{SUB2_TYPE},\"{nameof(Sub2.Value)}\":{{\"{nameof(Sub2.Inner.Text)}\":\"{SUB2_VALUE.Text}\"}}}}");

            Base result = converter.Read(ref reader, typeof(Base), options);

            Assert.AreEqual(JsonTokenType.EndObject, reader.TokenType);
            Assert.AreEqual(typeof(Sub2), result.GetType());
            Assert.AreEqual(SUB2_TYPE, result.Type);
            Assert.AreEqual(SUB2_VALUE.Text, ((Sub2)result)?.Value?.Text);
        }

        [Test]
        public void ReadSub3Test()
        {
            Utf8JsonReader reader = GetReader($"{{\"{nameof(Base.Type)}\":{SUB3_TYPE},\"{nameof(Sub3.Values)}\":[{string.Join(",", SUB3_VALUES.Select(i => i.ToString()))}]}}");

            Base result = converter.Read(ref reader, typeof(Base), options);

            Assert.AreEqual(JsonTokenType.EndObject, reader.TokenType);
            Assert.AreEqual(typeof(Sub3), result.GetType());
            Assert.AreEqual(SUB3_TYPE, result.Type);
            for (int i = 0; i < SUB3_VALUES.Length; i++)
                Assert.AreEqual(SUB3_VALUES[i], (result as Sub3)?.Values?[i]);
        }

        [Test]
        public void ReadSub4Test()
        {
            Utf8JsonReader reader = GetReader($"{{\"{nameof(Base.Type)}\":{SUB4_TYPE},"
                + $"\"{nameof(Sub4.Value1)}\":{SUB4_VALUE1.ToString().ToLower()},"
                + $"\"{nameof(Sub4.Value2)}\":{SUB4_VALUE2.ToString().ToLower()},"
                + $"\"{nameof(Sub4.Value3)}\":null"
                + $"}}");

            Base result = converter.Read(ref reader, typeof(Base), options);

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

            Base2 result = converter2.Read(ref reader, typeof(Base2), options);

            Assert.AreEqual(JsonTokenType.EndObject, reader.TokenType);
            Assert.AreEqual(typeof(Sub21), result.GetType());
            Assert.AreEqual(SUB21_TYPE, result.Type);
        }

        [Test]
        public void DeserializeTest()
        {
            JsonSerializerOptions options = new(JsonTypedConverterTest.options) { Converters = { new JsonTypedConverter<Base, int>() } };

            Base? result = JsonSerializer.Deserialize<Base>($"{{\"{nameof(Base.Type)}\":{SUB1_TYPE},\"{nameof(Sub1.Value)}\":\"{SUB1_VALUE}\"}}", options);

            Assert.AreEqual(typeof(Sub1), result?.GetType());
            Assert.AreEqual(SUB1_TYPE, result?.Type);
            Assert.AreEqual(SUB1_VALUE, (result as Sub1)?.Value);
        }

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

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

#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        [Test]
        public void ConvertWithPropertyAttributeFactoryTest()
        {
            Sub1 sub1 = new() { Value = SUB1_VALUE };
            Sub2 sub2 = new() { Value = SUB2_VALUE };

            ClassWithPropertyAttributeFactory expected1 = new() { Value = sub1 };
            ClassWithPropertyAttributeFactory expected2 = new() { Value = sub2 };

            string json1 = JsonSerializer.Serialize(expected1);
            string json2 = JsonSerializer.Serialize(expected2);

            ClassWithPropertyAttributeFactory? actual1 = JsonSerializer.Deserialize<ClassWithPropertyAttributeFactory>(json1);
            ClassWithPropertyAttributeFactory? actual2 = JsonSerializer.Deserialize<ClassWithPropertyAttributeFactory>(json2);

            Assert.AreEqual(typeof(Sub1), actual1?.Value?.GetType());
            Assert.AreEqual(SUB1_VALUE, (actual1?.Value as Sub1)?.Value);

            Assert.AreEqual(typeof(Sub2), actual2?.Value?.GetType());
            Assert.AreEqual(SUB2_VALUE.Text, (actual2?.Value as Sub2)?.Value?.Text);
        }

        [Test]
        public void ConvertWithPropertyAttributeTest()
        {
            Sub1 sub1 = new() { Value = SUB1_VALUE };
            Sub2 sub2 = new() { Value = SUB2_VALUE };

            ClassWithPropertyAttribute expected1 = new() { Value = sub1 };
            ClassWithPropertyAttribute expected2 = new() { Value = sub2 };

            string json1 = JsonSerializer.Serialize(expected1);
            string json2 = JsonSerializer.Serialize(expected2);

            ClassWithPropertyAttribute? actual1 = JsonSerializer.Deserialize<ClassWithPropertyAttribute>(json1);
            ClassWithPropertyAttribute? actual2 = JsonSerializer.Deserialize<ClassWithPropertyAttribute>(json2);

            Assert.AreEqual(typeof(Sub1), actual1?.Value?.GetType());
            Assert.AreEqual(SUB1_VALUE, (actual1?.Value as Sub1)?.Value);

            Assert.AreEqual(typeof(Sub2), actual2?.Value?.GetType());
            Assert.AreEqual(SUB2_VALUE.Text, (actual2?.Value as Sub2)?.Value?.Text);
        }

        [Test]
        public void ConvertWithPropertyAttributeThrowsExceptionsTest()
        {
            Assert.Throws<InvalidOperationException>(() => JsonSerializer.Serialize(new ClassWithWrongPropertyAttributeFactory()));
            Assert.Throws<InvalidOperationException>(() => JsonSerializer.Deserialize<ClassWithWrongPropertyAttributeFactory>("{}"));
            Assert.Throws<InvalidOperationException>(() => JsonSerializer.Serialize(new ClassWithWrongPropertyAttribute()));
            Assert.Throws<InvalidOperationException>(() => JsonSerializer.Deserialize<ClassWithWrongPropertyAttribute>("{}"));
            Assert.Throws<InvalidOperationException>(() => JsonSerializer.Serialize(new ClassWithWrongGenericPropertyAttribute()));
            Assert.Throws<InvalidOperationException>(() => JsonSerializer.Deserialize<ClassWithWrongGenericPropertyAttribute>("{}"));
        }

        [Test]
        public void ConvertWithAttributeFactoryTest()
        {
            string valueSub1 = Guid.NewGuid().ToString();
            int valueSub2 = 12;

            Sub1WithAttributeFactory expectedSub1 = new() { Value = valueSub1 };
            Sub2WithAttributeFactory expectedSub2 = new() { Value = valueSub2 };

            string jsonSub1 = JsonSerializer.Serialize(expectedSub1);
            string jsonSub2 = JsonSerializer.Serialize(expectedSub2);

            BaseWithAttributeFactory? actualSub1 = JsonSerializer.Deserialize<BaseWithAttributeFactory>(jsonSub1);
            BaseWithAttributeFactory? actualSub2 = JsonSerializer.Deserialize<BaseWithAttributeFactory>(jsonSub2);

            Assert.AreEqual(typeof(Sub1WithAttributeFactory), actualSub1?.GetType());
            Assert.AreEqual(valueSub1, (actualSub1 as Sub1WithAttributeFactory)?.Value);

            Assert.AreEqual(typeof(Sub2WithAttributeFactory), actualSub2?.GetType());
            Assert.AreEqual(valueSub2, (actualSub2 as Sub2WithAttributeFactory)?.Value);
        }

        [Test]
        public void ConvertWithAttributeTest()
        {
            string valueSub1 = Guid.NewGuid().ToString();
            int valueSub2 = 12;

            Sub1WithAttribute expectedSub1 = new() { Value = valueSub1 };
            Sub2WithAttribute expectedSub2 = new() { Value = valueSub2 };

            string jsonSub1 = JsonSerializer.Serialize(expectedSub1);
            string jsonSub2 = JsonSerializer.Serialize(expectedSub2);

            BaseWithAttribute? actualSub1 = JsonSerializer.Deserialize<BaseWithAttribute>(jsonSub1);
            BaseWithAttribute? actualSub2 = JsonSerializer.Deserialize<BaseWithAttribute>(jsonSub2);

            Assert.AreEqual(typeof(Sub1WithAttribute), actualSub1?.GetType());
            Assert.AreEqual(valueSub1, (actualSub1 as Sub1WithAttribute)?.Value);

            Assert.AreEqual(typeof(Sub2WithAttribute), actualSub2?.GetType());
            Assert.AreEqual(valueSub2, (actualSub2 as Sub2WithAttribute)?.Value);
        }
    }
}
