namespace Flub.Utils.Json
{
    /// <summary>
    /// Specifies a value to a class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class JsonTypedAttribute : Attribute
    {
        /// <summary>
        /// Value of the class.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonTypedAttribute"/> class with the specified value.
        /// </summary>
        /// <param name="value">The value of the class.</param>
        public JsonTypedAttribute(object value)
        {
            Value = value;
        }
    }
}
