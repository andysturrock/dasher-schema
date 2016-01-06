using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dasher.Schema.Generation.TestClasses
{
    [SendMessage]
    public class Sender
    {
    }

    [ReceiveMessage]
    public class Receiver
    {
        
    }

    [SendMessage]
    [ReceiveMessage]
    public class SenderAndReceiver
    {
        
    }

    [SendMessage]
    [ReceiveMessage]
    public class BaseSenderReceiver
    {
        
    }

    [SendMessage]
    public class DerivedSender : BaseSenderReceiver
    {
        
    }
}
