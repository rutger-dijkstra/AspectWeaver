using System.Reflection;
using System.Threading.Tasks;

namespace AspectWeaver {
  public interface IMethodInvoker {

    void InvokeAction(MethodInfo targetMethod, object[] args);
    Task InvokeActionAsync(MethodInfo targetMethod, object[] args);
    S InvokeFunc<S>(MethodInfo targetMethod, object[] args);
    Task<S> InvokeFuncAsync<S>(MethodInfo targetMethod, object[] args);
  }

}

