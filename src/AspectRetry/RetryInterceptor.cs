using System;
using System.Collections.Generic;
using System.Text;

namespace AspectWeaver {
  /// <summary>
  /// An <see cref="InvocationInterceptor"/> that adds a retry strategy to method calls.
  /// </summary>
  public class RetryInterceptor: InvocationInterceptor {
    readonly Func<Exception, bool> _retry;
    IEnumerator<TimeSpan> _delays;

    /// <summary>
    /// Constructs a <see cref="RetryInterceptor"/>.
    /// </summary>
    /// <param name="retry">A predicate used to determine whether or not to retry after an exception.</param>
    /// <param name="delays">The delays before each successive retry. 
    /// The number of delays determines the maximum number of retries.</param>
    public RetryInterceptor(Func<Exception, bool> retry, IEnumerator<TimeSpan> delays) {
      _retry = retry;
      _delays = delays;
    }

    /// <inheritdoc />
    public override void Dispose() {
      _delays?.Dispose();
      _delays = null;
    }

    /// <inheritdoc />
    public override Advice OnError(Exception e) {
      if( _delays.MoveNext() && _retry(e) ) {
        return Advice.Retry(_delays.Current);
      }
      return Advice.Proceed;
    }
  }
}
