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
    public sealed class Serialiser<T>
    {
        private readonly Dasher.Serialiser<T> _inner;

        public Serialiser()
        {
            var attributes = typeof(T).GetCustomAttributes(typeof(DasherMessageAttribute), true).Cast<DasherMessageAttribute>().ToList();
            if (attributes.Count < 1 || attributes.Any(a => a.Usage == MessageDirection.ReceiveOnly))
            {
                throw new DasherSchemaException("Type must have a DasherMessage attribute with usage parameter of SendOnly or SendReceive.", typeof(T));
            }
            _inner = new Dasher.Serialiser<T>();
        }

        public void Serialise(Stream stream, T value) => _inner.Serialise(stream, value);
        public void Serialise(UnsafePacker packer, T value) => _inner.Serialise(packer, value);
        public byte[] Serialise(T value) => _inner.Serialise(value);
    }
}
