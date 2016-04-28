using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dasher.Schema;
using System.IO;
using System.Diagnostics;

namespace Dasher.Schema
{
    public enum UnexpectedFieldBehaviour
    {
        Throw,
        Ignore
    }

    public sealed class Deserialiser<T>
    {
        private readonly Dasher.Deserialiser<T> _inner;

        public Deserialiser(UnexpectedFieldBehaviour unexpectedFieldBehaviour = Schema.UnexpectedFieldBehaviour.Throw)
        {
            var attributes = typeof(T).GetCustomAttributes(typeof(DasherSerialisableAttribute), true).Cast<DasherSerialisableAttribute>().ToList();
            if (attributes.Count < 1 || attributes.Any(a => a.Usage == SupportedOperations.SerialiseOnly))
            {
                throw new DasherSchemaException(
                    "Type must have a DasherSerialisable attribute with usage parameter of DeserialiseOnly or SerialiseDeserialise.", typeof(T));
            }
            _inner = new Dasher.Deserialiser<T>((Dasher.UnexpectedFieldBehaviour)unexpectedFieldBehaviour);
        }

        public T Deserialise(byte[] bytes) => (T)_inner.Deserialise(bytes);
        public T Deserialise(Stream stream) => (T)_inner.Deserialise(stream);
    }
}
