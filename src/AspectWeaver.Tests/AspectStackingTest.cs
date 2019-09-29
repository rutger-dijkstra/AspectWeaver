using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AspectLogging;
using AspectRetry;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Logging;

namespace AspectWeaver.Tests {
  [TestClass]
  public class AspectStackingTest {
    ILoggerFactory _loggerFactory = new LoggerFactory();
    Queue<TestLogEntry> _log = new Queue<TestLogEntry>();
    LoggingAspectConfiguration _logConfig = new LoggingAspectConfiguration(includeInherited: true);

    [TestInitialize]
    public void TestInitialize() {
      _log = new Queue<TestLogEntry>();
      (_loggerFactory = new LoggerFactory()).AddTestLogger(_log.Enqueue);
    }

    ILogger GetLogger() => _loggerFactory.CreateLogger<IZoZo>();

    IZoZo CreateStacked() =>
      (new ZoZo() as IZoZo)
      .AddLoggingAspect(GetLogger(), _logConfig)
      .AddRetryAspect(new RetryStrategy(0, 0))
      .AddLoggingAspect(GetLogger(), _logConfig);

    void AssertLogEvents(params string[] eventNames) {
      var n = 1;
      foreach( var name in eventNames ) {
        Assert.AreEqual(name, _log.Dequeue().EventId.Name, $"Log line {n++}");
      }
    }

    [TestMethod]
    public void HopTest() {
      var zozo = CreateStacked();
      var result = zozo.Hop();
      Assert.AreEqual(666, result);
      Assert.AreEqual(8, _log.Count);
      AssertLogEvents("Call", "Call", "Failure", "Call", "Failure", "Call", "Completed", "Completed");
    }

    [TestMethod]
    public async Task BofTest() {
      var zozo = CreateStacked();
      var result = await zozo.Bof();
      Assert.AreEqual(667, result);
      Assert.AreEqual(8, _log.Count);
      AssertLogEvents("Call", "Call", "Failure", "Call", "Failure", "Call", "Completed", "Completed");
    }

    [TestMethod]
    public async Task LaLaTest() {
      var zozo = CreateStacked();
      await zozo.LaLa();
      Assert.AreEqual(8, _log.Count);
      AssertLogEvents("Call", "Call", "Failure", "Call", "Failure", "Call", "Completed", "Completed");
    }
  }
}
