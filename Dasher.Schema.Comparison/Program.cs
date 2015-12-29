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

        private string manifestPath = null;
        private string otherManifestsDir = null;
        private string manifestFileGlob = null;
        private bool debug = false;
        private bool help = false;

        private ReturnCode CompareSchema(string[] args)
        {
            var optionSet = new OptionSet() {
                                { "manifestPath=", o => manifestPath = o },
                                { "otherManifestsDir=",  o => otherManifestsDir = o },
                                { "manifestFileGlob=",  o => manifestFileGlob = o },
                                { "debug",   v => debug = v != null },
                                { "h|?|help",   v => help = v != null },
                                };

            List<string> extra = optionSet.Parse(args);

            if (help)
            {
                Usage();
                return ReturnCode.EXIT_SUCCESS;
            }

            if (debug)
                Debugger.Launch();

            if (manifestPath == null || otherManifestsDir == null || manifestFileGlob == null)
            {
                // This format makes it show up properly in the VS Error window.
                Console.WriteLine("Dasher.Schema.Comparison.exe : error: Incorrect command line arguments.");
                Usage();
                return ReturnCode.EXIT_ERROR;
            }

            /*
            ** Algorithm is:
            ** Read file
            ** For each message in this manifest
            **     Next message if other manifest does not have message with same name
            **     Foreach message in this manifest, find a message in the other manifest with the same name
            *      Compare the messages.
            */
            var thisManifest = XDocument.Load(manifestPath);
            var theseReceivesMessageElements = thisManifest.XPathSelectElements("//ReceivesMessages/Message");
            var theseReceivesMessages = new List<Message>();
            foreach (var elem in theseReceivesMessageElements)
            {
                theseReceivesMessages.Add(Message.ParseFrom(elem));
            }
            var theseSendsMessageElements = thisManifest.XPathSelectElements("//SendsMessages/Message");
            var theseSendsMessages = new List<Message>();
            foreach (var elem in theseSendsMessageElements)
            {
                theseSendsMessages.Add(Message.ParseFrom(elem));
            }

            var otherManifestPaths = Directory.GetFiles(otherManifestsDir, manifestFileGlob, SearchOption.AllDirectories);
            var foundError = false;
            foreach (var otherManifestPath in otherManifestPaths)
            {
                var differences = new List<FieldDifference>();
                var otherManifest = XDocument.Load(otherManifestPath);

                // First look where we are the receiver and the other apps are the senders
                var thoseSendsMessageElems = otherManifest.XPathSelectElements("//SendsMessages/Message");
                var thoseSendsMessages = new List<Message>();
                foreach (var elem in thoseSendsMessageElems)
                {
                    thoseSendsMessages.Add(Message.ParseFrom(elem));
                }
                foreach (var thisReceivesMessage in theseReceivesMessages)
                {
                    var otherSendMessages = (from m in thoseSendsMessages
                                         where m.Name.ToLower() == thisReceivesMessage.Name.ToLower()
                                         select m).ToList();
                    if (otherSendMessages.Count == 0) // No message with same name in other manifest
                        continue;
                    if (otherSendMessages.Count > 1)
                    {
                        Console.WriteLine($"{otherManifestPath}({thisReceivesMessage.Name}) : warning: {otherManifestPath} contains more than one message named {thisReceivesMessage.Name} in the SendsMessages section.  Only the first definition will be compared.");
                    }
                    var otherSendsMessage = otherSendMessages.First();
                    differences.AddRange(otherSendsMessage.CompareTo(thisReceivesMessage).ToList());
                }
                // Now where we are the sender and the other app are the receivers
                var thoseReceivesMessageElems = otherManifest.XPathSelectElements("//ReceivesMessages/Message");
                var thoseReceivesMessages = new List<Message>();
                foreach (var elem in thoseReceivesMessageElems)
                {
                    thoseReceivesMessages.Add(Message.ParseFrom(elem));
                }
                foreach (var thisSendsMessage in theseSendsMessages)
                {
                    var otherReceivesMessages = (from m in thoseReceivesMessages
                                             where m.Name.ToLower() == thisSendsMessage.Name.ToLower()
                                             select m).ToList();
                    if (otherReceivesMessages.Count == 0) // No message with same name in other manifest
                        continue;
                    if (otherReceivesMessages.Count > 1)
                    {
                        Console.WriteLine($"{otherManifestPath}({thisSendsMessage.Name}) : warning: {otherManifestPath} contains more than one message named {thisSendsMessage.Name} in the ReceivesMessages section.  Only the first definition will be compared.");
                    }
                    var otherReceivesMessage = otherReceivesMessages.First();
                    differences.AddRange(thisSendsMessage.CompareTo(otherReceivesMessage).ToList());
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
                            throw new MessageComparisonException("Unknown difference level: " + difference.DifferenceLevel.ToString());
                    }
                }
            }

            if (foundError)
                return ReturnCode.EXIT_ERROR;
            else
                return ReturnCode.EXIT_SUCCESS;
        }



        private static void Usage()
        {
            Console.WriteLine("Usage: Dasher.Schema.Comparison.exe --manifestPath=MANIFESTPATH --otherManifestsDir=OTHERMANIFESTSPATH --manifestFileGlob=MANIFESTFILEGLOB [--debug] [--help|-h|-?");
            Console.WriteLine("TARGETDIR is the output directory of the project.");
            Console.WriteLine("OTHERMANIFESTSPATH is the top level directory, under which to search for other manifest files to compare messages.");
            Console.WriteLine("MANIFESTFILEGLOB is the filename pattern to use to match manifest files.  Eg *.* will match all files, App.manifest will only consider files called App.manifest.");
        }
    }
}
