using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Moq;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestPlatform.TestHost;


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


        [Test]
        public async Task ExecuteAsync_TerminateProcess_WhenLifetimeExceedsThreshold()
        {
            // Define MonitorService with low time parameters for testing purposes
            var cts = new CancellationTokenSource();
            var service = new MonitorService(
                ValidProcessName,
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(5),
                CancellationToken.None,
                _loggerMock.Object);

            // Create and start the process
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "notepad.exe",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            await Task.Delay(1000); // Give some time for process to start

            // Use reflection to gain access to the protected override method
            var executeAsyncMethod = typeof(MonitorService)
            .GetMethod("ExecuteAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Execute the ExecuteAsync function
            var executeTask = (Task)executeAsyncMethod.Invoke(service, new object[] { cts.Token });

            // Wait more than the provided lifetime
            await Task.Delay(TimeSpan.FromSeconds(7)); // Wait for enough time for process to be killed

            // Cancel the task
            cts.Cancel();

            try
            {
                process.Kill(); // Ensure the process is terminated if still running
            }
            catch (InvalidOperationException) { } // Process already killed

            var processes = Process.GetProcessesByName(ValidProcessName);
            Assert.That(processes.Length, Is.EqualTo(0), "The process should be killed by now");

            // Verify that logging includes process termination
            _loggerMock.Verify(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("has been killed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            // Verify that logging includes info
            _loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("has been running for")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(5));
        }


        [Test]
        public async Task ExecuteAsync_LogWarning_WhenProcessIsNotRunning()
        {
            // Define MonitorService with low time parameters for testing purposes
            var cts = new CancellationTokenSource();
            var service = new MonitorService(
                ValidProcessName,
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(5),
                CancellationToken.None,
                _loggerMock.Object);

            // Use reflection to gain access to the protected override method
            var executeAsyncMethod = typeof(MonitorService)
            .GetMethod("ExecuteAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Execute the ExecuteAsync function
            var executeTask = (Task)executeAsyncMethod.Invoke(service, new object[] { cts.Token });

            // Wait more than the provided lifetime
            await Task.Delay(TimeSpan.FromSeconds(7)); // Wait for enough time for process to be killed

            // Cancel the task
            cts.Cancel();

            // Verify that logging includes info
            _loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("No running processes found with the name")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}