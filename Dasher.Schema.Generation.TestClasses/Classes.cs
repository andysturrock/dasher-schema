using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dasher.Schema.Generation.TestClasses
{
    [DasherSerialisable(SupportedOperations.SerialiseOnly)]
    public class Sender
    {
    }

    [DasherSerialisable(SupportedOperations.DeserialiseOnly)]
    public class Receiver
    {
        
    }

    [DasherSerialisable(SupportedOperations.SerialiseDeserialise)]
    public class SenderAndReceiver
    {
        
    }

    [DasherSerialisable(SupportedOperations.SerialiseDeserialise)]
    public class BaseSenderReceiver
    {
        
    }

    [DasherSerialisable(SupportedOperations.SerialiseOnly)]
    public class DerivedSender : BaseSenderReceiver
    {
        
    }
}
