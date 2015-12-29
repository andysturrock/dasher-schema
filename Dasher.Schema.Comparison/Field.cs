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

        public IEnumerable<FieldDifference> CompareTo(Field other)
        {
            var differences = new List<FieldDifference>();
            // Warning if capitalisation is different
            if (this.Name != other.Name)
            {
                differences.Add(new FieldDifference(this, "Fields have capitalisation difference: " +
                    this.Name + " vs " + other.Name + ".",
                    FieldDifference.DifferenceLevelEnum.Warning));
            }
            // Check types
            if (this.Type != other.Type)
            {
                differences.Add(new FieldDifference(this, "Field " + this.Name
                    + " has type " + this.Type + ", other field has type " + other.Type + ".",
                    FieldDifference.DifferenceLevelEnum.Critical));
            }
            // Check default values
            if (this.DefaultValue != other.DefaultValue)
            {
                differences.Add(new FieldDifference(this, "Field " + this.Name +
                    " has default value " + this.DefaultValue + ", other has default value " +
                    other.DefaultValue + ".", FieldDifference.DifferenceLevelEnum.Warning));
            }

            // Check whether all fields in other exist in this
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
