using System;
using System.Reflection;

namespace Dasher.Schema.Generation
{
    public class AssemblyWalker
    {
        public static DasherAssemblyInfo GetDasherAssemblyInfo(Assembly assembly)
        {
            DasherAssemblyInfo result = new DasherAssemblyInfo();

            // TODO this is verbose.  Tidy up using Linq?
            foreach (Type t in assembly.GetTypes())
            {
                foreach (var attribute in t.GetCustomAttributes(false))
                {
                    if (attribute is SendMessageAttribute)
                    {
                        result.SendMessageTypes.Add(t);
                    }
                    if (attribute is ReceiveMessageAttribute)
                    {
                        result.ReceiveMessageTypes.Add(t);
                    }
                }
            }
            return result;
        }
    }
}