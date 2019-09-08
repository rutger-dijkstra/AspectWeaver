using System;
using System.Collections.Generic;
using System.Linq;
using TLN.Platform.GeneralNonsense;

namespace AspectWeaver {

  /// <summary>
  /// The interface of <see cref="RetryStrategy"/>.
  /// </summary>
  public interface IRetryStrategy {

    /// <summary>
    /// The sequence of delays that should be inserted before each successive retry.
    /// </summary>
    IEnumerable<TimeSpan> Delays { get; }

    /// <summary>
    /// The predicate that determines whether or not a retry should take place after an exception. 
    /// </summary>
    /// <param name="exception">The exception that just occurred.</param>
    /// <returns><c>true</c> to retry.</returns>
    bool ShouldRetry(Exception exception);
  }

  /// <summary>
  /// A retry strategy that is earmarked for dependency injection in a specific context.
  /// </summary>
  /// <typeparam name="TDiscriminator">A context discriminator that enables a DI-container to
  /// inject different strategies in different contexts.</typeparam>
  public class RetryStrategy<TDiscriminator>: IRetryStrategy {

    private readonly List<Func<Exception, bool>> _conditions = new List<Func<Exception, bool>>();

    /// <summary>
    /// Constructs a <see cref="RetryStrategy{TDiscriminator}"/> with the specified delays.
    /// </summary>
    public RetryStrategy(IEnumerable<TimeSpan> delays) {
      Delays = delays.OrEmpty().ToList();
    }

    /// <summary>
    /// Constructs a <see cref="RetryStrategy{TDiscriminator}"/> with the specified delays.
    /// </summary>
    public RetryStrategy(params TimeSpan[] delays) {
      Delays = delays.OrEmpty();
    }

    /// <summary>
    /// Constructs a <see cref="RetryStrategy{TDiscriminator}"/> with the specified milliseconds delays.
    /// </summary>
    public RetryStrategy(params int[] delays) :
        this(delays?.Select(ms => TimeSpan.FromMilliseconds(ms))) {
    }

    /// <summary>
    /// The sequence of delays that should be inserted before each successive retry.
    /// </summary>
    public IEnumerable<TimeSpan> Delays { get; }

    /// <summary>
    /// Whether or not a retry should occurr after the specified <paramref name="exception"/>.
    /// </summary>
    public bool ShouldRetry(Exception exception) =>
        !_conditions.Any() || _conditions.Any(shouldRetry => shouldRetry(exception));


    /// <summary>
    /// Adds handling of exceptions of the specified <typeparamref name="TException"/> type to
    /// the strategy. If the predicate <paramref name="shouldRetry"/> is not specified, all
    /// exceptions of type <typeparamref name="TDiscriminator"/> will be handled.
    /// 
    /// If no specific <see cref="Handle{TException}(Func{TException, bool})"/> clauses are
    /// specified for the strategy, all exceptions will be handled.
    /// </summary>
    public RetryStrategy<TDiscriminator> Handle<TException>(
        Func<TException, bool> shouldRetry = null
     ) where TException : Exception {
      if( shouldRetry is null ) {
        _conditions.Add(exception => exception is TException);
      } else {
        _conditions.Add(exception => exception is TException e && shouldRetry(e));
      }
      return this;
    }
  }

  /// <summary>
  /// A <see cref="RetryStrategy"/> that is not earmarked for a specific context.
  /// </summary>
  public class RetryStrategy: RetryStrategy<object> {

    /// <summary>
    /// Constructs a <see cref="RetryStrategy"/> with the specified delays.
    /// </summary>
    public RetryStrategy(IEnumerable<TimeSpan> delays) : base(delays) { }

    /// <summary>
    /// Constructs a <see cref="RetryStrategy"/> with the specified delays.
    /// </summary>
    public RetryStrategy(params TimeSpan[] delays) : base(delays) { }

    /// <summary>
    /// Constructs a <see cref="RetryStrategy"/> with the specified milliseconds delays.
    /// </summary>
    public RetryStrategy(params int[] delays) : base(delays) { }

  }


}
