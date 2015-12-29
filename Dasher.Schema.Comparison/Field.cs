using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Dasher.Schema.Comparison
{
    public class Field
    {
        private Dictionary<string, Field> nameToField = new Dictionary<string, Field>();

        public Field(string name, string type, string defaultValue = null, IEnumerable<Field> subTypes = null)
        {
            Name = name;
            Type = type;
            DefaultValue = defaultValue;
            Fields = subTypes;
            if (Fields == null)
                Fields = new LinkedList<Field>();
            foreach (var field in Fields)
            {
                nameToField[field.Name.ToLower()] = field;
            }
        }

        public string Name { get; }
        public string Type { get; }
        /// <summary>
        /// The default value for the field.
        /// Returns null if no default value.
        /// Returns the string "null" if the default value is null.
        /// </summary>
        public string DefaultValue { get; }
        public IEnumerable<Field> Fields { get; }

        /// <summary>
        /// <see cref="Message.CompareTo(Message)"/>
        /// 
        /// </summary>
        /// <param name="receiver"></param>
        /// <returns></returns>
        public IEnumerable<FieldDifference> CompareTo(Field receiver)
        {
            var differences = new List<FieldDifference>();
            // Warning if capitalisation is different
            if (this.Name != receiver.Name)
            {
                differences.Add(new FieldDifference(this, "Fields have capitalisation difference: " +
                    this.Name + " vs " + receiver.Name + ".",
                    FieldDifference.DifferenceLevelEnum.Warning));
            }
            // Check types
            if (this.Type != receiver.Type)
            {
                differences.Add(new FieldDifference(this, "Sender field " + this.Name
                    + " has type " + this.Type + ", receiver field has type " + receiver.Type + ".",
                    FieldDifference.DifferenceLevelEnum.Critical));
            }
            // Check default values
            if (this.DefaultValue != receiver.DefaultValue)
            {
                differences.Add(new FieldDifference(this, "Sender field " + this.Name +
                    " has default value " + this.DefaultValue + ", receiver has default value " +
                    receiver.DefaultValue + ".", FieldDifference.DifferenceLevelEnum.Warning));
            }

            // Check whether all fields in other exist in this
            foreach (var otherField in receiver.Fields)
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
                if (!receiver.nameToField.TryGetValue(thisField.Name.ToLower(), out otherField))
                {
                    differences.Add(new FieldDifference(thisField, "Sender contains a " +
                            thisField.Name + " field, but receiver does not.  The receiver must use Dasher UnexpectedFieldBehaviour.Ignore mode.",
                        FieldDifference.DifferenceLevelEnum.Warning));
                }
            }
            return differences;
        }

        public static Field ParseFrom(XElement xml)
        {
            if (xml.Name != "Field")
            {
                throw new MessageComparisonException("Expected a Field element, got a " + xml.Name);
            }
            var name = xml.Attribute("name");
            if (name == null)
            {
                throw new MessageComparisonException("Field element has no attribute \"name\"");
            }
            var nameValue = name.Value;

            var type = xml.Attribute("type");
            if (type == null)
            {
                throw new MessageComparisonException("Field element " + nameValue + " has no attribute \"type\"");
            }
            var typeValue = type.Value;
            // Strip off namespaces and containing types if not a system type
            if (!typeValue.StartsWith("System."))
            {
                typeValue = typeValue.Split('.').Last();
                typeValue = typeValue.Split('+').Last();
            }

            var defaultValue = xml.Attribute("default") == null ? null : xml.Attribute("default").Value;

            var subTypes = new LinkedList<Field>();
            if (xml.HasElements)
            {
                foreach (var element in xml.Elements())
                {
                    subTypes.AddLast(ParseFrom(element));
                }
            }

            return new Field(nameValue, typeValue, defaultValue, subTypes);
        }
    }
}
