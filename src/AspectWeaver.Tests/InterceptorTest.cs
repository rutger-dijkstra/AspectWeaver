using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspectWeaver.Tests {
  [TestClass]
  public class InterceptorTest {

    interface ITheInterface {
      void DoSomething(int fst, string scnd = "theDefault");
      Task DoSomethingAsync();
      Task<bool> FailGetSomethingAsync(int arg);
      void FailSomething(object arg);
      Task FailSomethingAsync();
      bool GetSomething(int arg);
      Task<bool> GetSomethingAsync(int arg);
    }

    class TheImplementation: ITheInterface {
      public void DoSomething(int fst, string scnd = "theDefault") { }

      public void FailSomething(object arg) {
        throw new FormatException("o o o");
      }

      public bool GetSomething(int arg) => arg == 42;

      public Task DoSomethingAsync() =>
          Task.Delay(2);

      public Task FailSomethingAsync() =>
         Task.Delay(5).ContinueWith(t => { throw new InvalidOperationException("i i i"); });

      public Task<bool> GetSomethingAsync(int arg) =>
        Task.Delay(5).ContinueWith(t => arg == 42);

      public Task<bool> FailGetSomethingAsync(int arg) =>
        Task.Delay(5).ContinueWith(t => (bool?)null ?? throw new InvalidOperationException("a a a"));
    }

     List<string> _log = new List<string>();

    ITheInterface _decorated;

    private ITheInterface CreateDecoratedImplementation() =>
        (new TheImplementation() as ITheInterface)
        .AddAspect(_ => new CallRecorder(_, _log));

    [TestInitialize]
    public void TestInitialize() {
      _log.Clear();
      _decorated = CreateDecoratedImplementation();
    }

    private void AssertStartsWith(string expected, string actual) {
      if( expected.Length < (actual?.Length ?? 0) ) {
        actual = actual.Substring(0, expected.Length);
      }
      Assert.AreEqual(expected, actual);
    }

    private void AssertLog(params string[] lines) =>
      CollectionAssert.AreEqual(lines, _log);

    [TestMethod]
    public void DoSomethingTest() {
      _decorated.DoSomething(5);
      AssertLog("DoSomething called", "DoSomething completed");
    }

    [TestMethod]
    public void GetSomethingTest() {
      var result = _decorated.GetSomething(8);
      AssertLog("GetSomething called", $"GetSomething returned {result}");
    }

    [TestMethod]
    public void FailSomethingTest() {
      try {
        _decorated.FailSomething(new { aap = "noot" });
        Assert.Fail("Expected exception");
      } catch( Exception e) {
         AssertLog("FailSomething called", $"FailSomething threw {e.GetType()}");
      }
    }

    [TestMethod]
    public async Task DoSomethingAsyncTest() {
      await _decorated.DoSomethingAsync();
      AssertLog("DoSomethingAsync called", "DoSomethingAsync completed");
    }

    [TestMethod]
    public async Task GetSomethingAsyncTest() {
      var result = await _decorated.GetSomethingAsync(183);
      AssertLog("GetSomethingAsync called", $"GetSomethingAsync returned {result}");
    }

    [TestMethod]
    public async Task FailSomethingAsyncTest() {
      try {
        await _decorated.FailSomethingAsync();
        Assert.Fail("Expected exception");
      } catch( Exception e ) {
        AssertLog("FailSomethingAsync called", $"FailSomethingAsync threw {e.GetType()}");
      }
    }
  }
}
