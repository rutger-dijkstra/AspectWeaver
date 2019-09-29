using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspectWeaver.Tests {
  [TestClass]
  public partial class AspectStackingTest {

    interface It {
      void Jo();
    }
    class Imp: It {
      public void Jo() { }
    }

    [TestMethod]
    public void TestAspectStacking() {
      var calls = new List<string>();
      var it = (new Imp() as It)
        .AddAspect(_ => new CallRecorder(_, calls, "inner"))
        .AddAspect(_ => new CallRecorder(_, calls, "outer"));
      it.Jo();
      Assert.AreEqual(
        "Jo called in outer, Jo called in inner, Jo completed in inner, Jo completed in outer",
        string.Join(", ", calls)
      );
    }

  }
}
