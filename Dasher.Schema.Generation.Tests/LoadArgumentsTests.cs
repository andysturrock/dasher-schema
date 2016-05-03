using System.Linq;
using Xunit;

namespace Dasher.Schema.Generation.Tests
{
    public class LoadArgumentsTests
    {
        [Fact]
        public void LoadArgumentTest()
        {
            AppArguments appArgs = new AppArguments();
            appArgs.LoadArguments(new[] { @"--targetDir=C:\TargetDir", @"--targetPath=C:\TargetPath", @"--projectDir=C:\ProjectDir",
                @"--outputFileName=app.manifest", @"--rootElementTag=App", @"--typeElementTag=Type", @"--serialisableTypesTag=SerialisableTypes",
                @"--deserialisableTypesTag=DeserialisableTypes", @"--includeDependencies=BP.*,Something", @"--excludeDependencies=Microsoft.*,BP.Synergy.App" });
            ReturnCode retCode;
            Assert.True(appArgs.ValidateArguments(out retCode));
            Assert.Equal(ReturnCode.EXIT_SUCCESS, retCode);
            Assert.Equal(@"C:\TargetPath", appArgs.TargetPath);
            Assert.Equal(@"C:\TargetDir", appArgs.TargetDir);
            Assert.Equal(@"C:\ProjectDir", appArgs.ProjectDir);
            Assert.Equal(@"BP.*,Something", appArgs.IncludedDependencies);
            Assert.Equal(@"Microsoft.*,BP.Synergy.App", appArgs.ExcludedDependencies);
        }

        [Fact]
        public void MissingArgumentsTest()
        {
            var mandatoryArgs =
                new[]
                {
                    @"--targetDir=C:\TargetDir", @"--targetPath=C:\TargetPath", @"--projectDir=C:\ProjectDir", @"--outputFileName=app.manifest",
                    @"--rootElementTag=App", @"--typeElementTag=Type", @"--serialisableTypesTag=SerialisableTypes", @"--deserialisableTypesTag=DeserialisableTypes"
                };
            foreach (var arg in mandatoryArgs)
            {
                MissingArgs(mandatoryArgs.Where(a => a != arg).ToArray());
            }
            MissingArgs(new[] { @"--targetPath=C:\TargetPath", @"--targetDir=C:\TargetDir" });
            MissingArgs(new[] { @"--targetPath=C:\TargetPath" });
            MissingArgs(new[] { @"--targetPath=" });
            MissingArgs(new string[0]);
        }

        private static void MissingArgs(string[] args)
        {
            AppArguments appArgs = new AppArguments();
            appArgs.LoadArguments(args);
            ReturnCode retCode;
            Assert.False(appArgs.ValidateArguments(out retCode));
            Assert.Equal(ReturnCode.EXIT_ERROR, retCode);
        }

        [Fact]
        public void HelpTest()
        {
            AppArguments appArgs = new AppArguments();
            appArgs.LoadArguments(new[] { @"--h" });
            ReturnCode retCode;
            Assert.False(appArgs.ValidateArguments(out retCode));
            Assert.Equal(ReturnCode.EXIT_SUCCESS, retCode);
        }
    }
}