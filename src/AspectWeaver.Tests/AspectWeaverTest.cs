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

    class Noop: AdviceProvider { }

    [TestMethod]
    public void WeaklyTypedCreationTest() {
      var record = new List<string>();
      var isIt = Weaver.Create(typeof(It), new Imp(), m => new CallRecorder(m,record));
      Assert.IsTrue(isIt is It);
      var it = (It)isIt;
      Assert.AreEqual(42, it.Jo());
      Assert.AreEqual(2, record.Count);
    }

  }
}
