using System.Reflection;
using System.Threading.Tasks;
using AspectWeaver.Util;

namespace AspectWeaver {
  public class ReflectionInvoker: IMethodInvoker {
    private readonly object _inner;

    public ReflectionInvoker(object inner) {
      _inner = inner;
    }

    public void InvokeAction(MethodInfo targetMethod, object[] args) =>
      targetMethod.TransparentInvoke(_inner, args);

    public Task InvokeActionAsync(MethodInfo targetMethod, object[] args) =>
      (Task)targetMethod.TransparentInvoke(_inner, args);

    public S InvokeFunc<S>(MethodInfo targetMethod, object[] args) =>
      (S)targetMethod.TransparentInvoke(_inner, args);

    public Task<S> InvokeFuncAsync<S>(MethodInfo targetMethod, object[] args) =>
      (Task<S>)targetMethod.TransparentInvoke(_inner, args);
  }

}
