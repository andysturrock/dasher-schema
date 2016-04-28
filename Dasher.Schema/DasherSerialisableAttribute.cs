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
    /// All classes that will be serialised/deserialised via <see cref="Serialiser{T}"/> or <see cref="Deserialiser{T}"/>must be tagged with this attribute, or
    /// an exception will be raised at (de)serialisation time.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
    public class DasherSerialisableAttribute : Attribute
    {
        public SupportedOperations Usage { get; set; }
        public string Description { get; }

        /// <summary>
        /// Construct an attribute that denotes that a class may be serialised or deserialised by <c>Dasher.Schema</c>.
        /// </summary>
        /// <param name="usage">Required parameter indicating whether this class is intended for serialisation only, deserialisation only, or for either
        /// serialisation or deserialisation.</param>
        /// <param name="description">Optional text describing purpose of the tagged class.</param>
        /// <remarks>
        /// The <paramref name="usage"/> parameter indicates whether the tagged class is intended for serialisation, deserialisation or both. Attempting to
        /// serialise a class marked for deserialisation only, or vice versa, will result in an exception. The unidirectional cases make sense when a component
        /// serialises an object that will then be deserialised by another component. In this case there are two separate but compatible class definitions, one
        /// marked <c>SerialiseOnly</c> and the other marked <c>DeserialiseOnly</c>, one in each of the different component source structures. It is up to the
        /// developers of the two components to ensure that the two class declarations are compatible, as per the requirements of the underlying Dasher
        /// serialiser. Where a single class declaration is being shared by both components (either via shared source code or an assembly reference to compiled
        /// code) then mark the class <c>SerialiseDeserialise</c> so that it may be used in both roles.
        /// </remarks>
        public DasherSerialisableAttribute(SupportedOperations usage, string description = null)
        {
            Usage = usage;
            Description = description;
        }
    }
}
