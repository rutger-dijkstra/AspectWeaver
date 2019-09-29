using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;

namespace AspectWeaver {
  /// <summary>
  /// Extension methods that weave an aspect into an interface implementation.
  /// </summary>
  public static class AspectExtensions {
    /// <summary>
    /// Extends the implementation of an interface with a cross-cutting concern.
    /// </summary>
    /// <typeparam name="T">The interface type.</typeparam>
    /// <param name="target">An implementation of <typeparamref name="T"/>.</param>
    /// <param name="interceptorFactory">A factory method for the <see cref="InvocationInterceptor"/> implementing
    /// the cross-cutting concern.
    /// </param>
    /// <returns></returns>
    public static T AddAspect<T>(
        this T target, Func<MethodInfo, InvocationInterceptor> interceptorFactory
    ) where T : class =>
        Weaver.Create(target, interceptorFactory);

    /// <summary>
    /// Adds an aspect that executes the specified action <paramref name="onReturning"/> just before returning
    /// any function or task result of type <typeparamref name="S"/>.
    /// </summary>
    /// <typeparam name="T">The interface type.</typeparam>
    /// <typeparam name="S">The result type for which <paramref name="onReturning"/> is executed.</typeparam>
    /// <param name="target">An implementation of <typeparamref name="T"/>.</param>
    /// <param name="onReturning">The action to execute.</param>
    /// <returns><paramref name="target"/></returns>
    public static T AddResultAction<T, S>(
        this T target, Action<S> onReturning
    ) where T : class {
      return Weaver.Create(
          target,
          (targetMethod) => new ResultInterceptor<S>(onReturning)
      );
    }
  }
}

