using System.Linq;
using System.IO;

namespace Dasher.Schema
{
    public sealed class Deserialiser<T>
    {
        private readonly Dasher.Deserialiser<T> _inner;

        public T Deserialise(byte[] bytes) => (T)_inner.Deserialise(bytes);
        public T Deserialise(Stream stream) => (T)_inner.Deserialise(stream);

        public static Deserialiser<T> GetDeserialiser(
            UnexpectedFieldBehaviour unexpectedFieldBehaviour = UnexpectedFieldBehaviour.Throw,
            UnsupportedOperationBehaviour unsupportedOperationBehaviour = UnsupportedOperationBehaviour.Throw)
        {
            var attributes = typeof(T).GetCustomAttributes(typeof(DasherSerialisableAttribute), true).Cast<DasherSerialisableAttribute>().ToList();
            if (attributes.Count >= 1 && attributes.All(a => a.Usage != SupportedOperations.SerialiseOnly))
                return new Deserialiser<T>(unexpectedFieldBehaviour);
            if (unsupportedOperationBehaviour == UnsupportedOperationBehaviour.ReturnNull)
                return null;
            throw new DasherSchemaException(
                "Type must have a DasherSerialisable attribute with usage parameter of DeserialiseOnly or SerialiseDeserialise.", typeof(T));
        }

        private Deserialiser(UnexpectedFieldBehaviour unexpectedFieldBehaviour)
        {
            _inner = new Dasher.Deserialiser<T>(unexpectedFieldBehaviour);
        }
    }
}
