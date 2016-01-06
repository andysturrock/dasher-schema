using System;
using System.Collections.Generic;
using System.Diagnostics;
using NDesk.Options;

namespace Dasher.Schema.Generation
{
    public class AppArguments
    {
        public string TargetPath { get; private set; }
        public string TargetDir { get; private set; }
        public string ProjectDir { get; private set; }
        public bool Debug { get; private set; }
        public bool Help { get; private set; }

        public void LoadArguments(string[] args)
        {
            var optionSet = new OptionSet()
            {
                {"targetPath=", o => TargetPath = o},
                {"targetDir=", o => TargetDir = o},
                {"projectDir=", o => ProjectDir = o},
                {"debug", v => Debug = v != null},
                {"h|?|help", v => Help = v != null},
            };

            List<string> extra = optionSet.Parse(args);
        }

        public bool ValidateArguments(out ReturnCode returnCode)
        {
            returnCode = ReturnCode.EXIT_SUCCESS;
            if (Help)
            {
                Usage();
                returnCode = ReturnCode.EXIT_SUCCESS;
                return false;
            }

            if (Debug)
                Debugger.Launch();

            if (TargetPath == null || TargetDir == null || ProjectDir == null)
            {
                // This format makes it show up properly in the VS Error window.
                Console.WriteLine("Dasher.Schema.Generation.exe : error: Incorrect command line arguments.");
                Usage();
                returnCode = ReturnCode.EXIT_ERROR;
                return false;
            }
            return true;

        }

        private static void Usage()
        {
            Console.WriteLine(
                "Usage: Dasher.Schema.Generation.exe --targetDir=TARGETDIR --targetName=TARGETNAME --projectDir=PROJECTDIR [--debug] [--help|-h|-?");
            Console.WriteLine(
                "TARGETDIR is the output directory of the project.  TARGETNAME is the full path of the project target.  PROJECTDIR is the root dir of the project, where the app.messages file will be written.");
        }
    }
}