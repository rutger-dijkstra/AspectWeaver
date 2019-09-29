using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AspectWeaver.Util {

  /// <summary>
  /// This class is not intended to be used rom user code.
  /// </summary>
  public class DiscriminatingDispatchProxy: DispatchProxy {

    abstract class ResultTypeResolver {
      public static ResultTypeResolver Create(Type resultType) =>
        (ResultTypeResolver)Activator.CreateInstance(typeof(TypedResolver<>).MakeGenericType(resultType));

      abstract public object InvokeFunc(IMethodInvoker weaver, MethodInfo targetMethod, object[] args);
      abstract public object InvokeFuncAsync(IMethodInvoker weaver, MethodInfo targetMethod, object[] args);

    }

    class TypedResolver<S>: ResultTypeResolver {
      public override object InvokeFunc(IMethodInvoker weaver, MethodInfo targetMethod, object[] args) =>
        weaver.InvokeFunc<S>(targetMethod, args);

      public override object InvokeFuncAsync(IMethodInvoker weaver, MethodInfo targetMethod, object[] args) =>
        weaver.InvokeFuncAsync<S>(targetMethod, args);
    }

    static ConcurrentDictionary<Type, Func<IMethodInvoker, MethodInfo, object[], object>> _invokesByResult =
      new ConcurrentDictionary<Type, Func<IMethodInvoker, MethodInfo, object[], object>>();

    static Func<IMethodInvoker, MethodInfo, object[], object> InvokeMethodForResult(Type returnType) =>
      _invokesByResult.GetOrAdd(returnType, type => {
        if( type == typeof(void) ) {
          return (weaver, method, args) => { weaver.InvokeAction(method, args); return null; };
        }
        if( type == typeof(Task) ) {
          return (weaver, method, args) => weaver.InvokeActionAsync(method, args);
        }
        if( type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>) ) {
          return ResultTypeResolver.Create(type.GetGenericArguments()[0]).InvokeFuncAsync;
        }
        return ResultTypeResolver.Create(type).InvokeFunc;
      });

    internal IMethodInvoker MethodInvoker { get; set; }

    /// <inheritdoc/>
    protected override object Invoke(MethodInfo targetMethod, object[] args) {
      return InvokeMethodForResult(targetMethod.ReturnType)(MethodInvoker, targetMethod, args);
    }

  }
}
