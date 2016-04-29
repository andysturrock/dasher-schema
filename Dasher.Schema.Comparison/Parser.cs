using System.Collections.Generic;
using System.Xml.Linq;

namespace Dasher.Schema.Comparison
{
    public class Parser
    {
        private string SerialisableTypeElementTag { get; }

        public Parser(string serialisableTypeElementTag)
        {
            SerialisableTypeElementTag = serialisableTypeElementTag;
        }

        public Serializable ParseFrom(XElement xml)
        {
            if (xml.Name != SerialisableTypeElementTag)
            {
                throw new ComparisonException($"XML should be a {SerialisableTypeElementTag} element");
            }
            var name = xml.Attribute("name");
            if (name == null)
            {
                throw new ComparisonException($"{SerialisableTypeElementTag} element has no attribute \"name\"");
            }

            var subTypes = new LinkedList<Field>();
            if (!xml.HasElements)
                return new Serializable(name.Value, subTypes);
            foreach (var element in xml.Elements())
            {
                subTypes.AddLast(Field.ParseFrom(element));
            }

            return new Serializable(name.Value, subTypes);
        }
    }
}