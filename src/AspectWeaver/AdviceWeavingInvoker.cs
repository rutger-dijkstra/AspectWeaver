using System;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectWeaver {
  public class AdviceWeavingInvoker: IMethodInvoker {

    readonly IMethodInvoker _inner;
    readonly Func<MethodInfo, AdviceProvider> _interceptorFactory;

    public AdviceWeavingInvoker(IMethodInvoker inner, Func<MethodInfo, AdviceProvider> interceptorFactory) {
      _interceptorFactory = interceptorFactory;
      _inner = inner;
    }

    public void InvokeAction(MethodInfo targetMethod, object[] args) {
      var interceptor = _interceptorFactory(targetMethod);
      if( interceptor is null ) {
        _inner.InvokeAction(targetMethod, args);
        return;
      }
      using( interceptor ) {
        interceptor?.BeforeCall(args);
        try {
          _inner.InvokeAction(targetMethod, args);
        } catch( Exception e ) {
          interceptor?.OnError(e);
          throw;
        }
        interceptor?.AfterCompletion();
      }
    }

    public S InvokeFunc<S>(MethodInfo targetMethod, object[] args) {
      var interceptor = _interceptorFactory(targetMethod);
      if( interceptor is null ) {
        return _inner.InvokeFunc<S>(targetMethod, args);
      }
      using( interceptor ) {
        S result = default;
        interceptor?.BeforeCall(args);
        try {
          result = _inner.InvokeFunc<S>(targetMethod, args);
        } catch( Exception e ) {
          interceptor?.OnError(e);
          throw;
        }
        interceptor?.AfterCompletion(result);
        return result;
      }
    }

    public Task InvokeActionAsync(MethodInfo targetMethod, object[] args) {
      var interceptor = _interceptorFactory(targetMethod);
      if( interceptor is null ) {
        return _inner.InvokeActionAsync(targetMethod, args);
      }
      return WeavingInvoke();

      async Task WeavingInvoke() {
        using( interceptor ) {
          interceptor?.BeforeCall(args);
          try {
            await _inner.InvokeActionAsync(targetMethod, args);
          } catch( Exception e ) {
            interceptor?.OnError(e);
            throw;
          }
          interceptor?.AfterCompletion();
        }
      }
    }

    public Task<S> InvokeFuncAsync<S>(MethodInfo targetMethod, object[] args) {
      var interceptor = _interceptorFactory(targetMethod);
      if( interceptor is null ) {
        return _inner.InvokeFuncAsync<S>(targetMethod, args);
      }
      return WeavingInvoke();

      async Task<S> WeavingInvoke() {
        using( interceptor ) {
          var result = default(S);
          interceptor.BeforeCall(args);
          try {
            result = await _inner.InvokeFuncAsync<S>(targetMethod, args);
          } catch( Exception e ) {
            interceptor.OnError(e);
            throw;
          }
          interceptor.AfterCompletion(result);
          return result;
        }
      }
    }
  }

}

