using System;
using System.Collections.Generic;
using System.Text;
using TLN.Platform.GeneralNonsense;

namespace AspectWeaver {
  class ResultInterceptor<S>: InvocationInterceptor {
    private readonly Action<S> _onCompleted;

    public ResultInterceptor(Action<S> onCompleted) {
      _onCompleted = onCompleted.NotNull();
    }

    public override Advice AfterCompletion(object result) {
      if( result is S sResult ) {
        _onCompleted(sResult);
      }
      return base.AfterCompletion(result);
    }
  }
}
