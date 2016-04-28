using System.Collections.Generic;
using System.Xml.Linq;

namespace Dasher.Schema.Comparison
{
    public class Message
    {
        private readonly Dictionary<string, Field> _nameToField = new Dictionary<string, Field>();

        public Message(string name, IEnumerable<Field> fields)
        {
            Name = name;
            Fields = fields;
            foreach (var field in Fields)
            {
                _nameToField[field.Name.ToLower()] = field;
            }
        }

        public string Name { get; }
        public IEnumerable<Field> Fields { get; }

        /// <summary>
        /// Compare this message with other and return any incompatibilities.
        /// Fields containing fields are compared recursively.
        /// For comparison purposes it is considered that this message is being sent,
        /// and other is being received.  Therefore the comparison is not commutative.
        /// Ie this.CompareTo(receiver) != receiver.CompareTo(this).
        /// </summary>
        /// <remarks>
        /// Messages are considered compatible if:
        /// <list>
        /// <item>All fields match exactly (case of field names is insensitive).</item>
        /// <item>All types match exactly (namespace and enclosing class names are removed for non System classes).</item>
        /// <item>All fields have the same default value.</item>
        /// </list>
        /// 
        /// Critical level difference will be created if:
        /// <list>
        /// <item>receiver contains fields that this Message doesn't, and doesn't have defaults for those fields.</item>
        /// <item>Field types don't match</item>
        /// </list>
        /// Warning level difference will be created if:
        /// <list>
        /// <item>receiver contains fields that this Message doesn't, and has defaults for those fields.</item>
        /// <item>this Message contains fields that receiver does not (1).</item>
        /// <item>Fields have same name and type but different default values.</item>
        /// </list>
        /// 
        /// (1) Using Dasher UnexpectedFieldBehaviour.Ignore mode makes this situation compatible
        ///     which is why it is only a warning.
        /// </remarks>
        /// <param name="receiver"></param>
        /// <returns>Collection of differences, empty if the messages are compatible</returns>
        public IEnumerable<FieldDifference> CompareTo(Message receiver)
        {
            var differences = new List<FieldDifference>();
            // Check whether all fields in receiver exist in this message
            foreach (var otherField in receiver.Fields)
            {
                Field thisField;
                if (_nameToField.TryGetValue(otherField.Name.ToLower(), out thisField))
                {
                    differences.AddRange(thisField.CompareTo(otherField));
                }
                else
                {
                    // If there is a default value, then just a warning
                    if (otherField.DefaultValue != null)
                    {
                        differences.Add(new FieldDifference(otherField, "Sender does not contain a " +
                            otherField.Name + " field, but receiver has a default value.",
                        FieldDifference.DifferenceLevelEnum.Warning));
                    }
                    else
                    {
                        differences.Add(new FieldDifference(otherField, "Sender does not contain a " +
                            otherField.Name + " field, and receiver has no default value.",
                        FieldDifference.DifferenceLevelEnum.Critical));
                    }

                }
            }
            // Check whether all fields in this exist in other
            foreach (var thisField in Fields)
            {
                Field otherField;
                if (!receiver._nameToField.TryGetValue(thisField.Name.ToLower(), out otherField))
                {
                    differences.Add(new FieldDifference(thisField, "Sender contains a " +
                            thisField.Name + " field, but receiver does not.  The receiver must use Dasher UnexpectedFieldBehaviour.Ignore mode.",
                        FieldDifference.DifferenceLevelEnum.Warning));
                }
            }
            return differences;
        }

        public static Message ParseFrom(XElement messageXML)
        {
            if (messageXML.Name != "Message")
            {
                throw new MessageComparisonException("messageXML should be a Message element");
            }
            var name = messageXML.Attribute("name");
            if (name == null)
            {
                throw new MessageComparisonException("Message element has no attribute \"name\"");
            }

            var subTypes = new LinkedList<Field>();
            if (messageXML.HasElements)
            {
                foreach (var element in messageXML.Elements())
                {
                    subTypes.AddLast(Field.ParseFrom(element));
                }
            }

            return new Message(name.Value, subTypes);
        }
    }
}
