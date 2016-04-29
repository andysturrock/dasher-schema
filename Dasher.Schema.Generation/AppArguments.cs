using System;
using System.Diagnostics;
using NDesk.Options;

namespace Dasher.Schema.Generation
{
    public class AppArguments
    {
        public string TargetPath { get; private set; }
        public string TargetDir { get; private set; }
        public string ProjectDir { get; private set; }
        public string OutputFileName { get; private set; }
        public string RootElementTag { get; private set; }
        public string SerialisableTypeElementTag { get; private set; }
        public string SerialisableTypesSectionTag { get; private set; }
        public string DeserialisableTypesSectionTag { get; private set; }
        public bool Debug { get; private set; }
        public bool Help { get; private set; }
        public string IncludedDependencies { get; private set; }
        public string ExcludedDependencies { get; private set; }

        public void LoadArguments(string[] args)
        {
            var optionSet = new OptionSet
            {
                {"targetPath=", o => TargetPath = o},
                {"targetDir=", o => TargetDir = o},
                {"projectDir=", o => ProjectDir = o},
                {"outputFileName=", o => OutputFileName = o},
                {"rootElementTag=",  o => RootElementTag = o },
                {"typeElementTag=",  o => SerialisableTypeElementTag = o },
                {"serialisableTypesTag=",  o => SerialisableTypesSectionTag = o },
                {"deserialisableTypesTag=",  o => DeserialisableTypesSectionTag = o },
                {"debug", v => Debug = v != null},
                {"h|?|help", v => Help = v != null},
                {"includeDependencies=", o=> IncludedDependencies = o },
                {"excludeDependencies=", o=> ExcludedDependencies = o }
            };
            optionSet.Parse(args);
        }

        public bool ValidateArguments(out ReturnCode returnCode)
        {
            if (Help)
            {
                Usage();
                returnCode = ReturnCode.EXIT_SUCCESS;
                return false;
            }

            if (Debug)
                Debugger.Launch();

            if (TargetPath == null || TargetDir == null || ProjectDir == null || OutputFileName == null | RootElementTag == null |
                SerialisableTypeElementTag == null || SerialisableTypesSectionTag == null || DeserialisableTypesSectionTag == null)
            {
                // This format makes it show up properly in the VS Error window.
                Console.WriteLine("Dasher.Schema.Generation.exe : error: Incorrect command line arguments.");
                Usage();
                returnCode = ReturnCode.EXIT_ERROR;
                return false;
            }

            returnCode = ReturnCode.EXIT_SUCCESS;
            return true;
        }

        private static void Usage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("    Dasher.Schema.Generation.exe --targetDir=TARGETDIR --targetPath=TARGETPATH --projectDir=PROJECTDIR");
            Console.WriteLine("        --outputFileName=OUTPUTFILENAME --rootElementTag=DOCTAG --typeElementTag=TYPETAG");
            Console.WriteLine("        --serialisableTypesTag=SERIALISABLESTAG --deserialisableTypesTag=DESERIALISABLESTAG");
            Console.WriteLine("        [--debug] [--help|-h|-?]");
            Console.WriteLine("        [--includeDependencies=INCLUDED_ASSEMBLY_LIST] [--excludeDependencies=EXCLUDED_ASSEMBLY_LIST]");
            Console.WriteLine("TARGETDIR is the output directory of the project.");
            Console.WriteLine("TARGETPATH is the full path of the project target.");
            Console.WriteLine("PROJECTDIR is the root dir of the project, where the output file will be written.");
            Console.WriteLine("OUTPUTFILENAME is name of the file to write to, in \"filename.ext\" form.");
            Console.WriteLine("DOCTAG is the XML tag for the root element in the output file, beneath which the generated manifest sections will be placed.");
            Console.WriteLine("TYPETAG is the XML tag for type entries (e.g., \"Message\").");
            Console.WriteLine("SERIALISABLESTAG is the XML tag for the manifest sections listing serialisable types (e.g., \"SendsMessages\").");
            Console.WriteLine("DESERIALISABLESTAG is the XML tag for the manifest sections listing deserialisable types (e.g., \"ReceivesMessages\").");
            Console.WriteLine(
                "Optional INCLUDED_ASSEMBLY_LIST is the comma delimited list of dependant assembly names to search for schemas. " +
                "The '*' character can be used at the end to search prefixes, e.g. --includeDependencies=MyAssembly,MyPrefix.*");
            Console.WriteLine(
                "Optional EXCLUDED_ASSEMBLY_LIST is the comma delimited list of dependant assembly names to skip. " +
                "The '*' character can be used at the end to search prefixes.");
            Console.WriteLine("By default System.*, Microsoft.*, Dasher and Dasher.Schema are excluded.");
        }
    }
}