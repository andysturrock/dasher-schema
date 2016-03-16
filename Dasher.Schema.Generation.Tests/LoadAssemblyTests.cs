using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dasher.Schema.Generation.TestRefAssembly;
using Xunit;

namespace Dasher.Schema.Generation.Tests
{
    public class AssemblyWalkerProxy
    {
        private readonly AssemblyWalker _assemblyWalker;
        private readonly System.Type _type;

        public AssemblyWalkerProxy(AssemblyWalker assemblyWalker)
        {
            _assemblyWalker = assemblyWalker;
            _type = _assemblyWalker.GetType();
        }

        public List<string> ExcludedPrefixes => (List<string>)_type.GetField("ExcludedPrefixes", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(_assemblyWalker);
        public List<string> ExcludedAssemblies => (List<string>)_type.GetField("ExcludedAssemblies", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(_assemblyWalker);
        public List<string> IncludedPrefixes => (List<string>)_type.GetField("IncludedPrefixes", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(_assemblyWalker);
        public List<string> IncludedAssemblies => (List<string>)_type.GetField("IncludedAssemblies", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(_assemblyWalker);

        public List<AssemblyName> GetFilteredReferencedAssemblyNames(AssemblyName[] referencedAssemblies)
        {
            return (List<AssemblyName>) _type.GetMethod("GetFilteredReferencedAssemblyNames", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(_assemblyWalker, new object[] { referencedAssemblies });
        }
    }

    public class LoadAssemblyTests
    {
        private readonly DasherAssemblyInfo _dasherAssemblyInfo;
        private AssemblyWalkerProxy _assemblyWalkerProxy;

        public LoadAssemblyTests()
        {
            var assembly = typeof(Dummy).Assembly;
            var assemblyWalker = new AssemblyWalker("Dasher.Schema.Generation.*,Something", "Microsoft.*,Another");
            _assemblyWalkerProxy = new AssemblyWalkerProxy(assemblyWalker);
            _dasherAssemblyInfo = assemblyWalker.GetDasherAssemblyInfo(assembly);
        }

        [Fact]
        public void LoadMessages()
        {
            Assert.Equal(3, _dasherAssemblyInfo.ReceiveMessageTypes.Count);
            Assert.Equal(4, _dasherAssemblyInfo.SendMessageTypes.Count);
        }


        [Fact]
        public void SendMessagesTest()
        {
            Assert.True(_dasherAssemblyInfo.SendMessageTypes.Any(o => o.Name == "Sender"));
            Assert.True(_dasherAssemblyInfo.SendMessageTypes.Any(o => o.Name == "BaseSenderReceiver"));
            Assert.True(_dasherAssemblyInfo.SendMessageTypes.Any(o => o.Name == "SenderAndReceiver"));
            Assert.True(_dasherAssemblyInfo.SendMessageTypes.Any(o => o.Name == "DerivedSender"));
            Assert.False(_dasherAssemblyInfo.SendMessageTypes.Any(o => o.Name == "Receiver"));
        }

        [Fact]
        public void ReceiveMessagesTest()
        {
            Assert.True(_dasherAssemblyInfo.ReceiveMessageTypes.Any(o => o.Name == "Receiver"));
            Assert.True(_dasherAssemblyInfo.ReceiveMessageTypes.Any(o => o.Name == "BaseSenderReceiver"));
            Assert.True(_dasherAssemblyInfo.ReceiveMessageTypes.Any(o => o.Name == "SenderAndReceiver"));
            Assert.False(_dasherAssemblyInfo.ReceiveMessageTypes.Any(o => o.Name == "DerivedSender"));
            Assert.False(_dasherAssemblyInfo.ReceiveMessageTypes.Any(o => o.Name == "Sender"));
        }
        [Fact]
        public void BaseClassIgnoreTest()
        {
            Assert.False(_dasherAssemblyInfo.ReceiveMessageTypes.Any(o => o.Name == "DerivedSender"));
            Assert.True(_dasherAssemblyInfo.SendMessageTypes.Any(o => o.Name == "DerivedSender"));
        }

        [Fact]
        public void IncludedListsTest()
        {
            var ep = _assemblyWalkerProxy.ExcludedPrefixes;
            var ea = _assemblyWalkerProxy.ExcludedAssemblies;
            var ip = _assemblyWalkerProxy.IncludedPrefixes;
            var ia = _assemblyWalkerProxy.IncludedAssemblies;
            Assert.NotNull(ep);
            Assert.NotNull(ea);
            Assert.NotNull(ip);
            Assert.NotNull(ia);
            Assert.Equal(3, ep.Count);
            Assert.Equal(3, ea.Count);
            Assert.Equal(1, ip.Count);
            Assert.Equal(1, ia.Count);
        }

        [Fact]
        public void FilterReferencedAssembliesTest()
        {
            var aw = new AssemblyWalker("IncludedAssembly,Included.*", "ExcludedAssembly,Excluded.*");
            var proxy = new AssemblyWalkerProxy(aw);
            AssemblyName[] assemblies = { new AssemblyName("Dasher"), new AssemblyName("System"), new AssemblyName("System.Core"), new AssemblyName("Microsoft"),
                new AssemblyName("Microsoft.Bill"), new AssemblyName("Included.Assembly"), new AssemblyName("IncludedAssembly"), new AssemblyName("ExcludedAssembly"), new AssemblyName("Excluded.Assembly"),  };
            var result = proxy.GetFilteredReferencedAssemblyNames(assemblies);
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Equal("Included.Assembly", result[0].Name);
            Assert.Equal("IncludedAssembly", result[1].Name);
        }

    }
}