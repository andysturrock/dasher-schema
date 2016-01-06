using System;
using System.Collections.Generic;

namespace Dasher.Schema.Generation
{
    public class DasherAssemblyInfo
    {
        public DasherAssemblyInfo()
        {
            SendMessageTypes = new HashSet<Type>();
            ReceiveMessageTypes = new HashSet<Type>();
        }

        public HashSet<Type> SendMessageTypes { get; }
        public HashSet<Type> ReceiveMessageTypes { get; }
    }
}