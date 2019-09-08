using System;
using System.Threading;
using System.Threading.Tasks;

namespace AspectWeaver {
  /// <summary>
  /// Advice is returned from methods of the <see cref="InvocationInterceptor"/> and used
  /// by the <see cref="Weaver"/> to determine how to proceed.
  /// </summary>
  public class Advice {

    /// <summary>
    /// Advice that specifies that execution should proceed normally. The value is of this property is
    /// <c>null</c>, but using <c>Advice.Proceed</c> makes the intent explicit.
    /// </summary>
    public static Advice Proceed => null;

    /// <summary>
    /// Advice that specifies that the operation schould be retried after the specified <paramref name="delay"/>.
    /// </summary>
    /// <param name="delay"></param>
    public static Advice Retry(TimeSpan delay = default(TimeSpan)) => new Advice(false, delay);

    /// <summary>
    /// Advice that specifies that the current method or <see cref="Task"/> should return.
    /// </summary>
    public static Advice Done { get; } = new Advice(true);

    readonly Action _ponder = () => { };
    readonly Task _ponderTask = Task.CompletedTask;

    internal Advice(bool isCompleted, TimeSpan delay = default(TimeSpan)) {
      IsCompleted = isCompleted;
      if( delay <= TimeSpan.Zero ) { return; }
      _ponder = () => Thread.Sleep(delay);
      _ponderTask = Task.Delay(delay);
    }

    internal bool IsCompleted { get; }

    internal void Ponder() => _ponder();

    internal Task PonderAsync() => _ponderTask;
  }
}
