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
    public class SchemaGenerator
    {
        private const string SENDS_MESSAGES_ELEMENT = "SendsMessages";
        private const string RECEIVE_MESSAGES_ELEMENT = "ReceivesMessages";
        private const string APP_ELEMENT = "App";

        public void GenerateSchema(AppArguments args)
        {
            var assembly = Assembly.LoadFrom(args.TargetPath);
            var assemblyInfo = AssemblyWalker.GetDasherAssemblyInfo(assembly);

            // Write to the project (ie source) dir and also the target (ie bin) dir.
            // This ensures the manifest is checked into source control but is also
            // can be bundled with the application if needed.
            WriteToAppManifest(args.TargetDir, assemblyInfo);
            WriteToAppManifest(args.ProjectDir, assemblyInfo);
        }

        private void WriteToAppManifest(string dirName, DasherAssemblyInfo result)
        {
            XDocument doc;
            // Create a file with a root element if the manifest doesn't exist.
            // This should only happen in testing.
            string appManifestFileName = dirName + "App.manifest";
            XElement appElement;
            if (File.Exists(appManifestFileName))
            {
                doc = XDocument.Load(appManifestFileName);
                appElement = doc.Element(APP_ELEMENT);
                if (appElement == null)
                {
                    appElement = new XElement(APP_ELEMENT);
                    doc.AddFirst(appElement);
                }
                if (appElement.Element(SENDS_MESSAGES_ELEMENT) == null)
                {
                    appElement.Add(new XElement(SENDS_MESSAGES_ELEMENT));
                }
                if (appElement.Element(RECEIVE_MESSAGES_ELEMENT) == null)
                {
                    appElement.Add(new XElement(RECEIVE_MESSAGES_ELEMENT));
                }
            }
            else
            {
                doc = new XDocument(new XElement(APP_ELEMENT));
                appElement = doc.Element(APP_ELEMENT);
                appElement.Add(new XElement(SENDS_MESSAGES_ELEMENT));
                appElement.Add(new XElement(RECEIVE_MESSAGES_ELEMENT));
            }

            var sendsMessagesElement = new XElement(SENDS_MESSAGES_ELEMENT);
            var receivesMessagesElement = new XElement(RECEIVE_MESSAGES_ELEMENT);
            foreach (var sendMessageType in result.SendMessageTypes)
            {
                var message = XMLSchemaGenerator.GenerateSchema(sendMessageType);
                sendsMessagesElement.AddFirst(message);
            }
            foreach (var receiveMessageType in result.ReceiveMessageTypes)
            {
                var message = XMLSchemaGenerator.GenerateSchema(receiveMessageType);
                receivesMessagesElement.AddFirst(message);
            }

            appElement.Element(SENDS_MESSAGES_ELEMENT).ReplaceWith(sendsMessagesElement);
            appElement.Element(RECEIVE_MESSAGES_ELEMENT).ReplaceWith(receivesMessagesElement);

            doc.Save(appManifestFileName);
        }
    }
}