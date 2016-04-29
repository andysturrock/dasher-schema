using System.Collections.Generic;

namespace Dasher.Schema.Comparison
{
    public class Serializable
    {
        private readonly Dictionary<string, Field> _nameToField = new Dictionary<string, Field>();

        public Serializable(string name, IEnumerable<Field> fields)
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
        /// Compare this serialisable object with another and return any incompatibilities.
        /// Fields containing fields are compared recursively.
        /// For comparison purposes it is considered that this object is being serialized,
        /// and other is being deserialized.  Therefore the comparison is not commutative.
        /// Ie this.CompareTo(deserialisable) != deserialisable.CompareTo(this).
        /// </summary>
        /// <remarks>
        /// Serialisables are considered compatible if:
        /// <list>
        /// <item>All fields match exactly (case of field names is insensitive).</item>
        /// <item>All types match exactly (namespace and enclosing class names are removed for non System classes).</item>
        /// <item>All fields have the same default value.</item>
        /// </list>
        /// 
        /// Critical level difference will be created if:
        /// <list>
        /// <item>deserialisable contains fields that this object doesn't, and doesn't have defaults for those fields.</item>
        /// <item>Field types don't match</item>
        /// </list>
        /// Warning level difference will be created if:
        /// <list>
        /// <item>deserialisable contains fields that this object doesn't, and has defaults for those fields.</item>
        /// <item>this object contains fields that deserialisable does not (1).</item>
        /// <item>Fields have same name and type but different default values.</item>
        /// </list>
        /// 
        /// (1) Using Dasher UnexpectedFieldBehaviour.Ignore mode makes this situation compatible
        ///     which is why it is only a warning.
        /// </remarks>
        /// <param name="deserialisable"></param>
        /// <returns>Collection of differences, empty if the objects are compatible</returns>
        public IEnumerable<FieldDifference> CompareTo(Serializable deserialisable)
        {
            var differences = new List<FieldDifference>();
            // Check whether all fields in deserialisable exist in this object
            foreach (var otherField in deserialisable.Fields)
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
            // Check whether all fields in this exist in deserialisable
            foreach (var thisField in Fields)
            {
                Field otherField;
                if (!deserialisable._nameToField.TryGetValue(thisField.Name.ToLower(), out otherField))
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
    }
}
