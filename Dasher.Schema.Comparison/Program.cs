using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Dasher.Schema.Comparison
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Environment.ExitCode = (int)new Program().CompareSchema(args);
            }
            catch (Exception e)
            {
                // Visual Studio parseable format
                Console.WriteLine("Dasher.Schema.Comparison.exe : error: " + e.ToString());
                Environment.ExitCode = (int)ReturnCode.EXIT_ERROR;
            }
        }

        private enum ReturnCode
        {
            EXIT_SUCCESS = 0,
            EXIT_ERROR = 1
        };

        private string _manifestPath;
        private string _otherManifestsDir;
        private string _manifestFileGlob;
        private string _serialisableTypeElementTag;
        private string _serialisableTypesSectionTag;
        private string _deserialisableTypesSectionTag;
        private bool _debug;
        private bool _help;

        private ReturnCode CompareSchema(string[] args)
        {
            var optionSet = new OptionSet {
                                { "manifestPath=", o => _manifestPath = o },
                                { "otherManifestsDir=",  o => _otherManifestsDir = o },
                                { "manifestFileGlob=",  o => _manifestFileGlob = o },
                                { "typeElementTag=",  o => _serialisableTypeElementTag = o },
                                { "serialisableTypesTag=",  o => _serialisableTypesSectionTag = o },
                                { "deserialisableTypesTag=",  o => _deserialisableTypesSectionTag = o },
                                { "debug",   v => _debug = v != null },
                                { "h|?|help",   v => _help = v != null },
                                };
            optionSet.Parse(args);

            if (_help)
            {
                Usage();
                return ReturnCode.EXIT_SUCCESS;
            }

            if (_debug)
                Debugger.Launch();

            if (_manifestPath == null || _otherManifestsDir == null || _manifestFileGlob == null ||
                _serialisableTypeElementTag == null || _serialisableTypesSectionTag == null || _deserialisableTypesSectionTag == null)
            {
                // This format makes it show up properly in the VS Error window.
                Console.WriteLine("Dasher.Schema.Comparison.exe : error: Incorrect command line arguments.");
                Usage();
                return ReturnCode.EXIT_ERROR;
            }

            /*
            ** Algorithm is:
            ** Read file
            ** For each type element in this manifest
            **     Next type if other manifest does not have type with same name
            **     Foreach type in this manifest, find a type in the other manifest with the same name
            *      Compare the types.
            */
            var parser = new Parser(_serialisableTypeElementTag);
            var thisManifest = XDocument.Load(_manifestPath);
            var theseDeserialisesTypeElements = thisManifest.XPathSelectElements($"//{_deserialisableTypesSectionTag}/{_serialisableTypeElementTag}");
            var theseDeserialisesTypes = theseDeserialisesTypeElements.Select(elem => parser.ParseFrom(elem)).ToList();
            var theseSerialisesTypeElements = thisManifest.XPathSelectElements($"//{_serialisableTypesSectionTag}/{_serialisableTypeElementTag}");
            var theseSerialisesTypes = theseSerialisesTypeElements.Select(elem => parser.ParseFrom(elem)).ToList();

            var otherManifestPaths = Directory.GetFiles(_otherManifestsDir, _manifestFileGlob, SearchOption.AllDirectories);
            foreach (var otherManifestPath in otherManifestPaths)
            {
                var differences = new List<FieldDifference>();
                var otherManifest = XDocument.Load(otherManifestPath);

                // First look where we are the deserialiser and the other apps are the serialisers
                var thoseSerialisesTypesElems = otherManifest.XPathSelectElements($"//{_serialisableTypesSectionTag}/{_serialisableTypeElementTag}");
                var thoseSerialisesTypes = thoseSerialisesTypesElems.Select(elem => parser.ParseFrom(elem)).ToList();
                foreach (var thisDeserialisesType in theseDeserialisesTypes)
                {
                    var otherSerialisesTypes =
                        (from m in thoseSerialisesTypes
                            where m.Name.ToLower() == thisDeserialisesType.Name.ToLower()
                            select m).ToList();
                    if (otherSerialisesTypes.Count == 0) // No type with same name in other manifest
                        continue;
                    if (otherSerialisesTypes.Count > 1)
                    {
                        Console.WriteLine(
                            $"{otherManifestPath}({thisDeserialisesType.Name}) : warning: {otherManifestPath} contains more than one type named " + 
                            $"{thisDeserialisesType.Name} in the {_serialisableTypesSectionTag} section.  Only the first definition will be compared.");
                    }
                    var otherSerialisesType = otherSerialisesTypes.First();
                    differences.AddRange(otherSerialisesType.CompareTo(thisDeserialisesType).ToList());
                }
                // Now where we are the serialiser and the other app is the deserialiser
                var thoseDeserialisesTypeElements = otherManifest.XPathSelectElements($"//{_deserialisableTypesSectionTag}/{_serialisableTypeElementTag}");
                var thoseDeserialisesTypes = thoseDeserialisesTypeElements.Select(elem => parser.ParseFrom(elem)).ToList();
                foreach (var thisSerialisesType in theseSerialisesTypes)
                {
                    var otherDeserialisesTypes = (from m in thoseDeserialisesTypes
                                             where m.Name.ToLower() == thisSerialisesType.Name.ToLower()
                                             select m).ToList();
                    if (otherDeserialisesTypes.Count == 0) // No type with same name in other manifest
                        continue;
                    if (otherDeserialisesTypes.Count > 1)
                    {
                        Console.WriteLine(
                            $"{otherManifestPath}({thisSerialisesType.Name}) : warning: {otherManifestPath} contains more than one type named " +
                            $"{thisSerialisesType.Name} in the {_deserialisableTypesSectionTag} section.  Only the first definition will be compared.");
                    }
                    var otherDeserialisesType = otherDeserialisesTypes.First();
                    differences.AddRange(thisSerialisesType.CompareTo(otherDeserialisesType).ToList());
                }

                foreach (var difference in differences)
                {
                    switch (difference.DifferenceLevel)
                    {
                        case FieldDifference.DifferenceLevelEnum.Critical:
                            Console.WriteLine($"{otherManifestPath}({difference.Field.Name}) : error: {difference.Description}");
                            break;
                        case FieldDifference.DifferenceLevelEnum.Warning:
                            Console.WriteLine($"{otherManifestPath}({difference.Field.Name}) : warning: {difference.Description}");
                            break;
                        default:
                            throw new ComparisonException("Unknown difference level: " + difference.DifferenceLevel.ToString());
                    }
                }
            }

            return ReturnCode.EXIT_SUCCESS;
        }


        private static void Usage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("    Dasher.Schema.Comparison.exe --manifestPath=MANIFESTPATH");
            Console.WriteLine("        --otherManifestsDir=OTHERMANIFESTSPATH --manifestFileGlob=MANIFESTFILEGLOB");
            Console.WriteLine("        --typeElementTag=TYPETAG");
            Console.WriteLine("        --serialisableTypesTag=SERIALISABLESTAG --deserialisableTypesTag=DESERIALISABLESTAG");
            Console.WriteLine("        [--debug] [--help|-h|-?");
            Console.WriteLine("MANIFESTPATH the path to the manifest to source the types from.");
            Console.WriteLine("OTHERMANIFESTSPATH is the top level directory, under which to search for other manifest files to compare types.");
            Console.WriteLine(
                "MANIFESTFILEGLOB is the filename pattern to use to match manifest files.  Eg *.* will match all files, App.manifest will only consider " +
                "files called App.manifest.");
            Console.WriteLine("TYPETAG is the XML tag for type entries (e.g., \"Message\").");
            Console.WriteLine("SERIALISABLESTAG is the XML tag for the manifest sections listing serialisable types (e.g., \"SendsMessages\").");
            Console.WriteLine("DESERIALISABLESTAG is the XML tag for the manifest sections listing deserialisable types (e.g., \"ReceivesMessages\").");
        }
    }
}
