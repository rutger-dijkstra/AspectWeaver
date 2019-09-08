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
    /// Adds logging to the implementation of an interface. Method calls, results, and exceptions get logged. 
    /// </summary>
    /// <typeparam name="T">The interface type.</typeparam>
    /// <param name="target">An implementation of <typeparamref name="T"/>.</param>
    /// <param name="logger">The logger to use.</param>
    /// <param name="valueWrapper">An optional function that the <see cref="LoggingInterceptor"/>
    /// can use to turn arguments and results into something readable.
    /// </param>
    /// <returns></returns>
    public static T AddLoggingAspect<T>(
        this T target, IServiceProvider provider, ILoggingAspectConfiguration configuration = null
    ) where T : class =>
        target.AddLoggingAspect(provider.GetRequiredService<ILogger<T>>(), configuration);

    /// <summary>
    /// Adds logging to the implementation of an interface. Method calls, results, and exceptions get logged. 
    /// </summary>
    /// <typeparam name="T">The interface type.</typeparam>
    /// <param name="target">An implementation of <typeparamref name="T"/>.</param>
    /// <param name="logger">The logger to use.</param>
    /// <param name="configuration">Configuration.</param>
    /// <returns></returns>
    public static T AddLoggingAspect<T>(
        this T target, ILogger logger, ILoggingAspectConfiguration configuration = null
    ) where T : class {
      var config = configuration ?? new LoggingAspectConfiguration();
      return Weaver.Create(target, (targetMethod) => config.CreateLoggingInterceptor<T>(targetMethod, logger));
    }

    private static InvocationInterceptor CreateLoggingInterceptor<T>(
        this ILoggingAspectConfiguration config, MethodInfo targetMethod, ILogger logger
    ) =>
        config.IncludeInherited || targetMethod.DeclaringType == typeof(T) ? new LoggingInterceptor(targetMethod, logger, config) : null;

    /// <summary>
    /// Adds a retry strategy to the implementation of an interface.
    /// </summary>
    /// <typeparam name="T">The interface type.</typeparam>
    /// <param name="target">An implementation of <typeparamref name="T"/>.</param>
    /// <param name="strategy">The retry strategy.</param>
    /// <returns></returns>
    public static T AddRetryAspect<T>(
        this T target, IRetryStrategy strategy
    ) where T : class {
      if( strategy == null ) { return target; }
      return Weaver.Create(
          target,
          (targetMethod) => new RetryInterceptor(strategy.ShouldRetry, strategy.Delays.GetEnumerator())
      );
    }

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

