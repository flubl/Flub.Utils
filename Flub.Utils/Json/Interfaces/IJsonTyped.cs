namespace Flub.Utils.Json
{
    /// <summary>
    /// Contains a property of the specified type with the name <see cref="Type"/>.
    /// </summary>
    /// <typeparam name="TType">The type of the <see cref="Type"/> property.</typeparam>
    public interface IJsonTyped<TType>
    {
        /// <summary>
        /// Type of the object.
        /// </summary>
        TType Type { get; }
    }
}
