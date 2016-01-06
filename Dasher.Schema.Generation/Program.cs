using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace Dasher.Schema.Generation
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Environment.ExitCode = (int)new Program().GenerateSchema(args);
            }
            catch (SchemaGenerationException e)
            {
                // Visual Studio parseable format
                Console.WriteLine("Dasher.Schema.Generation.exe : error: " + "Error generating schema for " + e.TargetType + ": " + e.Message);
                Environment.ExitCode = (int)ReturnCode.EXIT_ERROR;
            }
            catch (Exception e)
            {
                // Visual Studio parseable format
                Console.WriteLine("Dasher.Schema.Generation.exe : error: " + e.ToString());
                Environment.ExitCode = (int)ReturnCode.EXIT_ERROR;
            }
        }

        private enum ReturnCode
        {
            EXIT_SUCCESS = 0,
            EXIT_ERROR = 1
        };

        private string targetPath = null;
        private string targetDir = null;
        private string projectDir = null;
        private bool debug = false;
        private bool help = false;

        private ReturnCode GenerateSchema(string[] args)
        {
            var optionSet = new OptionSet() {
                                { "targetPath=", o => targetPath = o },
                                { "targetDir=",  o => targetDir = o },
                                { "projectDir=",  o => projectDir = o },
                                { "debug",   v => debug = v != null },
                                { "h|?|help",   v => help = v != null },
                                };

            List<string> extra = optionSet.Parse(args);


            if (help)
            {
                Usage(Console.Out);
                return ReturnCode.EXIT_SUCCESS;
            }

            if (debug)
                Debugger.Launch();

            if (targetPath == null || targetDir == null || projectDir == null)
            {
                // This format makes it show up properly in the VS Error window.
                Console.WriteLine("Dasher.Schema.Generation.exe : error: Incorrect command line arguments.");
                Usage(Console.Error);
                return ReturnCode.EXIT_ERROR;
            }

            var assembly = Assembly.LoadFrom(targetPath);

            var sendMessageTypes = new HashSet<Type>();
            var receiveMessageTypes = new HashSet<Type>();

            // TODO this is verbose.  Tidy up using Linq?
            foreach (Type t in assembly.GetTypes())
            {
                foreach (var attribute in t.GetCustomAttributes(false))
                {
                    if (attribute is SendMessageAttribute)
                    {
                        sendMessageTypes.Add(t);
                    }
                    if (attribute is ReceiveMessageAttribute)
                    {
                        receiveMessageTypes.Add(t);
                    }
                }
            }

            // Write to the project (ie source) dir and also the target (ie bin) dir.
            // This ensures the manifest is checked into source control but is also
            // can be bundled with the application if needed.
            writeToAppManifest(targetDir, sendMessageTypes, receiveMessageTypes);
            writeToAppManifest(projectDir, sendMessageTypes, receiveMessageTypes);

            return ReturnCode.EXIT_SUCCESS;
        }

        private void writeToAppManifest(string dirName, HashSet<Type> sendMessageTypes, HashSet<Type> receiveMessageTypes)
        {
            XDocument doc;
            // Create a file with a root element if the manifest doesn't exist.
            // This should only happen in testing.
            string appManifestFileName = dirName + "App.manifest";
            XElement appElement;
            if (File.Exists(appManifestFileName))
            {
                doc = XDocument.Load(appManifestFileName);
                appElement = doc.Element("App");
                if (appElement == null)
                {
                    appElement = new XElement("App");
                    doc.AddFirst(appElement);
                }
                if (appElement.Element("SendsMessages") == null)
                {
                    appElement.Add(new XElement("SendsMessages"));
                }
                if (appElement.Element("ReceivesMessages") == null)
                {
                    appElement.Add(new XElement("ReceivesMessages"));
                }
            }
            else
            {
                doc = new XDocument(new XElement("App"));
                appElement = doc.Element("App");
                appElement.Add(new XElement("SendsMessages"));
                appElement.Add(new XElement("ReceivesMessages"));
            }

            var sendsMessagesElement = new XElement("SendsMessages");
            var receivesMessagesElement = new XElement("ReceivesMessages");
            foreach (var sendMessageType in sendMessageTypes)
            {
                var message = XMLSchemaGenerator.GenerateSchema(sendMessageType);
                sendsMessagesElement.AddFirst(message);
            }
            foreach (var receiveMessageType in receiveMessageTypes)
            {
                var message = XMLSchemaGenerator.GenerateSchema(receiveMessageType);
                receivesMessagesElement.AddFirst(message);
            }

            appElement.Element("SendsMessages").ReplaceWith(sendsMessagesElement);
            appElement.Element("ReceivesMessages").ReplaceWith(receivesMessagesElement);

            doc.Save(appManifestFileName);
        }

        private static void Usage(TextWriter writer)
        {
            writer.WriteLine("Usage: Dasher.Schema.Generation.exe --targetDir=TARGETDIR --targetName=TARGETNAME --projectDir=PROJECTDIR [--debug] [--help|-h|-?");
            writer.WriteLine("TARGETDIR is the output directory of the project.  TARGETNAME is the full path of the project target.  PROJECTDIR is the root dir of the project, where the app.messages file will be written.");
        }
    }
}
