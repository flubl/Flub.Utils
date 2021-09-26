using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Flub.Utils.Json
{
    /// <summary>
    /// Specifies the field value that is present in the JSON when serializing and deserializing. This overrides any naming policy specified by <see cref="JsonNamingPolicy"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class JsonFieldValueAttribute : JsonAttribute
    {
        /// <summary>
        /// Gets the value of the field.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="JsonFieldValueAttribute"/> with the specified field value.
        /// </summary>
        /// <param name="value">The value of the field.</param>
        public JsonFieldValueAttribute(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("value can't be null or empty.", nameof(value));
            Value = value;
        }
    }
}
