using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AspectLogging;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Logging;

namespace AspectWeaver.Tests {
  [TestClass]
  public class SelectiveInterceptorTest {
    ILoggerFactory _loggerFactory = new LoggerFactory();
    Queue<TestLogEntry> _log = new Queue<TestLogEntry>();

    [TestInitialize]
    public void TestInitialize() {
      _log = new Queue<TestLogEntry>();
      (_loggerFactory = new LoggerFactory()).AddTestLogger(_log.Enqueue);
    }

    ILogger GetLogger() => _loggerFactory.CreateLogger<IZoZo>();

    IZoZo CreateSelective() =>
      (new ZoZo() { failures = 0 } as IZoZo)
      .AddLoggingAspect(GetLogger());

    void AssertLogEvents(params string[] eventNames) {
      var n = 1;
      foreach( var name in eventNames ) {
        Assert.AreEqual(name, _log.Dequeue().EventId.Name, $"Log line {n++}");
      }
    }

    [TestMethod]
    public void HopTest() {
      var zozo = CreateSelective();
      var result = zozo.Hop();
      Assert.AreEqual(666, result);
      Assert.AreEqual(0, _log.Count);
    }

    [TestMethod]
    public async Task BofTest() {
      var zozo = CreateSelective();
      var result = await zozo.Bof();
      Assert.AreEqual(667, result);
      Assert.AreEqual(2, _log.Count);
      AssertLogEvents("Call", "Completed");
    }

    [TestMethod]
    public async Task LaLaTest() {
      var zozo = CreateSelective();
      await zozo.LaLa();
      Assert.AreEqual(2, _log.Count);
      AssertLogEvents("Call", "Completed");
    }
  }
}
