using System;
using System.Runtime.Serialization;

namespace Dasher.Schema
{
    [Serializable]
    public class DasherSchemaException : Exception
    {
        public Type TargetType { get; }

        public DasherSchemaException(string message, Type targetType)
            : base(message)
        {
            TargetType = targetType;
        }
    }
}