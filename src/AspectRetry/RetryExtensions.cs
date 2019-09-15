using System;
using System.Collections.Generic;
using System.Text;

namespace AspectWeaver {
  public static class RetryExtensions {
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
  }
}
