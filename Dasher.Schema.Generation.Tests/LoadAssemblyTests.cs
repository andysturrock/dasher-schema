using System.Linq;
using Xunit;

namespace Dasher.Schema.Generation.Tests
{
    
    public class LoadAssemblyTests
    {
        private readonly DasherAssemblyInfo _dasherAssemblyInfo;

        public LoadAssemblyTests()
        {
            var assembly = typeof(TestClasses.Sender).Assembly;
            _dasherAssemblyInfo = AssemblyWalker.GetDasherAssemblyInfo(assembly);
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
    }
}