﻿using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace Dasher.Schema.Comparison.Tests
{
    public class SchemaComparisonTests
    {
        #region Test types
        private XElement simpleMessage =
                        new XElement("Message", new XAttribute("name", "UserScore"),
                            new XElement("Field",
                                new XAttribute("name", "Score"),
                                new XAttribute("type", "System.Int32")),
                            new XElement("Field",
                                new XAttribute("name", "Name"),
                                new XAttribute("type", "System.String"))
                                );
        // field order reversed, field names different case (will give warning)
        private XElement compatibleSimpleMessage =
                        new XElement("Message", new XAttribute("name", "UserScore"),
                            new XElement("Field",
                                new XAttribute("name", "naMe"),
                                new XAttribute("type", "System.String")),
                            new XElement("Field",
                                new XAttribute("name", "scOre"),
                                new XAttribute("type", "System.Int32"))
                                );

        private XElement messageWithEnum =
                new XElement("Message", new XAttribute("name", "UserScore"),
                    new XElement("Field",
                        new XAttribute("name", "Score"),
                        new XAttribute("type", "System.Int32")),
                    new XElement("Field",
                        new XAttribute("name", "Enum"),
                        new XAttribute("type", "Dasher.SchemaComparison.Tests.XMLSchemaGeneratorTests+TestEnum"))
                        );

        private XElement messageWithNestedComplexTypesAndEnum =
                        new XElement("Message", new XAttribute("name", "TypeWithComplexType"),
                            new XElement("Field",
                                new XAttribute("name", "Enum"),
                                new XAttribute("type", "Dasher.SchemaComparison.Tests.XMLSchemaGeneratorTests+TestEnum"),
                                new XAttribute("default", "Bar")),
                            new XElement("Field",
                                new XAttribute("name", "Complex1"),
                                new XAttribute("type", "Dasher.SchemaGeneration.Tests.XMLSchemaGeneratorTests+UserScore"),
                                new XAttribute("default", "null"),
                                    new XElement("Field",
                                        new XAttribute("name", "XYZ"),
                                        new XAttribute("type", "System.String")),
                                    new XElement("Field",
                                        new XAttribute("name", "ABC"),
                                        new XAttribute("type", "System.Int32")),
                                    new XElement("Field",
                                        new XAttribute("name", "Complex2"),
                                        new XAttribute("type", "Dasher.SchemaGeneration.Tests.XMLSchemaGeneratorTests+SubUserScore"),
                                        new XAttribute("default", "null"),
                                            new XElement("Field",
                                                new XAttribute("name", "subXYZ"),
                                                new XAttribute("type", "System.String")),
                                            new XElement("Field",
                                                new XAttribute("name", "subABC"),
                                                new XAttribute("type", "System.Int32"))
                                        )),
                            new XElement("Field",
                                new XAttribute("name", "Bool"),
                                new XAttribute("type", "System.Boolean"),
                                new XAttribute("default", "true")),
                            new XElement("Field",
                                new XAttribute("name", "ExtraBool"),
                                new XAttribute("type", "System.Boolean"),
                                new XAttribute("default", "true"))
                                );

        private XElement incompatibleMessageWithNestedComplexTypesAndEnum =
                        new XElement("Message", new XAttribute("name", "TypeWithComplexType"),
                            new XElement("Field",
                                new XAttribute("name", "Enum"),
                                new XAttribute("type", "Dasher.SchemaComparison.Tests.XMLSchemaGeneratorTests+TestEnum"),
                                new XAttribute("default", "Bar")),
                            new XElement("Field",
                                new XAttribute("name", "ExtraFieldWithNoDefault"),  // (1) Extra field, no default value
                                new XAttribute("type", "Dasher.SchemaComparison.Tests.XMLSchemaGeneratorTests+TestEnum")),
                            new XElement("Field",
                                new XAttribute("name", "Complex1"),
                                new XAttribute("type", "Other.NameSpace.EnclosingClasss+UserScore"),
                                new XAttribute("default", "null"),
                                    new XElement("Field",
                                        new XAttribute("name", "xyz"),  // (2) Capitalisation difference (warning)
                                        new XAttribute("type", "System.String")),
                                    new XElement("Field",
                                        new XAttribute("name", "ExtraSubFieldWithNoDefault"),   // (3) Extra field, no default value
                                        new XAttribute("type", "System.String")),
                                    new XElement("Field",
                                        new XAttribute("name", "ABC"),
                                        new XAttribute("type", "System.Int64")),    // (4) Different type
                                    new XElement("Field",
                                        new XAttribute("name", "Complex2"),
                                        new XAttribute("type", "Dasher.SchemaGeneration.Tests.XMLSchemaGeneratorTests+SubUserScore"),
                                        new XAttribute("default", "null"),
                                            new XElement("Field",
                                                new XAttribute("name", "subXYZ"),
                                                new XAttribute("type", "System.String")),
                                            new XElement("Field",
                                                new XAttribute("name", "subXYZWithDefault"),    // (5) Extra field, but has default (warning)
                                                new XAttribute("type", "System.String"),
                                                new XAttribute("default", "default value")),
                                            new XElement("Field",
                                                new XAttribute("name", "subABC"),
                                                new XAttribute("type", "System.Int32"))
                                        )),
                            new XElement("Field",
                                new XAttribute("name", "Bool"),
                                new XAttribute("type", "System.Boolean"),
                                new XAttribute("default", "true"))
                                // (6) No ExtraBool field (warning)
                                );
        #endregion

        [Fact]
        public void ParseSimpleType()
        {
            var actual = Message.ParseFrom(simpleMessage);

            Assert.Equal("UserScore", actual.Name);
            var fields = actual.Fields.ToArray();

            Assert.Equal("Score", fields[0].Name);
            Assert.Equal("System.Int32", fields[0].Type);
            Assert.Equal(null, fields[0].DefaultValue);

            Assert.Equal("Name", fields[1].Name);
            Assert.Equal("System.String", fields[1].Type);
            Assert.Equal(null, fields[1].DefaultValue);
        }

        [Fact]
        public void ParseTypeWithEnum()
        {
            var actual = Message.ParseFrom(messageWithEnum);

            Assert.Equal("UserScore", actual.Name);
            var fields = actual.Fields.ToArray();

            Assert.Equal("Score", fields[0].Name);
            Assert.Equal("System.Int32", fields[0].Type);
            Assert.Equal(null, fields[0].DefaultValue);

            Assert.Equal("Enum", fields[1].Name);
            Assert.Equal("TestEnum", fields[1].Type);
            Assert.Equal(null, fields[1].DefaultValue);
        }

        [Fact]
        public void ParseTypeWithNestedComplexTypeAndEnum()
        {
            var actual = Message.ParseFrom(messageWithNestedComplexTypesAndEnum);

            Assert.Equal("TypeWithComplexType", actual.Name);
            var fields = actual.Fields.ToArray();

            Assert.Equal("Enum", fields[0].Name);
            Assert.Equal("TestEnum", fields[0].Type);
            Assert.Equal("Bar", fields[0].DefaultValue);

            Assert.Equal("Complex1", fields[1].Name);
            Assert.Equal("UserScore", fields[1].Type);
            Assert.Equal("null", fields[1].DefaultValue);

            var subFields = fields[1].Fields.ToArray();
            Assert.Equal("XYZ", subFields[0].Name);
            Assert.Equal("System.String", subFields[0].Type);
            Assert.Equal("ABC", subFields[1].Name);
            Assert.Equal("System.Int32", subFields[1].Type);

            Assert.Equal("Complex2", subFields[2].Name);
            Assert.Equal("SubUserScore", subFields[2].Type);
            Assert.Equal("null", subFields[2].DefaultValue);
            var subSubFields = subFields[2].Fields.ToArray();
            Assert.Equal("subXYZ", subSubFields[0].Name);
            Assert.Equal("System.String", subSubFields[0].Type);
            Assert.Equal("subABC", subSubFields[1].Name);
            Assert.Equal("System.Int32", subSubFields[1].Type);

            Assert.Equal("Bool", fields[2].Name);
            Assert.Equal("System.Boolean", fields[2].Type);
            Assert.Equal("true", fields[2].DefaultValue);
        }

        [Fact]
        public void ComparesSimpleMessageThatIsCompatible()
        {
            var thisMessage = Message.ParseFrom(simpleMessage);
            var thatMessage = Message.ParseFrom(compatibleSimpleMessage);

            var differences = thisMessage.CompareTo(thatMessage).ToArray();
            Assert.Equal(2, differences.ToArray().Length);

            Assert.Equal("Fields have capitalisation difference: Name vs naMe.", differences[0].Description);
            Assert.Equal("Name", differences[0].Field.Name);
            Assert.Equal(FieldDifference.DifferenceLevelEnum.Warning, differences[0].DifferenceLevel);

            Assert.Equal("Fields have capitalisation difference: Score vs scOre.", differences[1].Description);
            Assert.Equal("Score", differences[1].Field.Name);
            Assert.Equal(FieldDifference.DifferenceLevelEnum.Warning, differences[1].DifferenceLevel);
        }

        [Fact]
        public void FindsAllDifferences()
        {
            var thisMessage = Message.ParseFrom(messageWithNestedComplexTypesAndEnum);
            // See comments above pointing out differences
            var thatMessage = Message.ParseFrom(incompatibleMessageWithNestedComplexTypesAndEnum);

            var differences = thisMessage.CompareTo(thatMessage).ToArray();

            Assert.Equal(6, differences.Length);

            Assert.Equal("This does not contain a ExtraFieldWithNoDefault field, and other has no default value.", differences[0].Description);
            Assert.Equal("ExtraFieldWithNoDefault", differences[0].Field.Name);
            Assert.Equal(FieldDifference.DifferenceLevelEnum.Critical, differences[0].DifferenceLevel);

            Assert.Equal("Fields have capitalisation difference: XYZ vs xyz.", differences[1].Description);
            Assert.Equal("XYZ", differences[1].Field.Name);
            Assert.Equal(FieldDifference.DifferenceLevelEnum.Warning, differences[1].DifferenceLevel);

            Assert.Equal("This does not contain a ExtraSubFieldWithNoDefault field, and other has no default value.", differences[2].Description);
            Assert.Equal("ExtraSubFieldWithNoDefault", differences[2].Field.Name);
            Assert.Equal(FieldDifference.DifferenceLevelEnum.Critical, differences[2].DifferenceLevel);

            Assert.Equal("Field ABC has type System.Int32, other field has type System.Int64.", differences[3].Description);
            Assert.Equal("ABC", differences[3].Field.Name);
            Assert.Equal(FieldDifference.DifferenceLevelEnum.Critical, differences[3].DifferenceLevel);

            Assert.Equal("This does not contain a subXYZWithDefault field, but other has a default value.", differences[4].Description);
            Assert.Equal("subXYZWithDefault", differences[4].Field.Name);
            Assert.Equal(FieldDifference.DifferenceLevelEnum.Warning, differences[4].DifferenceLevel);

            Assert.Equal("This contains a ExtraBool field, but other does not.  The other consumer must use Dasher UnexpectedFieldBehaviour.Ignore mode.",
                differences[5].Description);
            Assert.Equal("ExtraBool", differences[5].Field.Name);
            Assert.Equal(FieldDifference.DifferenceLevelEnum.Warning, differences[5].DifferenceLevel);
        }
    }
}
