using System;

namespace Dasher.Schema
{
    public enum SupportedOperations
    {
        SerialiseOnly,
        DeserialiseOnly,
        SerialiseDeserialise
    }

    /// <summary>
    /// Attribute to enable schema-based serialisation or deserialisation support in provided by <c>Dasher.Schema</c>.
    /// </summary>
    /// <remarks>
    /// All types that will be serialised/deserialised via <see cref="Serialiser{T}"/> or <see cref="Deserialiser{T}"/>must be tagged with this attribute, or
    /// an exception will be raised at (de)serialisation time.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class DasherSerialisableAttribute : Attribute
    {
        public SupportedOperations Usage { get; }
        public string Description { get; }

        /// <summary>
        /// Construct an attribute that denotes that a type (<c>class</c> or <c>struct</c> only) may be serialised or deserialised using <c>Dasher.Schema</c>
        /// serialisation.
        /// </summary>
        /// <param name="usage">Required parameter indicating whether this type is intended for serialisation only, deserialisation only, or for either
        /// serialisation or deserialisation.</param>
        /// <param name="description">Optional text describing purpose of the tagged type.</param>
        /// <remarks>
        /// The <paramref name="usage"/> parameter indicates whether the tagged type is intended for serialisation, deserialisation or both. Attempting to
        /// serialise a type marked for deserialisation only, or vice versa, will result in an exception. The unidirectional cases make sense when a component
        /// serialises an object that will then be deserialised by another component. In this case there are two separate but compatible type definitions, one
        /// marked <c>SerialiseOnly</c> and the other marked <c>DeserialiseOnly</c>, one in each of the different component source structures. It is up to the
        /// developers of the two components to ensure that the two type declarations are compatible, as per the requirements of the underlying Dasher
        /// serialiser. Where a single type declaration is being shared by both components (either via shared source code or an assembly reference to compiled
        /// code) then mark the type <c>SerialiseDeserialise</c> so that it may be used in both roles.
        /// </remarks>
        public DasherSerialisableAttribute(SupportedOperations usage, string description = null)
        {
            Usage = usage;
            Description = description;
        }
    }
}
