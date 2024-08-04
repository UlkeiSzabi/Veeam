using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Moq;


namespace VeeamTests
{
    public class Tests
    {
        private Mock<ILogger<MonitorService>> _loggerMock;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<MonitorService>>();
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }
    }
}