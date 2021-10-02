using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Flub.Utils.Json
{
    public static class JsonSerializerOptionsExtension
    {
        /// <summary>
        /// Copies the options from a <see cref="JsonSerializerOptions"/> instance to a new instance without the specified types of converters.
        /// </summary>
        /// <param name="options">The options instance to copy options from.</param>
        /// <param name="converters">A list of types of converters to be removed in the copy.</param>
        /// <returns>Returns a copy of the given options.</returns>
        public static JsonSerializerOptions GetWithoutConverters(this JsonSerializerOptions options, params Type[] converters)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));
            if (converters is null)
                throw new ArgumentNullException(nameof(converters));
            JsonSerializerOptions result = new(options);
            foreach (JsonConverter converter in options.Converters.Where(i => converters.Any(c => i.GetType().IsAssignableTo(c))))
                result.Converters.Remove(converter);
            return result;
        }

        /// <summary>
        /// Copies the options from a <see cref="JsonSerializerOptions"/> instance to a new instance without the specified type of converter.
        /// </summary>
        /// <typeparam name="TConverter">The type of converter to be removed in the copy.</typeparam>
        /// <param name="options">The options instance to copy options from.</param>
        /// <returns>Returns a copy of the given options.</returns>
        public static JsonSerializerOptions GetWithoutConverter<TConverter>(this JsonSerializerOptions options) =>
            GetWithoutConverters(options, typeof(TConverter));
    }
}
