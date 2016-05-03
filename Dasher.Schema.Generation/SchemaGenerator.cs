using System;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace Dasher.Schema.Generation
{
    public enum ReturnCode
    {
        EXIT_SUCCESS = 0,
        EXIT_ERROR = 1
    };
    public static class SchemaGenerator
    {
        public static void GenerateSchema(AppArguments args)
        {
            var assembly = Assembly.LoadFrom(args.TargetPath);
            var walker = new AssemblyWalker(args.IncludedDependencies, args.ExcludedDependencies);
            var assemblyInfo = walker.GetDasherAssemblyInfo(assembly);

            // Write to the project (ie source) dir and also the target (ie bin) dir. This ensures the manifest is checked into source control but is also
            // can be bundled with the application if needed.
            WriteToManifest(assemblyInfo,
                args.TargetDir, args.OutputFileName, args.RootElementTag, args.SerialisableTypeElementTag,
                args.SerialisableTypesSectionTag, args.DeserialisableTypesSectionTag);
            WriteToManifest(assemblyInfo,
                args.ProjectDir, args.OutputFileName, args.RootElementTag, args.SerialisableTypeElementTag,
                args.SerialisableTypesSectionTag, args.DeserialisableTypesSectionTag);
        }

        private static void WriteToManifest(
            DasherAssemblyInfo dasherAssemblyInfo, string outputDirectoryName, string outputFileName, string rootElementTag, string serialisableTypeElementTag,
            string serialisesTypesElementTag, string deserialisesTypesElementTag)
        {
            XDocument doc;
            // Create a file with a root element if the manifest doesn't exist. This should only happen in testing.
            string manifestFileName = outputDirectoryName + outputFileName;
            XElement rootElement;
            if (File.Exists(manifestFileName))
            {
                doc = XDocument.Load(manifestFileName);
                rootElement = doc.Element(rootElementTag);
                if (rootElement == null)
                {
                    rootElement = new XElement(rootElementTag);
                    doc.AddFirst(rootElement);
                }
                if (rootElement.Element(serialisesTypesElementTag) == null)
                {
                    rootElement.Add(new XElement(serialisesTypesElementTag));
                }
                if (rootElement.Element(deserialisesTypesElementTag) == null)
                {
                    rootElement.Add(new XElement(deserialisesTypesElementTag));
                }
            }
            else
            {
                doc = new XDocument(new XElement(rootElementTag));
                rootElement = doc.Element(rootElementTag);
                rootElement.Add(new XElement(serialisesTypesElementTag));
                rootElement.Add(new XElement(deserialisesTypesElementTag));
            }

            var serialisesTypesElement = new XElement(serialisesTypesElementTag);
            var deserialisesTypesElement = new XElement(deserialisesTypesElementTag);
            foreach (var serialisableType in dasherAssemblyInfo.SerialisableTypes)
            {
                var element = XMLSchemaGenerator.GenerateSchema(serialisableType, serialisableTypeElementTag);
                serialisesTypesElement.AddFirst(element);
            }
            foreach (var deserialisableType in dasherAssemblyInfo.DeserialisableTypes)
            {
                var element = XMLSchemaGenerator.GenerateSchema(deserialisableType, serialisableTypeElementTag);
                deserialisesTypesElement.AddFirst(element);
            }

            rootElement.Element(serialisesTypesElementTag).ReplaceWith(serialisesTypesElement);
            rootElement.Element(deserialisesTypesElementTag).ReplaceWith(deserialisesTypesElement);

            doc.Save(manifestFileName);
        }
    }
}