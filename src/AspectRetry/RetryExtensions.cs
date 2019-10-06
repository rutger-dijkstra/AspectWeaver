using System;
using System.Collections.Generic;
using System.Text;
using AspectWeaver;

namespace AspectRetry {
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
      return Weaver.Wrap( target, inner => new RetryInvoker(inner,strategy));
    }
  }
}
