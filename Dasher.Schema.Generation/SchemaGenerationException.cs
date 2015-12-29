using System;

namespace Dasher.Schema.Generation
{
    public sealed class SchemaGenerationException : Exception
    {
        public Type TargetType { get; }

        public SchemaGenerationException(string message, Type targetType)
            : base(message)
        {
            TargetType = targetType;
        }
    }
}
