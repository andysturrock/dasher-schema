using System;

namespace Dasher.Schema
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
    public class SendMessageAttribute : System.Attribute
    {
        public SendMessageAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
    public class ReceiveMessageAttribute : System.Attribute
    {
        public ReceiveMessageAttribute()
        {
        }
    }
}
