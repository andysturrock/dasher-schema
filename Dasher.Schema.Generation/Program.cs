using System;

namespace Dasher.Schema.Generation
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Environment.ExitCode = (int)ReturnCode.EXIT_SUCCESS;
                var appArguments = new AppArguments();
                appArguments.LoadArguments(args);
                ReturnCode retCode;
                if (!appArguments.ValidateArguments(out retCode))
                {
                    Environment.ExitCode = (int)retCode;
                    return;
                }
                new SchemaGenerator().GenerateSchema(appArguments);
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
    }
}

