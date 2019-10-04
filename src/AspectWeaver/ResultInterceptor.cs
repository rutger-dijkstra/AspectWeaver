using System;
using System.Collections.Generic;
using System.Text;
using AspectWeaver.Util;

namespace AspectWeaver {
  class ResultInterceptor<S>: AdviceProvider {
    private readonly Action<S> _onCompleted;

    public ResultInterceptor(Action<S> onCompleted) {
      _onCompleted = onCompleted.NotNull();
    }

    public override void AfterCompletion(object result) {
      if( result is S sResult ) {
        _onCompleted(sResult);
      }
    }
  }
}
