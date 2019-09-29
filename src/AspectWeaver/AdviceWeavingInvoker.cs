using System;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectWeaver {
  public class AdviceWeavingInvoker: IMethodInvoker {

    static Advice AdviceContinue { get; } = new Advice(false);

    readonly IMethodInvoker _inner;
    readonly Func<MethodInfo, InvocationInterceptor> _interceptorFactory;

    public AdviceWeavingInvoker(IMethodInvoker inner, Func<MethodInfo, InvocationInterceptor> interceptorFactory) {
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
        var advice = interceptor.BeforeCall(args) ?? AdviceContinue;
        while( !advice.IsCompleted ) {
          advice.Ponder();
          advice = null;
          try {
            _inner.InvokeAction(targetMethod, args);
          } catch( Exception e ) {
            advice = interceptor.OnError(e);
            if( advice is null ) { throw; }
          }
          advice = advice ?? interceptor.AfterCompletion() ?? Advice.Done;
        }
      }
    }

    public S InvokeFunc<S>(MethodInfo targetMethod, object[] args) {
      var interceptor = _interceptorFactory(targetMethod);
      if( interceptor is null ) {
        return _inner.InvokeFunc<S>(targetMethod, args);
      }
      using( interceptor ) {
        S result = default;
        var advice = interceptor.BeforeCall(args) ?? AdviceContinue;
        while( !advice.IsCompleted ) {
          advice.Ponder();
          advice = null;
          try {
            result = _inner.InvokeFunc<S>(targetMethod, args);
          } catch( Exception e ) {
            advice = interceptor.OnError(e);
            if( advice is null ) { throw; }
          }
          advice = advice ?? interceptor.AfterCompletion(result) ?? Advice.Done;
        }
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
          var advice = interceptor.BeforeCall(args) ?? AdviceContinue;
          while( !advice.IsCompleted ) {
            await advice.PonderAsync();
            advice = null;
            try {
              await _inner.InvokeActionAsync(targetMethod, args);
            } catch( Exception e ) {
              advice = interceptor.OnError(e);
              if( advice is null ) { throw; }
            }
            advice = advice ?? interceptor.AfterCompletion() ?? Advice.Done;
          }
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
          var advice = interceptor.BeforeCall(args) ?? AdviceContinue;
          while( !advice.IsCompleted ) {
            await advice.PonderAsync();
            advice = null;
            try {
              result = await _inner.InvokeFuncAsync<S>(targetMethod, args);
            } catch( Exception e ) {
              advice = interceptor.OnError(e);
              if( advice is null ) { throw; }
            }
            advice = advice ?? interceptor.AfterCompletion(result) ?? Advice.Done;
          }
          return result;
        }
      }
    }
  }

}

