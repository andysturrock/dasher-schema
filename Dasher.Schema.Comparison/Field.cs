using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Dasher.Schema.Comparison
{
    public class Field
    {
        private readonly Dictionary<string, Field> _nameToField = new Dictionary<string, Field>();

        private Field(string name, string type, string defaultValue = null, IEnumerable<Field> subTypes = null)
        {
            Name = name;
            Type = type;
            DefaultValue = defaultValue;
            Fields = subTypes ?? new LinkedList<Field>();
            foreach (var field in Fields)
            {
                _nameToField[field.Name.ToLower()] = field;
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
        /// <see cref="Serializable.CompareTo(Serializable)"/>
        /// </summary>
        /// <param name="other"></param>
        public IEnumerable<FieldDifference> CompareTo(Field other)
        {
            var differences = new List<FieldDifference>();
            // Warning if capitalisation is different
            if (Name != other.Name)
            {
                differences.Add(
                    new FieldDifference(
                        this,
                        $"Fields have capitalisation difference: {Name} vs {other.Name}.",
                        FieldDifference.DifferenceLevelEnum.Warning));
            }
            // Check types
            if (Type != other.Type)
            {
                differences.Add(
                    new FieldDifference(
                        this,
                        $"Serialisable field {Name} has type {Type}, deserialisable field has type {other.Type}.",
                        FieldDifference.DifferenceLevelEnum.Critical));
            }
            // Check default values
            if (DefaultValue != other.DefaultValue)
            {
                differences.Add(
                    new FieldDifference(
                        this,
                        $"Serialisable field {Name} has default value {DefaultValue}, deserialisable has default value {other.DefaultValue}.",
                        FieldDifference.DifferenceLevelEnum.Warning));
            }

            // Check whether all fields in other exist in this
            foreach (var otherField in other.Fields)
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
                        differences.Add(
                            new FieldDifference(
                                otherField,
                                $"Serialisable does not contain a {otherField.Name} field, but deserialisable has a default value.",
                                FieldDifference.DifferenceLevelEnum.Warning));
                    }
                    else
                    {
                        differences.Add(
                            new FieldDifference(
                                otherField,
                                $"Serialisable does not contain a {otherField.Name} field, and deserialisable has no default value.",
                                FieldDifference.DifferenceLevelEnum.Critical));
                    }

                }
            }
            // Check whether all fields in this exist in other
            foreach (var thisField in Fields)
            {
                Field otherField;
                if (!other._nameToField.TryGetValue(thisField.Name.ToLower(), out otherField))
                {
                    differences.Add(
                        new FieldDifference(
                            thisField,
                            $"Serialisable contains a {thisField.Name} field, but deserialisable does not. Deserialisable must use Dasher " +
                            "UnexpectedFieldBehaviour.Ignore mode.",
                            FieldDifference.DifferenceLevelEnum.Warning));
                }
            }
            return differences;
        }

        public static Field ParseFrom(XElement xml)
        {
            if (xml.Name != "Field")
            {
                throw new ComparisonException($"Expected a Field element, got a {xml.Name}");
            }
            var name = xml.Attribute("name");
            if (name == null)
            {
                throw new ComparisonException("Field element has no attribute \"name\"");
            }
            var nameValue = name.Value;

            var type = xml.Attribute("type");
            if (type == null)
            {
                throw new ComparisonException($"Field element {nameValue} has no attribute \"type\"");
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
