using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dasher.Schema.Generation.TestClasses
{
    [DasherMessage(MessageDirection.SendOnly)]
    public class Sender
    {
    }

    [DasherMessage(MessageDirection.ReceiveOnly)]
    public class Receiver
    {
        
    }

    [DasherMessage(MessageDirection.SendReceive)]
    public class SenderAndReceiver
    {
        
    }

    [DasherMessage(MessageDirection.SendReceive)]
    public class BaseSenderReceiver
    {
        
    }

    [DasherMessage(MessageDirection.SendOnly)]
    public class DerivedSender : BaseSenderReceiver
    {
        
    }
}
