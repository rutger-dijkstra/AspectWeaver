using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AspectWeaver;

namespace AspectRetry {

  class RetryInvoker: IMethodInvoker {

    readonly IMethodInvoker _inner;
    readonly IRetryStrategy _retryStrategy;

    public RetryInvoker(IMethodInvoker inner, IRetryStrategy retryStrategy) {
      _inner = inner;
      _retryStrategy = retryStrategy;
    }

    public void InvokeAction(MethodInfo targetMethod, object[] args) {
      var delays = _retryStrategy.Delays.GetEnumerator();
      for(; ; ) {
        try {
          _inner.InvokeAction(targetMethod, args);
          return;
        } catch( Exception e ) when( WillRetry(e, delays) ) {
          Thread.Sleep(delays.Current);
        }
      }
    }

    public S InvokeFunc<S>(MethodInfo targetMethod, object[] args) {
      var delays = _retryStrategy.Delays.GetEnumerator();
      for(; ; ) {
        try {
          return _inner.InvokeFunc<S>(targetMethod, args);
        } catch( Exception e ) when( WillRetry(e, delays) ) {
          Thread.Sleep(delays.Current);
        }
      }
    }

    public async Task InvokeActionAsync(MethodInfo targetMethod, object[] args) {
      var delays = _retryStrategy.Delays.GetEnumerator();
      for(; ; ) {
        try {
          await _inner.InvokeActionAsync(targetMethod, args);
          return;
        } catch( Exception e ) when( WillRetry(e, delays) ) {
          await Task.Delay(delays.Current);
        }
      }
    }

    public async Task<S> InvokeFuncAsync<S>(MethodInfo targetMethod, object[] args) {
      var delays = _retryStrategy.Delays.GetEnumerator();
      for(; ; ) {
        try {
          return await _inner.InvokeFuncAsync<S>(targetMethod, args);
        } catch( Exception e ) when( WillRetry(e, delays) ) {
          await Task.Delay(delays.Current);
        }
      }
    }

    bool WillRetry(Exception e, IEnumerator<TimeSpan> delays) =>
      delays.MoveNext() && _retryStrategy.ShouldRetry(e);
  }
}

