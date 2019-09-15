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
    /// <summary>
    /// Asserts that <paramref name="this"/> is not <c>null</c>. If <paramref name="this"/> 
    /// is <c>null</c>, raises an <see cref="ArgumentNullException"/> with either the given <paramref name="parameterName"/>
    /// or else the expected type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="parameterName"></param>
    /// <returns><paramref name="this"/> if not <c>null</c></returns>
    /// <exception cref="ArgumentNullException">If <paramref name="this"/> is null.</exception>
    public static T NotNull<T>(
        [ValidatedNotNull] this T @this, string parameterName = null
    ) where T : class {
      if( @this != null ) { return @this; }
      throw new ArgumentNullException(parameterName ?? $"Parameter of type {typeof(T).Name}");
    }
  }

  /// <summary>
  /// When applied to a parameter, this attribute indicates to code analysis 
  /// that the method is a not-null validator for that parameter.
  /// </summary>
  [AttributeUsage(AttributeTargets.Parameter)]
  public sealed class ValidatedNotNullAttribute: Attribute { }

}
