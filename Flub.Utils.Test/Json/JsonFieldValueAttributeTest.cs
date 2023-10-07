using NUnit.Framework;
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
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.Throws<ArgumentException>(() => new JsonFieldValueAttribute(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.Throws<ArgumentException>(() => new JsonFieldValueAttribute(string.Empty));
        }

        [Test]
        public void GetCustomAttributeTest()
        {
            static void CheckValue(TestEnum value) =>
                typeof(TestEnum).GetField(value.ToString())?.GetCustomAttribute<JsonFieldValueAttribute>();

            Assert.DoesNotThrow(() => { CheckValue(TestEnum.Value); });
            Assert.Throws<ArgumentException>(() => { CheckValue(TestEnum.Null); });
            Assert.Throws<ArgumentException>(() => { CheckValue(TestEnum.Empty); });
        }

        enum TestEnum
        {
            [JsonFieldValue("Value")]
            Value,
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            [JsonFieldValue(null)]
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            Null,
            [JsonFieldValue("")]
            Empty
        }
    }
}
