using System;
using System.Collections.Generic;

namespace Dasher.Schema.Generation
{
    public class DasherAssemblyInfo
    {
        public DasherAssemblyInfo()
        {
            SerialisableTypes = new HashSet<Type>();
            DeserialisableTypes = new HashSet<Type>();
        }

        public HashSet<Type> SerialisableTypes { get; }
        public HashSet<Type> DeserialisableTypes { get; }
    }
}