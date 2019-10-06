using System;
using System.Collections.Generic;
using System.Reflection;

namespace AspectWeaver.Tests {

  class CallRecorder: AdviceProvider {
    private readonly MethodInfo _method;
    private readonly string _source = "";
    private readonly ICollection<string> _calls;

    public CallRecorder(MethodInfo method, ICollection<string> calls, string interceptorName = null) {
      _method = method;
      _calls = calls;
      if(string.IsNullOrEmpty(interceptorName)) { return; }
      _source = " in " + interceptorName;
    }

    public override void BeforeCall(object[] args) {
      _calls.Add($"{_method.Name} called{_source}");
    }

    public override void AfterCompletion() {
      _calls.Add($"{_method.Name} completed{_source}");
   }

    public override void AfterCompletion(object result) {
      _calls.Add($"{_method.Name} returned {result}{_source}");
    }
    public override void OnError(Exception e) {
      _calls.Add($"{_method.Name} threw {e.GetType()}{_source}");
     }
  }
}
