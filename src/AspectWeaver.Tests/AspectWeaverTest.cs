using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace AspectWeaver.Tests {
  [TestClass]
  public class AspectWeaverTest {
    interface It {
      int Jo();
      T Echo<T>(T arg);
    }
    class Imp: It {
      public int Jo() => 42;
      public T Echo<T>(T arg) => arg;
    }

    class BeforeCallDone: InvocationInterceptor {
      public override Advice BeforeCall(object[] args) => Advice.Done;
    }

    class Noop: InvocationInterceptor { }

    [TestMethod]
    public void PreemptCallTest() {
      var it = (new Imp()).AddAspect<It>(_ => new BeforeCallDone());
      Assert.AreEqual(0, it.Jo());
      Assert.AreEqual(default(DateTime), it.Echo(DateTime.Today));
      it = (new Imp()).AddAspect<It>(_ => new Noop());
      Assert.AreEqual(42, it.Jo());
      Assert.AreEqual(DateTime.Today, it.Echo(DateTime.Today));
    }

    [TestMethod]
    public void WeaklyTypedCreationTest() {
      object isIt = Weaver.Create(typeof(It), new Imp(), _ => new BeforeCallDone());
      Assert.IsTrue(isIt is It);
      var it = (It)isIt;
      Assert.AreEqual(0, it.Jo());
      Assert.AreEqual(default(DateTime), it.Echo(DateTime.Today));
      it = (new Imp()).AddAspect<It>(_ => new Noop());
      Assert.AreEqual(42, it.Jo());
      Assert.AreEqual(DateTime.Today, it.Echo(DateTime.Today));
    }

  }
}
