using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;

namespace AspectWeaver.Util {
  static class Extensions {
    /// <summary>
    /// Just <see cref="MethodBase.Invoke(object, object[])"/>, but leaves exceptions as is instead of 
    /// wrapping them in a <see cref="TargetInvocationException"/>.
    /// </summary>
    /// <param name="targetMethod"></param>
    /// <param name="target"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static object TransparentInvoke(this MethodBase targetMethod, object target, object[] args) {
      try {
        return targetMethod.Invoke(target, args);
      } catch( TargetInvocationException e ) {
        e.InnerException.Throw();
        throw; //prevent CS0161 'not all code paths return a value' compiler error
      }
    }

    /// <summary>
    /// Throws <paramref name="exception"/>, preserving the stack trace, if any.
    /// </summary>
    public static void Throw(this Exception exception) {
      if( !string.IsNullOrEmpty(exception.StackTrace) ) {
        ExceptionDispatchInfo.Capture(exception).Throw();
      }
      throw exception;
    }
  }
}
