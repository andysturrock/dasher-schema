using System;

namespace Dasher.Schema
{
    public abstract class SynergyMessageBaseAttribute : Attribute
    {
        protected SynergyMessageBaseAttribute()
        {
        }

        protected SynergyMessageBaseAttribute(string description)
        {
            Description = description;
        }

        public string Description { get; }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
    public class SendMessageAttribute : SynergyMessageBaseAttribute
    {
        public SendMessageAttribute()
        {
        }

        public SendMessageAttribute(string description) : base(description)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
    public class ReceiveMessageAttribute : SynergyMessageBaseAttribute
    {
        public ReceiveMessageAttribute()
        {
        }

        public ReceiveMessageAttribute(string description) : base(description)
        {
        }
    }
}
