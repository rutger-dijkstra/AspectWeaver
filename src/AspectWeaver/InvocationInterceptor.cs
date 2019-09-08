using System;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectWeaver {
  /// <summary>
  /// This is the base class for interceptors that are used by the <see cref="Weaver"/>
  /// to inject orthogonal concerns into method invocations. Implementations exist for implementing 
  /// a retry policy and for logging. 
  /// 
  /// Create you own subclass to realise your own implementation of an orthogonal concern.
  /// One instance of your interceptor will be created for each method invocation and it will
  /// be disposed upon completion (whether successful or exceptional). 
  /// </summary>
  public abstract class InvocationInterceptor: IDisposable {
    /// <summary>
    /// This method will be called exactly once, before the invocation of the intercepted method. 
    /// Returning <see cref="Advice.Proceed"/> from this method, will result in the intercepted method being 
    /// executed as normal. In order to prevent the intercepted method being executed, 
    /// either raise an exception, which will be the result of the invocation, or return <see cref="Advice.Done"/> 
    /// to have the invocation return the default value of the expected return type.
    /// 
    /// The inherited implementation does nothing and returns <see cref="Advice.Proceed"/>. 
    /// </summary>
    /// <param name="args">The arguments passed to the method invocation.</param>
    public virtual Advice BeforeCall(object[] args) => Advice.Proceed;

    /// <summary>
    /// For intercepted methods not returning a result (return type <see cref="void"/>), this
    /// method is injected just after successful completion. For intercepted method returning a
    /// <see cref="Task"/>, this method is injected after successful completion <em>of the task</em>.
    ///
    /// Returning <see cref="Advice.Proceed"/> or <see cref="Advice.Done"/> will result in the 
    /// intercepted method or task terminating as normal. As there is no result to inspect, there
    /// is little basis for raising exceptions or instigating retries.
    ///  
    /// The inherited implementation does nothing and returns <see cref="Advice.Proceed"/>. 
    /// </summary>
    public virtual Advice AfterCompletion() => Advice.Proceed;

    /// <summary>
    /// For intercepted methods returning a result that is not a <see cref="Task"/> or subclass thereof, this
    /// method is injected just after successful completion. For intercepted method returning a
    /// <see cref="Task{TResult}"/>, this method is injected after successful completion <em>of the task</em>.
    ///
    /// Returning <see cref="Advice.Proceed"/> or <see cref="Advice.Done"/> will result in the 
    /// intercepted method or task terminating as normal and return the <paramref name="result"/>. 
    /// Based on the obtained <paramref name="result"/>, one can also raise an exception or instigate a retry
    /// after, possibly, some delay by returning <see cref="Advice.Retry(TimeSpan)"/>.
    /// 
    /// The inherited implementation does nothing and returns <see cref="Advice.Proceed"/>. 
    /// </summary>
    public virtual Advice AfterCompletion(object result) => Advice.Proceed;

    /// <summary>
    /// This method is injected when the intercepted method or the resulting 
    /// <see cref="Task"/> encounter an exception. Note that exception raised from any of the
    /// other methods from this injector terminate the invocation without 
    /// calling <see cref="OnError(Exception)"/> .
    ///
    /// Returning <see cref="Advice.Proceed"/> will result in the exception being raised as normal.
    /// Returning <see cref="Advice.Done"/> will result in the invocation terminating, 
    /// yielding the default value of the expected type as result. Returning <see cref="Advice.Retry(TimeSpan)"/>
    /// instigates a retry after the specified delay. One can also replace the exception by throwing one of
    /// your own.
    /// 
    /// The inherited implementation does nothing and returns <see cref="Advice.Proceed"/>. 
    /// </summary>
    public virtual Advice OnError(Exception e) => Advice.Proceed;

    /// <summary>
    /// Called once the entire invocation, including all retries, is finished.
    /// </summary>
    public virtual void Dispose() { }
  }
}
