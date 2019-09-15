using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;

namespace AspectWeaver {
  /// <summary>
  /// Extension methods that weave an aspect into an interface implementation.
  /// </summary>
  public static class LoggingExtensions {

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

  }
}

