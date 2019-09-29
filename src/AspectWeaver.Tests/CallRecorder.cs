using System;
using System.Collections.Generic;
using System.Reflection;

namespace AspectWeaver.Tests {

  class CallRecorder: InvocationInterceptor {
    private readonly MethodInfo _method;
    private readonly string _source = "";
    private readonly ICollection<string> _calls;

    public CallRecorder(MethodInfo method, ICollection<string> calls, string interceptorName = null) {
      _method = method;
      _calls = calls;
      if(string.IsNullOrEmpty(interceptorName)) { return; }
      _source = " in " + interceptorName;
    }

    public override Advice BeforeCall(object[] args) {
      _calls.Add($"{_method.Name} called{_source}");
      return base.BeforeCall(args);
    }

    public override Advice AfterCompletion() {
      _calls.Add($"{_method.Name} completed{_source}");
      return base.AfterCompletion();
    }

    public override Advice AfterCompletion(object result) {
      _calls.Add($"{_method.Name} returned {result}{_source}");
      return base.AfterCompletion();
    }
    public override Advice OnError(Exception e) {
      _calls.Add($"{_method.Name} threw {e.GetType()}{_source}");
      return base.OnError(e);
    }
  }
}
