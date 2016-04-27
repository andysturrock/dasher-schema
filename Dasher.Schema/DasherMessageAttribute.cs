using System;

namespace Dasher.Schema
{
    public enum MessageDirection
    {
        SendOnly,
        ReceiveOnly,
        SendReceive
    }

    /// <summary>
    /// Attribute for denoting a message class as being intended for serialisation or deserialisation by Dasher.
    /// </summary>
    /// <remarks>
    /// All classes that will be serialised/deserialised using Dasher must be tagged with this attribute, or an exception will be raised at (de)serialisation
    /// time.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
    public class DasherMessageAttribute : Attribute
    {
        public MessageDirection Usage { get; set; }
        public string Description { get; }

        /// <summary>
        /// Construct an attribute instance for denoting a message class as being supported for serialisation or deserialisation by Dasher.
        /// </summary>
        /// <param name="usage">Mandatory parameter indicating whether this class is intended for serialisation only, deserialisation only, or for either
        /// serialisation or deserialisation.</param>
        /// <param name="description">Optional text describing purpose of the tagged message type.</param>
        /// <remarks>
        /// The <paramref name="usage"/> parameter indicates whether the tagged class is intended for sending (serialisation), receipt (deserialisation) or
        /// both. Attempting to serialise a class that is tagged for deserialisation only, or vice versa, will result in an exception. The unidirectional cases
        /// make sense when a component sends or receives a message that has been defined within its own source code to have the structure expected by a peer
        /// component it communicates with. In this case there are two separate definitions of the message, one marked <c>SendOnly</c> and the other marked
        /// <c>ReceiveOnly</c>, in different component source structures. Where a single message definition is being shared by both peers (either via shared
        /// source code or an assembly reference to compiled code) then the message must be marked <c>SendReceive</c>.
        /// </remarks>
        public DasherMessageAttribute(MessageDirection usage, string description = null)
        {
            Usage = usage;
            Description = description;
        }
    }
}
