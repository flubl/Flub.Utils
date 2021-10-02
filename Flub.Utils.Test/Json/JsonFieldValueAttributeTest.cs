using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Flub.Utils.Json.Test
{
    [ExcludeFromCodeCoverage]
    public class JsonFieldValueAttributeTest
    {
        [Test]
        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new JsonFieldValueAttribute("Value"));
            Assert.Throws<ArgumentException>(() => new JsonFieldValueAttribute(null));
            Assert.Throws<ArgumentException>(() => new JsonFieldValueAttribute(string.Empty));
        }

        [Test]
        public void GetCustomAttributeTest()
        {
            static void CheckValue(TestEnum value) =>
                typeof(TestEnum).GetField(value.ToString()).GetCustomAttribute<JsonFieldValueAttribute>();

            Assert.DoesNotThrow(() => { CheckValue(TestEnum.Value); });
            Assert.Throws<ArgumentException>(() => { CheckValue(TestEnum.Null); });
            Assert.Throws<ArgumentException>(() => { CheckValue(TestEnum.Empty); });
        }

        enum TestEnum
        {
            [JsonFieldValue("Value")]
            Value,
            [JsonFieldValue(null)]
            Null,
            [JsonFieldValue("")]
            Empty
        }
    }
}
