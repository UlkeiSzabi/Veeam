using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Moq;
using System.Diagnostics;


namespace VeeamTests
{
    public class Tests
    {
        private Mock<ILogger<MonitorService>> _loggerMock;
        private const string ValidProcessName = "notepad";
        private TimeSpan ValidCheckInterval = TimeSpan.FromMinutes(1); // 1 minute
        private TimeSpan ValidLifeTime = TimeSpan.FromMinutes(5); // 5 minute

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

        [Test]
        public void Constructor_WithValidParameters_DoesNotThrow()
        {
            // TestDelegate for assert
            TestDelegate action = () => new MonitorService(
                ValidProcessName,
                ValidCheckInterval,
                ValidLifeTime,
                CancellationToken.None,
                _loggerMock.Object);

            // Assert
            Assert.DoesNotThrow(action, "Constructor should not throw an exception with valid parameters.");
        }

        [Test]
        public void Constructor_WithInvalidProcessName_ThrowsArgumentException()
        {
            // TestDelegate for assert with invalid process name
            TestDelegate action = () => new MonitorService(
                "",
                ValidCheckInterval,
                ValidLifeTime,
                CancellationToken.None,
                _loggerMock.Object);

            // Assert
            var ex = Assert.Throws<ArgumentException>(action);
            Assert.That(ex.Message, Is.EqualTo("Process name cannot be null, empty, or whitespace. Example: 'notepad'"));
        }

        [Test]
        public void Constructor_WithInvalidCheckInterval_ThrowsArgumentException()
        {
            // TestDelegate for assert with invalid process name
            TestDelegate action = () => new MonitorService(
                ValidProcessName,
                TimeSpan.Zero,
                ValidLifeTime,
                CancellationToken.None,
                _loggerMock.Object);

            // Assert
            var ex = Assert.Throws<ArgumentException>(action);
            Assert.That(ex.Message, Is.EqualTo("Check interval must be a positive integer greater than zero. Example: 1"));
        }

        [Test]
        public void Constructor_WithInvalidLifeTime_ThrowsArgumentException()
        {
            // TestDelegate for assert with invalid process name
            TestDelegate action = () => new MonitorService(
                ValidProcessName,
                ValidCheckInterval,
                TimeSpan.Zero,
                CancellationToken.None,
                _loggerMock.Object);

            // Assert
            var ex = Assert.Throws<ArgumentException>(action);
            Assert.That(ex.Message, Is.EqualTo("Life Time must be a positive integer greater than zero. Example: 5"));
        }

    }
}