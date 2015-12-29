using System;

namespace Dasher.Schema.Comparison
{
    class MessageComparisonException : Exception
    {
        public MessageComparisonException(string message) : base(message)
        { }
    }
}
