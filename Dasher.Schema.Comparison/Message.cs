using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Dasher.Schema.Comparison
{
    public class Message
    {
        private Dictionary<string, Field> nameToField = new Dictionary<string, Field>();

        public Message(string name, IEnumerable<Field> fields)
        {
            Name = name;
            Fields = fields;
            foreach (var field in Fields)
            {
                nameToField[field.Name.ToLower()] = field;
            }
        }

        public string Name { get; }
        public IEnumerable<Field> Fields { get; }

        /// <summary>
        /// Try to get a field with the given name.  Case is insensitive.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="field">out param containing found field</param>
        /// <returns>True if a field is found with the given name, otherwise false</returns>
        public bool TryGetField(string name, out Field field)
        {
            return nameToField.TryGetValue(name.ToLower(), out field);
        }

        /// <summary>
        /// Compare this message with other and return any incompatibilities.
        /// Fields containing fields are compared recursively.
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
        /// <item>other contains fields that this Message doesn't, and doesn't have defaults for those fields.</item>
        /// <item>Field types don't match</item>
        /// </list>
        /// Warning level difference will be created if:
        /// <list>
        /// <item>other contains fields that this Message doesn't, and has defaults for those fields.</item>
        /// <item>this Message contains fields than other does not (1).</item>
        /// <item>Fields have same name and type but different default values.</item>
        /// </list>
        /// 
        /// (1) Using Dasher UnexpectedFieldBehaviour.Ignore mode makes this situation compatible
        ///     which is why it is only a warning.
        /// </remarks>
        /// <param name="other"></param>
        /// <returns>Collection of differences, empty if the messages are compatible</returns>
        public IEnumerable<FieldDifference> CompareTo(Message other)
        {
            var differences = new List<FieldDifference>();
            // Check whether all fields in other exist in this message
            foreach (var otherField in other.Fields)
            {
                Field thisField;
                if (nameToField.TryGetValue(otherField.Name.ToLower(), out thisField))
                {
                    differences.AddRange(thisField.CompareTo(otherField));
                }
                else
                {
                    // If there is a default value, then just a warning
                    if (otherField.DefaultValue != null)
                    {
                        differences.Add(new FieldDifference(otherField, "This does not contain a " +
                            otherField.Name + " field, but other has a default value.",
                        FieldDifference.DifferenceLevelEnum.Warning));
                    }
                    else
                    {
                        differences.Add(new FieldDifference(otherField, "This does not contain a " +
                            otherField.Name + " field, and other has no default value.",
                        FieldDifference.DifferenceLevelEnum.Critical));
                    }

                }
            }
            // Check whether all fields in this exist in other
            foreach (var thisField in Fields)
            {
                Field otherField;
                if (!other.nameToField.TryGetValue(thisField.Name.ToLower(), out otherField))
                {
                    differences.Add(new FieldDifference(thisField, "This contains a " +
                            thisField.Name + " field, but other does not.  The other consumer must use Dasher UnexpectedFieldBehaviour.Ignore mode.",
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
