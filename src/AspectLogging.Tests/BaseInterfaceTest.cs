using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Logging;

namespace AspectLogging.Tests {
  [TestClass]
  public class BaseInterfaceTest {
    interface IBaseInterface {
      int BaseMethod();
    }

    interface IChildInterface: IBaseInterface {
     void ChildMethod();
    }

    class ZoZo: IChildInterface {
      public void ChildMethod() { }
      public int BaseMethod() => 42;
    }

    ILoggerFactory _loggerFactory = new LoggerFactory();
    List<TestLogEntry> _log = new List<TestLogEntry>();

    [TestInitialize]
    public void TestInitialize() {
      _log = new List<TestLogEntry>();
      (_loggerFactory = new LoggerFactory()).AddTestLogger(_log.Add);
    }

    ILogger GetLogger() => _loggerFactory.CreateLogger<IChildInterface>();

    IChildInterface CreateSelective() =>
      new ZoZo().AddLoggingAspect<IChildInterface>(GetLogger());

    IChildInterface CreateFull() =>
      (new ZoZo() as IChildInterface).AddLoggingAspect(GetLogger(),new LoggingAspectConfiguration(includeInherited: true));

    void AssertLogEvents(params string[] eventNames) {
      Assert.AreEqual(eventNames.Length, _log.Count);
      for(var i = 0; i < eventNames.Length; i++) {
        Assert.AreEqual(eventNames[i], _log[i].EventId.Name, $"Log line {1}");
      }
    }

    [TestMethod]
    public void SelectiveBaseMethodTest() {
      var it = new ZoZo().AddLoggingAspect<IChildInterface>(GetLogger());
      Assert.AreEqual(42, it.BaseMethod());
      Assert.IsFalse(_log.Any());
    }

    [TestMethod]
    public void BofTest() {
      var zozo = new ZoZo().AddLoggingAspect<IChildInterface>(GetLogger());
      zozo.ChildMethod();
      AssertLogEvents("Call", "Completed");
    }

    [TestMethod]
    public void FullHopTest() {
      var it = CreateFull();
      it.BaseMethod();
      AssertLogEvents("Call", "Completed");
    }
  }
}
