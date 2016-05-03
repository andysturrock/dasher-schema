using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dasher.Schema.Generation.TestClasses
{
    [DasherSerialisable(SupportedOperations.SerialiseOnly)]
    public class DummySerialisable
    {
    }

    [DasherSerialisable(SupportedOperations.DeserialiseOnly)]
    public class DummyDeserialiseOnly
    {
        
    }

    [DasherSerialisable(SupportedOperations.SerialiseDeserialise)]
    public class DummySerialiseDeserialise
    {
        
    }

    [DasherSerialisable(SupportedOperations.SerialiseDeserialise)]
    public class BaseSerialiseDeserialise
    {
        
    }

    [DasherSerialisable(SupportedOperations.SerialiseOnly)]
    public class DerivedSerialiseOnly : BaseSerialiseDeserialise
    {
        
    }
}
