using System.Linq;
using System.IO;

namespace Dasher.Schema
{
    public enum UnsupportedOperationBehaviour
    {
        Throw,
        ReturnNull
    }

    public sealed class Serialiser<T>
    {
        private readonly Dasher.Serialiser<T> _inner;

        public void Serialise(Stream stream, T value) => _inner.Serialise(stream, value);
        public void Serialise(Packer packer, T value) => _inner.Serialise(packer, value);
        public byte[] Serialise(T value) => _inner.Serialise(value);
 
        public static Serialiser<T> GetSerialiser(UnsupportedOperationBehaviour unsupportedOperationBehaviour = UnsupportedOperationBehaviour.Throw)
        {
            var attributes = typeof(T).GetCustomAttributes(typeof(DasherSerialisableAttribute), true).Cast<DasherSerialisableAttribute>().ToList();
            if (attributes.Count >= 1 && attributes.All(a => a.Usage != SupportedOperations.DeserialiseOnly))
                return new Serialiser<T>();
            if (unsupportedOperationBehaviour == UnsupportedOperationBehaviour.ReturnNull)
                return null;
            throw new DasherSchemaException(
                "Type must have a DasherSerialisable attribute with usage parameter of SerialiseOnly or SerialiseDeserialise.", typeof(T));
        }

        private Serialiser()
        {
            _inner = new Dasher.Serialiser<T>();
        }
   }
}
